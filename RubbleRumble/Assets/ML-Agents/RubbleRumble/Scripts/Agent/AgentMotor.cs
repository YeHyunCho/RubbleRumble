using UnityEngine;
using System.Collections;

public class AgentMotor : MonoBehaviour 
{
    public float moveSpeed = 5.0f;           // 캐릭터의 기본 이동 속도
    public float runSpeedMultiplier = 1.8f;  // 달리기 시 이동 속도에 곱해질 배율
    public float rotationSpeed = 10f;        // 캐릭터의 기본 회전 속도 (초당 각도 변화)
    public float runRotationSpeedMultiplier = 1.5f; // 달리기 시 회전 속도에 곱해질 배율

    public float speedChangeSmoothTime = 0.1f; // 애니메이터의 'speed' 파라미터 변경 시 부드러운 전환에 걸리는 시간

    public ParticleSystem dustEffect;          // 달릴 때 바닥에서 발생하는 먼지 파티클 효과

    // 입력값 저장을 위한 내부 변수
    private float hAxis;                       // 수평축(Horizontal) 입력값 (좌우: A, D 또는 화살표 좌우)
    private float vAxis;                       // 수직축(Vertical) 입력값 (상하: W, S 또는 화살표 위아래)
    private bool isShiftDown;                  // 왼쪽 또는 오른쪽 Shift 키가 눌렸는지 여부 (달리기 판단용)

    // 자주 사용될 컴포넌트 참조를 위한 변수
    private Animator animator;                 // 캐릭터 애니메이션을 제어하는 Animator 컴포넌트
    private Rigidbody rb;                      // 캐릭터의 물리적 움직임을 담당하는 Rigidbody 컴포넌트

    // 캐릭터의 내부 상태 및 계산에 사용될 변수
    private Vector3 moveDirectionInput;        // 사용자의 입력에 따른 순수 이동 방향 벡터 (정규화됨)
    private float currentAnimatorSpeed;        // 애니메이터에 현재 프레임에 전달할 최종 속도 값
    private float animatorSpeedVelocity;       // Mathf.SmoothDamp 함수 내부에서 사용되는 참조 변수 (속도 변화량 추적)
    private bool isDustPlaying = false;        // 먼지 효과가 현재 재생 중인지 나타내는 플래그
    private Quaternion targetBodyRotation;     // 캐릭터가 바라봐야 할 목표 Y축 회전값

    // 회전의 안정성을 위해, 이 값보다 작은 입력 크기는 무시 (제곱된 크기 비교용)
    private const float MinInputSqrMagnitudeForRotation = 0.01f * 0.01f; // 0.01의 제곱, 매우 작은 입력 무시

    // 스크립트가 처음 활성화될 때 또는 게임 오브젝트가 처음 생성될 때 1회 호출
    void Awake()
    {
        // 필요한 컴포넌트들을 찾아와서 변수에 할당
        animator = GetComponentInChildren<Animator>(); // 자식 오브젝트에서 Animator 컴포넌트 검색
        rb = GetComponent<Rigidbody>();                // 현재 게임 오브젝트에서 Rigidbody 컴포넌트 검색

        // Rigidbody 컴포넌트가 정상적으로 할당되었는지 확인
        if (rb != null)
        {
            // 물리 엔진에 의해 캐릭터가 X축이나 Z축으로 넘어지지 않도록 회전을 제한
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            // 점프를 사용하지 않고 캐릭터가 Y축(수직)으로 움직이지 않도록 하려면,
            // Rigidbody의 'Use Gravity'(중력 사용) 옵션을 false로 설정하는 것이 좋습니다.
            // 아래 코드의 주석을 해제하거나, 유니티 인스펙터에서 직접 Rigidbody 컴포넌트의 Use Gravity를 체크 해제하세요.
            // rb.useGravity = false;
        }
        else
        {
            // Rigidbody 컴포넌트가 없다면 오류 메시지 출력
            Debug.LogError("AgentMotor: Rigidbody 컴포넌트를 찾을 수 없습니다! 캐릭터 이동에 필수적입니다.");
        }

        // Animator 컴포넌트가 정상적으로 할당되었는지 확인
        if (animator == null)
        {
            // Animator 컴포넌트가 없다면 오류 메시지 출력
            Debug.LogError("AgentMotor: Animator 컴포넌트를 찾을 수 없습니다! 애니메이션 재생에 필요합니다.");
        }

        // 먼지 효과 파티클 시스템이 인스펙터에서 할당되었는지 확인
        if (dustEffect == null)
        {
            // 할당되지 않았다면 경고 메시지 출력
            Debug.LogWarning("AgentMotor: 먼지 효과(Dust Effect) 파티클 시스템이 설정되지 않았습니다. 달리기 시 효과가 나타나지 않습니다.");
        }

        // 초기 목표 회전값을 현재 캐릭터의 회전값으로 설정 (갑작스러운 회전 방지)
        targetBodyRotation = transform.rotation;
    }

