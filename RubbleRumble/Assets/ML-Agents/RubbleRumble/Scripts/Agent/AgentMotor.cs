using UnityEngine;
using System.Collections;

public class AgentMotor : MonoBehaviour
{
    public float moveSpeed = 5.0f;           // 캐릭터의 기본 이동 속도
    public float runSpeedMultiplier = 1.8f;  // 달리기(대시) 시 이동 속도에 곱해질 배율
    public float rotationSpeed = 10f;        // 캐릭터의 기본 회전 속도 (초당 각도 변화)
    public float runRotationSpeedMultiplier = 1.5f; // 달리기(대시) 시 회전 속도에 곱해질 배율

    public float speedChangeSmoothTime = 0.1f; // 애니메이터의 'speed' 파라미터 변경 시 부드러운 전환에 걸리는 시간

    public ParticleSystem dustEffect;          // 달리기(대시) 시 바닥에서 발생하는 먼지 파티클 효과

    // 입력값 저장을 위한 내부 변수
    private float hAxis;                       // 수평축(Horizontal) 입력값 (좌우: A, D 또는 화살표 좌우)
    private float vAxis;                       // 수직축(Vertical) 입력값 (상하: W, S 또는 화살표 위아래)
    private bool isShiftDown;                  // 왼쪽 또는 오른쪽 Shift 키가 눌렸는지 여부 (달리기/대시 시도 판단용)

    // 자주 사용될 컴포넌트 참조를 위한 변수
    private Animator animator;                 // 캐릭터 애니메이션을 제어하는 Animator 컴포넌트
    private Rigidbody rb;                      // 캐릭터의 물리적 움직임을 담당하는 Rigidbody 컴포넌트
    private PlayerStamina playerStamina;       // 스태미너 관리를 위한 PlayerStamina 컴포넌트 참조 << 추가

    // 캐릭터의 내부 상태 및 계산에 사용될 변수
    private Vector3 moveDirectionInput;        // 사용자의 입력에 따른 순수 이동 방향 벡터 (정규화됨)
    private float currentAnimatorSpeed;        // 애니메이터에 현재 프레임에 전달할 최종 속도 값
    private float animatorSpeedVelocity;       // Mathf.SmoothDamp 함수 내부에서 사용되는 참조 변수 (속도 변화량 추적)

    // private bool isDustPlaying = false;        // 먼지 효과가 현재 재생 중인지 나타내는 플래그 (2025_06_09 : 파티클 오류 수정을 위해 주석 처리리)
    private Quaternion targetBodyRotation;     // 캐릭터가 바라봐야 할 목표 Y축 회전값
    private bool isActuallyDashing = false;    // 실제로 대시 중인지 여부를 나타내는 상태 << 추가

    // 회전의 안정성을 위해, 이 값보다 작은 입력 크기는 무시 (제곱된 크기 비교용)
    private const float MinInputSqrMagnitudeForRotation = 0.01f * 0.01f; // 0.01의 제곱, 매우 작은 입력 무시

