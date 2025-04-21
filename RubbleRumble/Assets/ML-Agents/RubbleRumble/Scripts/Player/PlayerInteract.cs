using UnityEditor.EditorTools;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private float interactRange;       // 쓰레기 탐지 범위
    [SerializeField] private LayerMask pickupLayerMask; // 상호작용 가능한 레이어 마스크(pickable로 설정)

    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerHand playerHand;
    [SerializeField] private Mop mop;
    public int InteractUIState { get; private set; }

    private void Awake()
    {
        interactRange = 3;
    }

    private void Update()
    {
        UpdateInteractUIState();
    }

    // 모든 상호작용 조건을 체크하고 최종 UI 상태를 결정하는 메서드
    private void UpdateInteractUIState()
    {
        int newState = 0; // 기본 상태: 비활성화

        // 플레이어가 맨손일 때 raycast로 'Pickable' layer를 탐지하면 상호작용 E 활성화
        if (ToolManager.Instance.currentTool == 0 && !playerController.GetIsHoldingTrash())
        {
            Ray ray = new Ray(transform.position + Vector3.up, transform.forward * interactRange);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactRange, pickupLayerMask))
            {
                // 줍기 가능한 오브젝트 확인
                if (hit.collider.CompareTag("Can") || hit.collider.CompareTag("Box") || hit.collider.CompareTag("UnfoldedBox"))
                {
                    newState = 1; // 상호작용 E 활성화
                }
            }
        }

        // 맨손으로 쓰레기를 들고 있는 상태
        if (ToolManager.Instance.currentTool == 0 && playerController.GetIsHoldingTrash() && playerController.GetHeldObject() != null)
        {
            // 쓰레기를 들고 있는 상태에서 recyclingBin 근처에 있으면 상호작용 E 활성화
            if (playerController.GetIsNearRecyclingBin())
            {
                if (playerController.GetHeldObject().CompareTag("UnfoldedBox") || playerController.GetHeldObject().CompareTag("Can"))
                {
                    newState = 1; // 상호작용 E 활성화 (버리기)
                }
                // Box 태그는 버릴 수 없음 (상태 0 유지)
            }

            // Box 태그인 오브젝트를 들고 있는 상태에서 workbench에 근처에 있으면 상호작용 Q 활성화
            else if (playerController.GetIsNearWorkbench() && playerController.GetHeldObject().CompareTag("Box"))
            {
                if (playerController.GetIsUnfolding())
                {
                    newState = 3; // 홀딩바 활성화
                }
                else if (playerController.GetTrashOnWorkbench() == null) // 작업대가 비어있을 때만
                {
                    newState = 2; // 상호작용 Q 활성화 (놓기)
                }
            }
        }

        // workbench에 trigger된 상태에서 Q키를 누르고 있으면 홀딩바 활성화
        // workbench에 trigger된 상태에서 workbench 위에 tag가 unfoldedbox인 오브젝트가 있으면 상호작용 E 활성화
        if (playerController.GetIsNearWorkbench() && !playerController.GetIsHoldingTrash())
        {
            if (playerController.GetTrashOnWorkbench() != null)
            {
                if (playerController.GetTrashOnWorkbench().CompareTag("Box") && !playerController.GetIsUnfolding())
                {
                    if (Input.GetKey(KeyCode.Q))
                    {
                        newState = 3; // 홀딩바 활성화
                    }
                    else
                    {
                        newState = 2; // 상호작용 Q 활성화 (펴기 시작)
                    }
                }
                else if (playerController.GetTrashOnWorkbench().CompareTag("UnfoldedBox"))
                {
                    newState = 1; // 상호작용 E 활성화 (집기)
                }
            }
        }

        // mop을 들고 있는 상태에서 dust가 mop에 trigger되고 가용 횟수를 넘지 않은 상태이면 상호작용 E 활성화
        if (ToolManager.Instance.currentTool == 2) // 도구 인덱스 2가 대걸레인 경우
        {
            if (mop == null) mop = FindObjectOfType<Mop>(); // mop 참조 연결
            if (mop.GetNearDust() != null && mop.GetUseCount() < 2)
            {
                newState = 1; // 상호작용 E 활성화 (닦기)
            }

            // mop을 들고 있는 상태에서 sink 근처에 있고 mop이 최대 가용 횟수인 상태이면 상호작용 Q 활성화
            // mop을 들고 있는 상태에서 sink 근처에 있고 mop이 최대 가용 횟수인 상태이고 Q 키를 누르고 있으면 홀딩바 활성화
            float sinkDistance = Vector3.Distance(mop.transform.position, mop.sink.transform.position);
            if (sinkDistance <= mop.triggerDistance && mop.GetUseCount() >= 2)
            {
                if (Input.GetKey(KeyCode.Q) && mop.GetHoldingTime() > 0f && mop.GetHoldingTime() < 2f)
                {
                    newState = 3; // 홀딩바 활성화
                }
                else
                {
                    newState = 2; // 상호작용 Q 활성화 (씻기)
                }
            }
        }

        InteractUIState = newState;
    }
}