    // 매 프레임 호출 (주로 입력 처리, 비물리 로직 업데이트에 사용)
    void Update()
    {
        // 입력값을 기반으로 이동 방향 벡터 계산 후 정규화 (길이를 1로 만듦)
        // Y축은 0으로 설정하여 수평 이동만 고려
        moveDirectionInput = new Vector3(hAxis, 0, vAxis).normalized;
    }

    public void move_update(float mvx, float mvz, bool isshiftdown)
    {
        hAxis = mvx;
        vAxis = mvz;
        isShiftDown = isshiftdown;
    }

    // 고정된 시간 간격으로 호출 (주로 물리 계산, Rigidbody 조작에 사용)
    void FixedUpdate()
    {
        // 이동 입력이 충분히 큰지 (의미 있는 입력인지) 확인
        // sqrMagnitude는 magnitude보다 연산 비용이 저렴하여 성능에 유리
        bool hasSignificantInput = moveDirectionInput.sqrMagnitude > MinInputSqrMagnitudeForRotation;

        // 각 기능별 함수 호출
        HandleRotation(hasSignificantInput);    // 캐릭터 회전 처리
        HandleMovement(hasSignificantInput);    // 캐릭터 이동 처리
        UpdateAnimator(hasSignificantInput);    // 애니메이터 파라미터 업데이트
        HandleDustEffect(hasSignificantInput && isShiftDown); // 먼지 효과 처리 (입력이 있고 달리기 중일 때)
    }

