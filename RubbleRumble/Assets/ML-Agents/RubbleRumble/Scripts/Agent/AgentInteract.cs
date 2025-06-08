using UnityEngine;

// 상호작용 UI 상태를 열거형으로 정의
// public enum InteractUIState
// {
//     None = 0,           // 비활성화 상태
//     PressE = 1,         // E키 누르기 (잡기, 버리기 등)
//     PressQ = 2,         // Q키 누르기 (재활용 시작, 세척 시작 등)
//     Holding = 3         // 진행 휠바퀴 표시 (재활용중, 세척중)
// }

public class AgentInteract : MonoBehaviour
{
    [SerializeField] private float interactRange;       // 상호작용 탐지 범위
    [SerializeField] private LayerMask pickupLayerMask; // 상호작용 가능한 레이어 마스크(pickable로 설정)

    //[SerializeField] private agentInputHandler agentInputHandler;
    //[SerializeField] private AgentHand agentHand;

    [SerializeField] private AgentInputHandler agentInputHandler;
    [SerializeField] private Mop mop;
    public InteractUIState CurrentUIState { get; private set; }

    private void Awake()
    {
        interactRange = 3;
        agentInputHandler = GetComponent<AgentInputHandler>();
    }

    private void Update()
    {
        UpdateInteractUIState();
    }

    // 모든 상호작용 조건을 체크하고 최종 UI 상태를 결정하는 메서드
    private void UpdateInteractUIState()
    {
        // 기본 상태: 비활성화
        InteractUIState newState = InteractUIState.None;

        // 대걸레 사용 관련 상태 체크
        if (agentInputHandler.GetCurrentTool() == 2) // 도구 인덱스 2가 대걸레인 경우
        {
            newState = CheckMopInteract();
            CurrentUIState = newState;
            return;
        }

        // 빈 손으로 물건을 집을 수 있는 상태 체크
        if (agentInputHandler.GetCurrentTool() == 0 && !agentInputHandler.GetIsHoldingTrash())
        {
            newState = CheckHandInteract();
            if (newState != InteractUIState.None)
            {
                CurrentUIState = newState;
                return;
            }
        }

        // 쓰레기를 들고 있는 상태에서의 상호작용 체크
        //if (agentInputHandler.GetCurrentTool() == 0 && agentInputHandler.GetIsHoldingTrash() && agentInputHandler.GetHeldObject() != null)
        if (agentInputHandler.GetCurrentTool() == 0 && agentInputHandler.GetIsHoldingTrash())
        {
            newState = CheckTrashInteract();
            if (newState != InteractUIState.None)
            {
                CurrentUIState = newState;
                return;
            }
        }

        // 작업대 근처에서의 상호작용 체크
        if (agentInputHandler.GetIsNearWorkbench() && !agentInputHandler.GetIsHoldingTrash())
        {
            newState = CheckWorkbenchInteract();
        }

        CurrentUIState = newState;
    }

    // 대걸레 관련 상호작용 체크
    private InteractUIState CheckMopInteract()
    {
        if (mop == null) mop = FindObjectOfType<Mop>();
        AgentInputHandler agentInputHandler = FindObjectOfType<AgentInputHandler>();

       // agentInputHandler의 nearDust 사용
        if (agentInputHandler.GetReadyToClean() && mop.GetUseCount() < 2)
        {
            return InteractUIState.PressE;
        }

        // 개수대에서 대걸레 세척 관련 체크
        float sinkDistance = Vector3.Distance(mop.transform.position, mop.sink.transform.position);
        if (sinkDistance <= mop.triggerDistance && mop.GetUseCount() >= 2)
        {
            if (agentInputHandler.GetQPressed() && mop.GetHoldingTime() > 0f && mop.GetHoldingTime() < 2f)
            {
                return InteractUIState.Holding; // 홀딩바 활성화
            }

            return InteractUIState.PressQ; // 상호작용 Q 활성화 (세척)
        }

        return InteractUIState.None;
    }

    // 빈 손일 때 상호작용 체크
    private InteractUIState CheckHandInteract()
    {
        Ray ray = new Ray(transform.position + Vector3.up, transform.forward * interactRange);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange, pickupLayerMask))
        {
            // 집기 가능한 오브젝트 확인
            if (hit.collider.CompareTag("Can") || hit.collider.CompareTag("Box") || hit.collider.CompareTag("UnfoldedBox"))
            {
                return InteractUIState.PressE; // 상호작용 E 활성화
            }
        }
        return InteractUIState.None;
    }

    // 쓰레기를 들고 있을 때 상호작용 체크
    private InteractUIState CheckTrashInteract()
    {
        GameObject heldObject = agentInputHandler.GetHeldObject();

        // 분리수거장 근처에서의 상호작용
        if (agentInputHandler.GetIsNearRecyclingBin())
        {
            if (heldObject.CompareTag("UnfoldedBox") || heldObject.CompareTag("Can"))
            {
                return InteractUIState.PressE; // 상호작용 E 활성화 (버리기)
            }

            return InteractUIState.None; // Box 태그는 버릴 수 없음
        }

        // 작업대 근처에서의 상호작용
        if (agentInputHandler.GetIsNearWorkbench() && agentInputHandler.GetTrashOnWorkbench() != null)
        {
            if (agentInputHandler.GetIsUnfolding())
            {
                return InteractUIState.Holding; // 홀딩바 활성화
            }

            if (agentInputHandler.GetTrashOnWorkbench() == null) // 작업대가 비어있을 때만
            {
                return InteractUIState.PressQ; // 상호작용 Q 활성화 (놓기)
            }
        }

        return InteractUIState.None;
    }

    // 작업대 근처에서의 상호작용 체크
    private InteractUIState CheckWorkbenchInteract()
    {
        GameObject trashOnWorkbench = agentInputHandler.GetTrashOnWorkbench();

        if (trashOnWorkbench == null)
        {
            return InteractUIState.None;
        }

        if (trashOnWorkbench.CompareTag("Box") && !agentInputHandler.GetIsUnfolding())
        {
            if (agentInputHandler.GetQPressed())
            {
                return InteractUIState.Holding; // 홀딩바 활성화
            }

            return InteractUIState.PressQ; // 상호작용 Q 활성화 (재활용 시작)
        }

        if (trashOnWorkbench.CompareTag("UnfoldedBox"))
        {
            return InteractUIState.PressE; // 상호작용 E 활성화 (집기)
        }

        return InteractUIState.None;
    }
}