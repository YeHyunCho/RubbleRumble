using UnityEngine;

public class AgentMotorK : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float runSpeedMultiplier = 1.8f;
    public float rotationSpeed = 10f;
    public float runRotationSpeedMultiplier = 1.5f;
    public float speedChangeSmoothTime = 0.1f;
    public ParticleSystem dustEffect;

    private Vector3 moveDirectionInput;
    private float currentAnimatorSpeed;
    private float animatorSpeedVelocity;
    private bool isDustPlaying = false;
    private Quaternion targetBodyRotation;
    private Animator animator;
    private Rigidbody rb;

    private const float MinInputSqrMagnitudeForRotation = 0.0001f;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();

        if (rb != null)
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        else
            Debug.LogError("AgentMotor: Rigidbody 컴포넌트를 찾을 수 없습니다!");

        if (animator == null)
            Debug.LogError("AgentMotor: Animator 컴포넌트를 찾을 수 없습니다!");

        if (dustEffect == null)
            Debug.LogWarning("AgentMotor: 먼지 효과(Dust Effect)가 설정되지 않았습니다.");

        targetBodyRotation = transform.rotation;
    }

    // 외부(AI)가 호출하는 이동 함수
    public void Move(Vector3 direction, bool dash = true)
    {
        bool hasInput = direction.sqrMagnitude > MinInputSqrMagnitudeForRotation;
        if (hasInput)
            moveDirectionInput = direction.normalized;

        HandleRotation(hasInput, dash);
        HandleMovement(hasInput, dash);
        UpdateAnimator(hasInput, dash);
        HandleDustEffect(dash);
    }

    private void HandleRotation(bool hasSignificantInput, bool isDashingNow)
    {
        if (hasSignificantInput)
        {
            Quaternion lookRotation = Quaternion.LookRotation(moveDirectionInput);
            targetBodyRotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
        }

        float currentRotationSpeed = rotationSpeed * (isDashingNow && hasSignificantInput ? runRotationSpeedMultiplier : 1f);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetBodyRotation, Time.fixedDeltaTime * currentRotationSpeed));
    }

    private void HandleMovement(bool hasSignificantInput, bool isDashingNow)
    {
        if (hasSignificantInput)
        {
            float currentMoveSpeed = moveSpeed * (isDashingNow ? runSpeedMultiplier : 1f);
            Vector3 movement = transform.forward * currentMoveSpeed;
            rb.velocity = new Vector3(movement.x, 0f, movement.z);
        }
        else
        {
            rb.velocity = Vector3.zero;
        }
    }

    private void UpdateAnimator(bool hasSignificantInput, bool isDashingNow)
    {
        if (animator == null) return;

        float targetAnimSpeed = hasSignificantInput ? (isDashingNow ? runSpeedMultiplier : 1f) : 0f;
        currentAnimatorSpeed = Mathf.SmoothDamp(animator.GetFloat("speed"), targetAnimSpeed, ref animatorSpeedVelocity, speedChangeSmoothTime);
        animator.SetFloat("speed", currentAnimatorSpeed);
        animator.SetBool("isRunning", isDashingNow);
    }

    private void HandleDustEffect(bool isDashingNow)
    {
        if (dustEffect != null)
        {
            if (isDashingNow && !isDustPlaying)
            {
                dustEffect.Play();
                isDustPlaying = true;
            }
            else if (!isDashingNow && isDustPlaying)
            {
                dustEffect.Stop();
                isDustPlaying = false;
            }
        }
    }
}