    // 스크립트가 처음 활성화될 때 또는 게임 오브젝트가 처음 생성될 때 1회 호출
    void Awake()
    {
        // 필요한 컴포넌트들을 찾아와서 변수에 할당
        animator = GetComponentInChildren<Animator>(); // 자식 오브젝트에서 Animator 컴포넌트 검색
        rb = GetComponent<Rigidbody>();                // 현재 게임 오브젝트에서 Rigidbody 컴포넌트 검색

        // PlayerStamina 컴포넌트 할당 << 추가
        playerStamina = GetComponent<PlayerStamina>();
        if (playerStamina == null)
        {
            // PlayerStamina 컴포넌트가 없다면 경고 메시지 출력 (대시 기능 비활성화)
            Debug.LogWarning("AgentMotor: PlayerStamina 컴포넌트를 찾을 수 없습니다! 대시 기능이 작동하지 않습니다.");
        }

        // Rigidbody 컴포넌트가 정상적으로 할당되었는지 확인
        if (rb != null)
        {
            // 물리 엔진에 의해 캐릭터가 X축이나 Z축으로 넘어지지 않도록 회전을 제한
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            // rb.useGravity = false; // 원본 주석 유지
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
            // 할당되지 않았다면 경고 메시지 출력 (이름에서 "달리기 시"를 "대시 시"로 해석될 수 있도록 여지 남김)
            Debug.LogWarning("AgentMotor: 먼지 효과(Dust Effect) 파티클 시스템이 설정되지 않았습니다. 효과가 나타나지 않을 수 있습니다.");
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
        bool hasSignificantInput = moveDirectionInput.sqrMagnitude > MinInputSqrMagnitudeForRotation;
        bool wantsToDashWithInput = isShiftDown && hasSignificantInput; // 사용자가 Shift를 누르고 움직이는지 (대시 시도)

        // << 실제 대시 상태 결정 로직 추가 >>
        bool determinedDashStateForThisFrame = false; // 이번 FixedUpdate에서 최종적으로 대시를 할 것인지 결정하는 지역 변수

        if (wantsToDashWithInput && playerStamina != null) // 대시를 시도하고 스태미너 컴포넌트가 있다면
        {
            if (this.isActuallyDashing) // 이전 FixedUpdate에서 이미 대시 중이었다면 (대시 지속 조건)
            {
                if (playerStamina.currentStamina > 0) // 스태미너가 남아있다면
                {
                    playerStamina.ConsumeStamina(); // 스태미너 소모
                    determinedDashStateForThisFrame = true; // 이번 프레임도 대시
                }
                // 스태미너가 없다면 determinedDashStateForThisFrame은 false로 유지되어 대시 중단
            }
            else // 새로 대시를 시작하려 한다면 (대시 시작 조건)
            {
                if (playerStamina.CanStartDashing()) // 최소 시작 스태미너가 있다면
                {
                    playerStamina.ConsumeStamina(); // 스태미너 소모
                    determinedDashStateForThisFrame = true; // 대시 시작
                }
                // 최소 시작 스태미너가 없다면 determinedDashStateForThisFrame은 false로 유지되어 대시 시작 불가
            }
        }
        // 대시를 시도하지 않거나 스태미너 컴포넌트가 없다면 determinedDashStateForThisFrame은 false로 유지

        this.isActuallyDashing = determinedDashStateForThisFrame; // 현재 프레임의 최종 대시 상태를 멤버 변수에 저장

        // 각 기능별 함수 호출 시 isShiftDown 대신 isActuallyDashing 사용 << 수정 >>
        HandleRotation(hasSignificantInput, this.isActuallyDashing);    // 캐릭터 회전 처리
        HandleMovement(hasSignificantInput, this.isActuallyDashing);    // 캐릭터 이동 처리
        UpdateAnimator(hasSignificantInput, this.isActuallyDashing);    // 애니메이터 파라미터 업데이트
        HandleDustEffect(this.isActuallyDashing); // 먼지 효과는 실제 대시 중일 때만 << 수정 >>
    }

    // 캐릭터의 회전을 처리하는 함수 << 매개변수 isDashingNow 추가 >>
    void HandleRotation(bool hasSignificantInput, bool isDashingNow)
    {
        // 의미 있는 이동 입력이 있을 경우에만 목표 회전값 갱신
        if (hasSignificantInput)
        {
            Quaternion lookRotation = Quaternion.LookRotation(moveDirectionInput);
            targetBodyRotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
        }

        // 현재 회전 속도 계산 (대시 중이라면 더 빠르게 회전) << isShiftDown 대신 isDashingNow 사용 >>
        float currentRotationSpeed = rotationSpeed * (isDashingNow && hasSignificantInput ? runRotationSpeedMultiplier : 1f);

        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetBodyRotation, Time.fixedDeltaTime * currentRotationSpeed));
    }

    // 캐릭터의 이동을 처리하는 함수 << 매개변수 isDashingNow 추가 >>
    void HandleMovement(bool hasSignificantInput, bool isDashingNow)
    {
        if (hasSignificantInput)
        {
            // 현재 이동 속도 계산 (대시 중이라면 더 빠르게) << isShiftDown 대신 isDashingNow 사용 >>
            float currentMoveSpeed = moveSpeed * (isDashingNow ? runSpeedMultiplier : 1f);
            Vector3 movement = transform.forward * currentMoveSpeed;

            // Rigidbody의 속도(velocity) 설정하여 캐릭터 이동
            // Y축 속도는 0으로 설정하여 수직 이동 방지 (점프 사용 안 함 가정)
            // Rigidbody의 Use Gravity가 true라면 이 설정은 중력과 계속 충돌하여 의도치 않은 움직임을 유발할 수 있으므로,
            // Use Gravity를 false로 설정하는 것이 권장됨.
            // 캐릭터가 점프 등으로 Y축 속도를 가질 수 있다면 rb.velocity.y를 보존하는 것이 좋습니다.
            rb.velocity = new Vector3(movement.x, 0f, movement.z);
        }
        else // 이동 입력이 없을 경우 (캐릭터 정지)
        {
            // 모든 축의 속도를 0으로 설정하여 미끄러짐 없이 완전히 정지 (Y축 포함)
            // Y축 속도를 보존하려면 위와 같이 rb.velocity = new Vector3(0f, rb.velocity.y, 0f); 로 변경
            rb.velocity = new Vector3(0f, 0f, 0f);
        }
    }

    // 애니메이터의 파라미터를 업데이트하는 함수 << 매개변수 isDashingNow 추가 >>
    void UpdateAnimator(bool hasSignificantInput, bool isDashingNow)
    {
        if (animator == null) return;

        float targetAnimSpeed = 0f;
        if (hasSignificantInput)
        {
            // 대시 중이면 runSpeedMultiplier 값(예: 1.8), 아니면 1f (걷기 속도 기준)을 목표 속도로 설정 << isShiftDown 대신 isDashingNow 사용 >>
            targetAnimSpeed = isDashingNow ? runSpeedMultiplier : 1f;
        }

        currentAnimatorSpeed = Mathf.SmoothDamp(animator.GetFloat("speed"), targetAnimSpeed, ref animatorSpeedVelocity, speedChangeSmoothTime);
        animator.SetFloat("speed", currentAnimatorSpeed);

        // "isRunning" 파라미터 설정 (실제 대시 중일 때 true) << isShiftDown 조건 대신 isDashingNow 사용 >>
        // 이 파라미터는 걷기/대시(달리기) 애니메이션 상태 전환 등에 사용될 수 있음
        animator.SetBool("isRunning", isDashingNow);
    }

    // 먼지 효과를 제어하는 함수 << 매개변수 isDashingNow로 변경 (이전: isRunningEffect) >>
    void HandleDustEffect(bool isDashingNow)
    {
        if (dustEffect != null)
        {
            /*
            // 먼지 효과를 내야 하는 상황(isDashingNow가 true)이고, 현재 효과가 재생 중이 아닐 때
            if (isDashingNow && !isDustPlaying)
            {
                dustEffect.Play();
                isDustPlaying = true;
            }
            // 먼지 효과를 내지 않아야 하는 상황(isDashingNow가 false)이고, 현재 효과가 재생 중일 때
            else if (!isDashingNow && isDustPlaying)
            {
                dustEffect.Stop();
                isDustPlaying = false;
            }
            (2025_06_09 파티클 수정을 위해 주석 처리)*/

        }
        if (isDashingNow && !dustEffect.isPlaying)
            {
                dustEffect.Play(); // 파티클 재생
            }
            // 대시하면 안 되는 상황인데, 파티클이 현재 재생 중일 때
            else if (!isDashingNow && dustEffect.isPlaying)
            {
                dustEffect.Stop(); // 파티클 중지
            }
    }
}