    // 캐릭터의 회전을 처리하는 함수
    void HandleRotation(bool hasSignificantInput)
    {
        // 의미 있는 이동 입력이 있을 경우에만 목표 회전값 갱신
        if (hasSignificantInput)
        {
            // 입력된 이동 방향(moveDirectionInput)을 바라보는 Quaternion(회전값) 계산
            Quaternion lookRotation = Quaternion.LookRotation(moveDirectionInput);
            // 계산된 회전값에서 Y축 회전 정보만 사용하여 새로운 목표 회전값 생성 (X, Z축 회전은 0으로 고정)
            targetBodyRotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
        }
        // 입력이 없다면 targetBodyRotation은 이전 값을 유지 (캐릭터가 마지막 방향을 보도록 함)

        // 현재 회전 속도 계산 (달리기 중이라면 더 빠르게 회전)
        float currentRotationSpeed = rotationSpeed * (isShiftDown && hasSignificantInput ? runRotationSpeedMultiplier : 1f);

        // Rigidbody의 회전을 부드럽게 변경 (Slerp: 구면 선형 보간)
        // rb.rotation (현재 회전)에서 targetBodyRotation (목표 회전)으로 점진적 변화
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetBodyRotation, Time.fixedDeltaTime * currentRotationSpeed));
    }

    // 캐릭터의 이동을 처리하는 함수
    void HandleMovement(bool hasSignificantInput)
    {
        // 의미 있는 이동 입력이 있을 경우
        if (hasSignificantInput)
        {
            // 현재 이동 속도 계산 (달리기 중이라면 더 빠르게)
            float currentMoveSpeed = moveSpeed * (isShiftDown ? runSpeedMultiplier : 1f);
            // 캐릭터가 현재 바라보는 앞쪽 방향(transform.forward)으로 이동 벡터 계산
            Vector3 movement = transform.forward * currentMoveSpeed;

            // Rigidbody의 속도(velocity) 설정하여 캐릭터 이동
            // Y축 속도는 0으로 설정하여 수직 이동 방지 (점프 사용 안 함 가정)
            // Rigidbody의 Use Gravity가 true라면 이 설정은 중력과 계속 충돌하여 의도치 않은 움직임을 유발할 수 있으므로,
            // Use Gravity를 false로 설정하는 것이 권장됨.
            rb.velocity = new Vector3(movement.x, 0f, movement.z);
        }
        else // 이동 입력이 없을 경우 (캐릭터 정지)
        {
            // 모든 축의 속도를 0으로 설정하여 미끄러짐 없이 완전히 정지
            rb.velocity = new Vector3(0f, 0f, 0f);
        }
    }

    // 애니메이터의 파라미터를 업데이트하는 함수
    void UpdateAnimator(bool hasSignificantInput)
    {
        // Animator 컴포넌트가 없다면 함수 종료 (오류 방지)
        if (animator == null) return;

        float targetAnimSpeed = 0f; // 애니메이터에 전달할 목표 'speed' 값 초기화
        // 의미 있는 이동 입력이 있을 경우
        if (hasSignificantInput)
        {
            // 달리기 중이면 runSpeedMultiplier 값(예: 1.8), 아니면 1f (걷기 속도 기준)을 목표 속도로 설정
            // 이 값은 애니메이션 블렌드 트리의 설정에 따라 조절될 수 있음
            targetAnimSpeed = isShiftDown ? runSpeedMultiplier : 1f;
        }

        // 현재 애니메이터의 'speed' 값을 목표 값(targetAnimSpeed)으로 부드럽게 변경
        // Mathf.SmoothDamp는 시간에 따라 값을 점진적으로 변화시키는 데 사용
        currentAnimatorSpeed = Mathf.SmoothDamp(animator.GetFloat("speed"), targetAnimSpeed, ref animatorSpeedVelocity, speedChangeSmoothTime);

        // 계산된 최종 속도 값을 애니메이터의 "speed" 파라미터에 전달
        animator.SetFloat("speed", currentAnimatorSpeed);
        // "isRunning" 파라미터 설정 (입력이 있고 Shift 키를 누르고 있을 때 true)
        // 이 파라미터는 걷기/달리기 애니메이션 상태 전환 등에 사용될 수 있음
        animator.SetBool("isRunning", hasSignificantInput && isShiftDown);
    }

    // 먼지 효과를 제어하는 함수
    void HandleDustEffect(bool isRunningEffect) // 달리기 상태에 따라 먼지 효과를 켤지 결정하는 파라미터
    {
        // 먼지 효과 파티클 시스템이 할당되어 있을 경우에만 실행
        if (dustEffect != null)
        {
            // 먼지 효과를 내야 하는 상황(isRunningEffect가 true)이고, 현재 효과가 재생 중이 아닐 때
            if (isRunningEffect && !isDustPlaying)
            {
                dustEffect.Play();    // 파티클 효과 재생 시작
                isDustPlaying = true; // 효과 재생 중 상태로 변경
            }
            // 먼지 효과를 내지 않아야 하는 상황(isRunningEffect가 false)이고, 현재 효과가 재생 중일 때
            else if (!isRunningEffect && isDustPlaying)
            {
                dustEffect.Stop();    // 파티클 효과 중지
                isDustPlaying = false;// 효과 중지 상태로 변경
            }
        }
    }
}