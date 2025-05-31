using UnityEditor.EditorTools;
using UnityEngine;

// 상호작용 UI 상태를 열거형으로 정의
public enum InteractUIState
{
    None = 0,           // 비활성화 상태
    PressE = 1,         // E키 누르기 (잡기, 버리기 등)
    PressQ = 2,         // Q키 누르기 (재활용 시작, 세척 시작 등)
    Holding = 3         // 진행도 표시 (재활용중, 세척중)
}

public class PlayerInteract : MonoBehaviour
{
    //[SerializeField] private float interactRange;       // 상호작용 탐지 범위
    //[SerializeField] private LayerMask pickupLayerMask; // 상호작용 가능한 레이어 마스크(pickable로 설정)

    [SerializeField] private PlayerInputHandler playerInputHandler;
    public Mop mop { get; private set; }
    public InteractUIState CurrentUIState { get; private set; }

    private void Awake()
    {
        //interactRange = 3;
    }

    private void Start()
    {
        if (playerInputHandler == null)
            playerInputHandler = GetComponent<PlayerInputHandler>();
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
        if (playerInputHandler.GetCurrentTool() == 2) // 도구 인덱스 2가 대걸레인 경우
        {
            newState = CheckMopInteract();
            CurrentUIState = newState;
            return;
        }

        // 작업대 근처에서의 상호작용 체크
        if (playerInputHandler.GetIsNearWorkbench())
        {
            newState = CheckWorkbenchInteract();
            CurrentUIState = newState;
            return;
        }

        // 빈 손으로 물건을 집을 수 있는 상태 체크
        if (playerInputHandler.GetCurrentTool() == 0 && !playerInputHandler.GetIsHoldingTrash())
        {
            newState = CheckHandInteract();
            CurrentUIState = newState;
            return;
        }

        // 쓰레기를 들고 있는 상태에서의 상호작용 체크
        if (playerInputHandler.GetCurrentTool() == 0 && playerInputHandler.GetIsHoldingTrash())
        {
            newState = CheckTrashInteract();
            CurrentUIState = newState;
            return;
        }

        CurrentUIState = newState;
    }

    // 대걸레 관련 상호작용 체크
    private InteractUIState CheckMopInteract()
    {
        // 대걸레 참조 연결
        if (mop == null)
        {
            Mop[] mops = FindObjectsOfType<Mop>();
            if (mops[0].isPlayer) { mop = mops[0]; }
            else { mop = mops[1]; }
        }

        // 개수대에서 대걸레 세척 관련 체크
        float sinkDistance = Vector3.Distance(mop.transform.position, mop.sink.transform.position);
        if (sinkDistance <= mop.triggerDistance && mop.GetUseCount() >= 2)
        {
            if (Input.GetKey(KeyCode.Q) && mop.GetHoldingTime() > 0f && mop.GetHoldingTime() < 2f)
            {
                return InteractUIState.Holding; // 홀딩바 활성화
            }

            return InteractUIState.PressQ; // 상호작용 Q 활성화 (세척)
        }

        // 플레이어 근처에 먼지가 있고, 대걸레 사용할 수 있으면 상호작용 E 활성화 
        if (playerInputHandler.GetIsNearObject())
        {
            if (playerInputHandler.GetNearObject().CompareTag("Dust") && mop.GetUseCount() < 2)
            {
                return InteractUIState.PressE;
            }
        }

        return InteractUIState.None;
    }

    // 빈 손일 때 상호작용 체크
    private InteractUIState CheckHandInteract()
    {
        //Ray ray = new Ray(transform.position + Vector3.up, transform.forward * interactRange);
        //RaycastHit hit;

        //if (Physics.Raycast(ray, out hit, interactRange, pickupLayerMask))
        //{
        //    // 집기 가능한 오브젝트 확인
        //    if (hit.collider.CompareTag("Can") || hit.collider.CompareTag("Box") || hit.collider.CompareTag("UnfoldedBox"))
        //        return InteractUIState.PressE; // 상호작용 E 활성화
        //}

        GameObject nearObject = playerInputHandler.GetNearObject();
        if (nearObject != null)
        {
            if (nearObject.CompareTag("Can") || nearObject.CompareTag("Box") || nearObject.CompareTag("UnfoldedBox"))
            {
                return InteractUIState.PressE;
            }
        }

        return InteractUIState.None;
    }

    // 쓰레기를 들고 있을 때 상호작용 체크
    private InteractUIState CheckTrashInteract()
    {
        GameObject heldObject = playerInputHandler.GetHeldObject();
        TrashManager heldTrash = heldObject.GetComponent<TrashManager>();

        // 분리수거장 근처에서의 상호작용
        if (playerInputHandler.GetIsNearRecyclingBin())
        {
            // 들고 있는 쓰레기를 버릴 수 있고 버릴 수 있는 쓰레기통이 근처에 있으면
            if (heldTrash.trashData.readyToThrowAway && playerInputHandler.GetCurRecycleBin().CompareTag(heldTrash.trashData.trashbin))
            {
                return InteractUIState.PressE; // 상호작용 E 활성화 (버리기)
            }

            return InteractUIState.None; // Box 태그는 버릴 수 없음
        }

        return InteractUIState.None;
    }

    // 작업대 근처에서의 상호작용 체크
    private InteractUIState CheckWorkbenchInteract()
    {
        // 손에 쓰레기를 들고 있는 경우
        if (playerInputHandler.GetHeldObject() != null)
        {
            if (!playerInputHandler.GetHeldObject().CompareTag("Box"))  // 들고 있는 오브젝트가 박스가 아니면
            {
                return InteractUIState.None;    // 상호작용 비활성화
            }
            else // 들고 있는 오브젝트가 박스이면
            {
                return InteractUIState.PressQ;    // 상호작용 Q활성화 (작업대 위에 올리기)
            }
        }
        
        GameObject trashOnWorkbench = playerInputHandler.GetTrashOnWorkbench();

        if (trashOnWorkbench == null)   // 작업대 위에 쓰레기가 없으면
        {
            return InteractUIState.None;    // 상호작용 비활성화
        }

        if (trashOnWorkbench.CompareTag("Box")) // 작업대 위에 상자가 있으면
        {
            if (Input.GetKey(KeyCode.Q))
            {
                return InteractUIState.Holding; // 홀딩바 활성화
            }

            return InteractUIState.PressQ; // 상호작용 Q 활성화 (재활용 시작)
        }

        if (trashOnWorkbench.CompareTag("UnfoldedBox")) // 작업대 위에 펼쳐진 상자가 있으면
        {
            return InteractUIState.PressE; // 상호작용 E 활성화 (집기)
        }

        return InteractUIState.None;
    }
}