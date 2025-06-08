// =============================================================
// AITrashCleaner.cs (캔만 처리하는 버전, 강화된 시간 페널티 및 거리 기반 보상)
// =============================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

// 필요한 컴포넌트 선언
[RequireComponent(typeof(AgentMotorK))]
[RequireComponent(typeof(TrashInteractionManager))]
[RequireComponent(typeof(AICleanerBase))]
public class AITrashCleaner : Agent
{
    // ─────────────── 참조 변수 ────────────────
    [Header("References")]
    public AgentMotorK motor;                         // 이동 처리용 모터
    public TrashInteractionManager interaction;       // 쓰레기와의 상호작용 매니저
    public AICleanerBase cleaner;                     // 에이전트의 도구 및 쓰레기 상태 관리

    // ─────────────── 설정값 ────────────────
    [Header("Decision Parameters")]
    public int decisionInterval = 1;                  // 행동 결정을 내리는 간격
    public float stepPenalty = -0.01f;                // 매 스텝마다 주는 시간 지연 벌점

    // ─────────────── 보상 값 ────────────────
    [Header("Reward Values")]
    public float rPickUpCan = 0.5f;                   // 캔을 주웠을 때 보상
    public float rReachBin = 0.3f;                    // 분리수거장에 도달했을 때 보상
    public float rThrowAway = 1.0f;                   // 쓰레기를 버렸을 때 보상
    public float rApproachTarget = 0.02f;             // 목표물에 접근했을 때 보상
    public float rRetreatPenalty = -0.01f;            // 목표물에서 멀어졌을 때 벌점

    // 내부 상태 추적용 변수들
    private bool _pickedUp = false;
    private bool _reachedBin = false;
    private bool _threwAway = false;
    private int _prevScore = 0;
    private float _prevCanDist = 0f;
    private float _prevBinDist = 0f;

    private DecisionRequester _requester;

    // 에이전트 초기화 시 호출
    public override void Initialize()
    {
        base.Initialize();
        if (cleaner == null) cleaner = GetComponent<AICleanerBase>();
        if (motor == null) motor = GetComponent<AgentMotorK>();
        if (interaction == null) interaction = FindObjectOfType<TrashInteractionManager>();

        _requester = GetComponent<DecisionRequester>();
        _requester.DecisionPeriod = Mathf.Max(1, decisionInterval);
        _requester.TakeActionsBetweenDecisions = false;
    }

    // 에피소드가 시작될 때 호출
    public override void OnEpisodeBegin()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 시작 위치 및 회전 초기화

        transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        // 환경 및 내부 상태 초기화
        StageManager.Instance?.TimeReset();
        MapManager.Instance?.ResetEnvironment();
        RetryTestBtn.OnRetryTestButtonCliked();

        _pickedUp = false;
        _reachedBin = false;
        _threwAway = false;
        _prevScore = 0;
        cleaner.ResetHeldTrash();

