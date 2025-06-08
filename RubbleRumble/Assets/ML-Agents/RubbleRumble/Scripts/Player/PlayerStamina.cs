// PlayerStamina.cs
using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina Settings")] // 헤더
    public float maxStamina = 3.5f; // 스태미너 총량 (3~4초 사이로 설정)
    [Tooltip("Current stamina amount. Can be observed in the Inspector for debugging.")] // 툴팁
    public float currentStamina; // 현재 스태미너
    public float staminaDrainRate = 1.0f; // 초당 스태미너 소모량 (대시 중)
    public float staminaRegenRate = 0.8f; // 초당 스태미너 회복량
    public float staminaRegenDelay = 1.5f; // 대시 종료 후 스태미너 회복 시작까지의 지연 시간 (초)
    public float minStaminaToInitiateDash = 0.2f; // 대시를 시작하는 데 필요한 최소 스태미너

    private float _timeSinceLastStaminaDrain = 0f; // 마지막 스태미너 소모 후 경과 시간
    private bool _isDrainingStaminaThisFrame = false; // 이번 프레임에 스태미너가 소모되었는지 여부

    void Awake()
    {
        currentStamina = maxStamina; // 게임 시작 시 스태미너를 최대로 설정
        // 게임 시작 시 바로 회복이 가능하도록 초기화 (만약 처음부터 대기시간을 적용하고 싶다면 0으로 설정)
        _timeSinceLastStaminaDrain = staminaRegenDelay;
    }

    void Update()
    {
        // 이번 프레임/FixedUpdate에서 스태미너가 소모되지 않았다면 회복 로직 실행
        if (!_isDrainingStaminaThisFrame)
        {
            _timeSinceLastStaminaDrain += Time.deltaTime; // 마지막 소모 후 시간 누적
            // 회복 지연 시간이 지났고, 현재 스태미너가 최대치 미만이면 회복
            if (_timeSinceLastStaminaDrain >= staminaRegenDelay && currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina); // 최대 스태미너를 넘지 않도록 함
            }
        }
        _isDrainingStaminaThisFrame = false; // 다음 프레임/FixedUpdate를 위해 리셋
    }

    /// <summary>
    /// 새로운 대시를 시작할 충분한 스태미너가 있는지 확인합니다.
    /// </summary>
    public bool CanStartDashing()
    {
        return currentStamina >= minStaminaToInitiateDash;
    }

    /// <summary>
    /// 대시 중 스태미너를 소모합니다. 대시가 활성화된 매 FixedUpdate마다 호출해야 합니다.
    /// 스태미너 소모 가능 여부(CanStartDashing 또는 currentStamina > 0)는 호출하는 쪽에서 판단했다고 가정합니다.
    /// </summary>
    public void ConsumeStamina()
    {
        if (currentStamina > 0)
        {
            // FixedUpdate에서 호출되므로 Time.fixedDeltaTime 사용
            currentStamina -= staminaDrainRate * Time.fixedDeltaTime;
            currentStamina = Mathf.Max(currentStamina, 0); // 스태미너가 0 미만으로 내려가지 않도록 함
            _timeSinceLastStaminaDrain = 0f; // 스태미너 소모했으므로 회복 지연 타이머 리셋
            _isDrainingStaminaThisFrame = true; // 스태미너가 소모되었음을 표시
        }
    }

    /// <summary>
    /// (선택 사항) UI 등에 현재 스태미너 비율(0~1)을 전달하기 위한 함수입니다.
    /// </summary>
    public float GetCurrentStaminaNormalized()
    {
        // maxStamina가 0 이하일 때 발생하는 DivideByZeroException 방지
        return maxStamina > 0 ? currentStamina / maxStamina : 0f;
    }
}