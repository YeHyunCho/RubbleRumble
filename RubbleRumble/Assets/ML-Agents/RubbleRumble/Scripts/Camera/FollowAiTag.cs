using UnityEngine;

public class FollowAiTag : MonoBehaviour
{
    [Header("타겟 설정")]
    [SerializeField] // private 이지만 인스펙터에는 노출
    private string targetTag = "Ai"; // 따라갈 게임 오브젝트의 태그
    private Transform target; // 찾아낸 타겟의 Transform 컴포넌트

    [Header("카메라 위치")]
    public Vector3 offset = new Vector3(0f, 5f, -10f); // 타겟으로부터 유지할 거리와 각도 (인스펙터에서 조절)

    [Header("부드러운 움직임")]
    [Range(0.01f, 1.0f)] // smoothSpeed 값 범위를 0.01 ~ 1.0 으로 제한
    public float smoothSpeed = 0.125f; // 카메라 이동 부드러움 정도 (값이 낮을수록 부드럽지만 느리게 따라감)

    [Header("타겟 바라보기")]
    public bool lookAtTarget = true; // 카메라가 항상 타겟을 바라보게 할지 여부

    // --- 부드러운 움직임 SmoothDamp 방식 (선택 사항) ---
    // public float smoothTime = 0.3f;
    // private Vector3 velocity = Vector3.zero;
    // -----------------------------------------------

    // 게임 시작 시 타겟을 찾습니다.
    void Start()
    {
        FindTarget();
    }

    // 모든 Update 로직이 끝난 후, 프레임 마지막에 호출됩니다. (카메라 이동에 적합)
    void LateUpdate()
    {
        // target 변수가 비어있는지(null) 확인합니다. (타겟이 파괴되었을 수도 있음)
        if (target == null)
        {
            // 여기서 타겟을 다시 찾도록 시도할 수도 있습니다.
            // FindTarget(); // 주석 해제하면 타겟을 잃었을 때 계속 다시 찾으려고 시도함

            // 다시 찾아도 타겟이 없다면 더 이상 진행하지 않습니다.
            if (target == null)
            {
                // Debug.LogWarning($"태그 '{targetTag}'를 가진 오브젝트를 찾을 수 없습니다. 카메라 추적 중지.");
                return; // 타겟 없으면 함수 종료
            }
        }

        // 원하는 카메라 위치 계산 (타겟 위치 + 오프셋)
        Vector3 desiredPosition = target.position + offset;

        // 현재 카메라 위치에서 원하는 위치로 부드럽게 이동 (Lerp 사용)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition; // 계산된 위치로 카메라 이동

        // --- SmoothDamp 방식 이동 (선택 사항) ---
        // transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        // ---------------------------------------

        // 카메라가 타겟을 바라보도록 설정 (선택 사항)
        if (lookAtTarget)
        {
            transform.LookAt(target);
            // 필요하다면 LookAt 회전에도 부드러움을 추가할 수 있습니다 (조금 더 복잡함)
        }
    }

    // 지정된 태그로 타겟 게임 오브젝트를 찾는 함수
    void FindTarget()
    {
        GameObject targetObject = GameObject.FindWithTag(targetTag);
        if (targetObject != null)
        {
            // 찾았다면 target 변수에 Transform 컴포넌트를 저장
            target = targetObject.transform;
            Debug.Log($"카메라가 '{targetTag}' 태그를 가진 타겟 '{target.name}'을 찾았습니다.");
        }
        else
        {
            // 못 찾았다면 target 변수를 null로 유지
            target = null;
            // Debug.LogWarning($"'{targetTag}' 태그를 가진 오브젝트를 Start에서 찾지 못했습니다.");
        }
    }
}