        _prevCanDist = GetClosestTargetDistance("Can");
        _prevBinDist = GetClosestTargetDistance("TBcan");
    }

    // 매 프레임마다 실행되는 행동 요청
    private void FixedUpdate()
    {
        if (_requester == null)
            RequestDecision();
    }

    // 행동 수행 및 보상 처리
    public override void OnActionReceived(ActionBuffers actions)
    {
        AddReward(stepPenalty); // 시간 지연 페널티

        // ─ 이동 처리 ─
        int move = actions.DiscreteActions[0];
        Vector3 dir = move switch
        {
            1 => Vector3.forward,
            2 => Vector3.back,
            3 => Vector3.left,
            4 => Vector3.right,
            5 => (Vector3.forward + Vector3.left),
            6 => (Vector3.forward + Vector3.right),
            7 => (Vector3.back + Vector3.left),
            8 => (Vector3.back + Vector3.right),
            _ => Vector3.zero
        };
        if (dir != Vector3.zero) dir.Normalize();
        motor.Move(dir); // 모터를 통해 이동

        // ─ 상호작용 처리 ─
        int act = actions.DiscreteActions[1];
        switch (act)
        {
            case 1: cleaner.UseToolPublic(); break;         // 도구 사용
            case 2: cleaner.TryThrowAwayPublic(); break;    // 쓰레기 버리기
            case 5: cleaner.EquipToolPublic(0); break;      // 맨손 장착
        }

        // ─ 보상 처리 ─
        if (!_pickedUp && cleaner.IsHoldingTrash)
        {
            _pickedUp = true;
            AddReward(rPickUpCan);
        }

        float binDist = GetClosestTargetDistance("TBcan");
        if (!_reachedBin && binDist < 1f)
        {
            _reachedBin = true;
            AddReward(rReachBin);
        }

        if (_pickedUp && !_threwAway && !cleaner.IsHoldingTrash)
        {
            _threwAway = true;
            AddReward(rThrowAway);
            EndEpisode(); // 에피소드 종료
        }

        // 점수 변화 기반 보상
        int cur = StageManager.Instance != null ? StageManager.Instance.AIScore : _prevScore;
        if (cur > _prevScore)
            AddReward(cur - _prevScore);
        _prevScore = cur;

        // 목표물 접근/후퇴 감지 보상
        float curCanDist = GetClosestTargetDistance("Can");
        if (curCanDist < _prevCanDist)
            AddReward(rApproachTarget);
        else if (curCanDist > _prevCanDist)
            AddReward(rRetreatPenalty);
        _prevCanDist = curCanDist;

        float curBinDist = GetClosestTargetDistance("TBcan");
        if (curBinDist < _prevBinDist)
            AddReward(rApproachTarget);
        else if (curBinDist > _prevBinDist)
            AddReward(rRetreatPenalty);
        _prevBinDist = curBinDist;
    }

    // 휴리스틱(키보드 테스트용)
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var d = actionsOut.DiscreteActions;

        // 이동 키 입력
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A)) d[0] = 5;
        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D)) d[0] = 6;
        else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.A)) d[0] = 7;
        else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D)) d[0] = 8;
        else if (Input.GetKey(KeyCode.W)) d[0] = 1;
        else if (Input.GetKey(KeyCode.S)) d[0] = 2;
        else if (Input.GetKey(KeyCode.A)) d[0] = 3;
        else if (Input.GetKey(KeyCode.D)) d[0] = 4;
        else d[0] = 0;

        // 상호작용 키 입력
        if (Input.GetKey(KeyCode.E)) d[1] = 1;
        else if (Input.GetKey(KeyCode.R)) d[1] = 2;
        else if (Input.GetKey(KeyCode.Alpha1)) d[1] = 5;
        else d[1] = 0;
    }

    // 센서 관측 추가
    public override void CollectObservations(VectorSensor sensor)
    {
        // 쓰레기를 들고 있는지 여부
        sensor.AddObservation(cleaner.IsHoldingTrash ? 1f : 0f);

        // 캔의 방향 및 거리
        Vector3 dirCan;
        float distCan = GetClosestTargetDistance("Can", out dirCan);
        sensor.AddObservation(dirCan);
        sensor.AddObservation(distCan);

        // 분리수거장의 방향 및 거리
        Vector3 dirBin;
        float distBin = GetClosestTargetDistance("TBcan", out dirBin);
        sensor.AddObservation(dirBin);
        sensor.AddObservation(distBin);

        // 벽까지의 가장 가까운 거리
        sensor.AddObservation(GetClosestTargetDistance("Wall"));
    }

    // 가장 가까운 대상과의 거리 계산 (방향 무시)
    private float GetClosestTargetDistance(string tag)
        => GetClosestTargetDistance(tag, out _);

    // 가장 가까운 대상과의 거리 및 방향 계산
    private float GetClosestTargetDistance(string tag, out Vector3 direction)
    {
        direction = Vector3.zero;
        GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
        float min = float.MaxValue;
        Vector3 agentPos = transform.position;

        foreach (var o in objs)
        {
            float d = Vector3.Distance(agentPos, o.transform.position);
            if (d < min)
            {
                min = d;
                direction = (o.transform.position - agentPos).normalized;
            }
        }

        // 아무것도 없으면 거리 0 처리
        if (min == float.MaxValue) min = 0f;
        if (min == 0f) direction = Vector3.zero;
        return min;
    }
}
