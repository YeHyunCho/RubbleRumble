// =============================================================
// AITrashCleaner.cs  ─ Box·Can·Mop 통합 로직 (Pause-&-Unfold 개선)
// =============================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

// 필요한 컴포넌트 선언 ──────────────────────────────────────────
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
    public float rApproachTarget = 0.001f;            // 목표물에 접근했을 때 보상
    public float rRetreatPenalty = -0.0011f;          // 목표물에서 멀어졌을 때 벌점
    public float rWashMop = 0.7f;                     // 대걸레 세척 보상
    public float rPlaceBox = 0.6f;                    // Box를 책상에 내려놓으면 보상

    // ─────────────── 내부 상태 ────────────────
    private bool _pickedUp = false;
    private bool _reachedBin = false;
    private bool _threwAway = false;
    private int _prevScore = 0;
    private float _prevBinDist = 0f;
    private float _prevPaperBinDist = 0f;
    private float _prevSinkDist = 0f;
    private float _prevDeskDist = 0f;
    private DecisionRequester _requester;
    private bool _washingPause = false;

    // “박스 내려놓음 → 2초 정지 → 펼치기” 컨트롤용
    private bool _pauseAfterPlace = false;
    private float _pauseEndTime = 0f;

    // 쿨타임
    private float _distRewardCooldown = 0.5f;
    private float _lastDistRewardTime = -999f;

    // ──────────────────────────────────────────────────────────
    // 초기화
    // ──────────────────────────────────────────────────────────
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

    // 에피소드 시작
    public override void OnEpisodeBegin()
    {
        // 속도 초기화
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 시작 위치·회전 및 환경 리셋
        transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        StageManager.Instance?.TimeReset();
        MapManager.Instance?.ResetEnvironment();
        RetryTestBtn.OnRetryTestButtonCliked();

        // 내부 변수 초기화
        _pickedUp = false;
        _reachedBin = false;
        _threwAway = false;
        _prevScore = 0;
        cleaner.ResetHeldTrash();

        _prevPaperBinDist = GetClosestTargetDistance("TBpaper");
        _prevBinDist = GetClosestTargetDistance("TBcan");
        _prevSinkDist = GetClosestTargetDistance("Sink");
        _prevDeskDist = GetClosestTargetDistance("Desk");

        _pauseAfterPlace = false;
        _pauseEndTime = 0f;
    }

    // ──────────────────────────────────────────────────────────
    // 매 프레임 의사결정
    // ──────────────────────────────────────────────────────────
    private void FixedUpdate()
    {
        if (_requester == null)
            RequestDecision();
    }

    // ──────────────────────────────────────────────────────────
    // 행동 및 보상
    // ──────────────────────────────────────────────────────────
    public override void OnActionReceived(ActionBuffers actions)
    {
        var mop = cleaner.HeldTrash?.GetComponent<Mop>();

        // 0-A. 이미 세척 정지 상태라면 계속 멈춤
        if (_washingPause)
        {
            motor.Move(Vector3.zero);          // 가만히 서 있기

            // Mop 내부 holdTime이 0이 되면 세척 완료
            if (mop == null || mop.GetHoldingTime() == 0f)
            {
                _washingPause = false;
                AddReward(rWashMop);           // ← ★ 이 안에서 단-한-번만 지급
            }

            return;                            // 다른 행동 모두 스킵
        }

        // 0-B. 세척을 막 시작해야 하는지 체크
        if (mop != null && mop.GetUseCount() >= 2 && mop.GetHoldingTime() > 0f)
        {
            _washingPause = true;              // 다음 프레임부터 멈추기
            motor.Move(Vector3.zero);          // 첫 프레임도 멈춤
            return;
        }



        //------------------------------------------------------
        // 0. “2초 멈춤” 상태 체크
        //------------------------------------------------------
        if (_pauseAfterPlace)
        {
            motor.Move(Vector3.zero);

            // ★ 1) 매 프레임 호출: 타이머 누적
            cleaner.TryUnfoldBoxPublic();

            // ★ 2) 2초가 지나면(=타이머도 2초 쌓였으므로) 블록 탈출
            if (Time.time >= _pauseEndTime)
            {
                _pauseAfterPlace = false;
                Debug.Log("박스 펼치기 성공");
                cleaner.EquipToolPublic(0);
                cleaner.UseToolPublic();
            }
            return;
        }

        //------------------------------------------------------
        // 1. 기본 시간 페널티
        //------------------------------------------------------
        AddReward(stepPenalty);

        //------------------------------------------------------
        // 2. 이동 처리
        //------------------------------------------------------
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
        motor.Move(dir);

        //------------------------------------------------------
        // 3. 상호작용 처리
        //------------------------------------------------------
        int act = actions.DiscreteActions[1];
        switch (act)
        {
            case 1: cleaner.UseToolPublic(); break;              // (도구 사용)
            case 2: cleaner.TryThrowAwayPublic(); break;         // (버리기)
            case 3: cleaner.EquipToolPublic(0); break;           // 맨손
            case 4: cleaner.EquipToolPublic(1); break;           // 빗자루
            case 5: cleaner.EquipToolPublic(2); break;           // 대걸레
            case 6:
                // ① 내려놓기 시도 전, 박스 보유 여부 판단
                bool hadBox = cleaner.IsHoldingTrash && cleaner.HeldTrash.CompareTag("Box");

                cleaner.TryPlaceTrashOnWorkbenchPublic();   // 책상에 내려놓기 시도

                // ② 성공 조건 : (전) 박스를 들고 있었고, (후) 손이 비어 있으며, 실제로 책상 위에 박스가 존재할 때
                if (hadBox && !cleaner.IsHoldingTrash && cleaner.IsTrashOnWorkbench)
                {
                    AddReward(rPlaceBox);
                    Debug.Log("[DEBUG] 박스 내려놓음 & 2초 정지 돌입");

                    _pauseAfterPlace = true;
                    _pauseEndTime = Time.time + 2f;
                }
                break;

        }

        //------------------------------------------------------
        // 4. 간단 이벤트 보상 (줍기·도착·버리기)
        //------------------------------------------------------
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
        }

        //------------------------------------------------------
        // 5. 점수 변화 기반 보상 (최대 +1)
        //------------------------------------------------------
        int curScore = StageManager.Instance != null ? StageManager.Instance.AIScore : _prevScore;
        if (curScore > _prevScore)
        {
            int delta = curScore - _prevScore;
            AddReward(Mathf.Clamp(delta, 0, 1));
        }
        _prevScore = curScore;

        //------------------------------------------------------
        // 6. 목표물 접근/후퇴 shaping
        //------------------------------------------------------
        var heldTrash = cleaner.HeldTrash;
        if (heldTrash != null)
        {
            string tag = heldTrash.tag;
            float curDist = 0f;
            float prevDist = 0f;
            string targetTag = null;

            // 주목 대상 결정
            if (tag == "Can")
            {
                targetTag = "TBcan";
                curDist = GetClosestTargetDistance(targetTag);
                prevDist = _prevBinDist;
                _prevBinDist = curDist;
            }
            else if (tag == "Box")
            {
                targetTag = "Desk";
                curDist = GetClosestTargetDistance(targetTag);
                prevDist = _prevDeskDist;
                _prevDeskDist = curDist;
            }
            else if (tag == "UnfoldedBox")
            {
                targetTag = "TBpaper";
                curDist = GetClosestTargetDistance(targetTag);
                prevDist = _prevPaperBinDist;
                _prevPaperBinDist = curDist;
            }

            // Mop일 경우 세척 대상 전환
            var mopUseComp = heldTrash.GetComponent<Mop>();
            if (mopUseComp != null && mopUseComp.GetUseCount() >= 2)
            {
                targetTag = "Sink";
                curDist = GetClosestTargetDistance(targetTag);
                prevDist = _prevSinkDist;
                _prevSinkDist = curDist;
            }

            // 쿨타임 보상/벌점
            float now = Time.time;
            if (now - _lastDistRewardTime >= _distRewardCooldown && targetTag != null)
            {
                if (curDist < prevDist)
                    AddReward(rApproachTarget);
                else if (curDist > prevDist)
                    AddReward(rRetreatPenalty);

                _lastDistRewardTime = now;
            }

        }
    }
    // 임시 Heuristic()
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;

        for (int i = 0; i < discreteActions.Length; i++)
        {
            discreteActions[i] = 0;  // 모든 액션을 0으로 설정 (정지 등)
        }
    }


    // ──────────────────────────────────────────────────────────
    // 센서 관측
    // ──────────────────────────────────────────────────────────
    public override void CollectObservations(VectorSensor sensor)
    {
        // 1) 기본 상태 ------------------------------------------------
        sensor.AddObservation(cleaner.IsHoldingTrash ? 1f : 0f);

        // 2) 각종 객체 방향·거리 --------------------------------------
        AddDirDistObs(sensor, "Can");
        AddDirDistObs(sensor, "Dust");
        AddDirDistObs(sensor, "Box");
        AddDirDistObs(sensor, "Water");
        AddDirDistObs(sensor, "TBcan");
        sensor.AddObservation(GetClosestTargetDistance("Wall"));

        // 3) Mop 오염 단계 --------------------------------------------
        int mopUse = 0;
        var mop = cleaner.HeldTrash?.GetComponent<Mop>();
        if (mop != null) mopUse = Mathf.Clamp(mop.GetUseCount(), 0, 2);
        sensor.AddObservation(mopUse / 2f); // 0.0, 0.5, 1.0

        // 4) 추가 대상 -----------------------------------------------
        AddDirDistObs(sensor, "TBdust");
        AddDirDistObs(sensor, "TBpaper");
        AddDirDistObs(sensor, "Desk");
        AddDirDistObs(sensor, "Sink");

        // 5) 손에 든 물건 원-핫 ----------------------------------------
        float[] heldType = new float[4]; // 0: 없음, 1: Can, 2: Box, 3: UnfoldedBox
        if (cleaner.HeldTrash != null)
        {
            string tag = cleaner.HeldTrash.tag;
            if (tag == "Can") heldType[1] = 1f;
            else if (tag == "Box") heldType[2] = 1f;
            else if (tag == "UnfoldedBox") heldType[3] = 1f;
        }
        else heldType[0] = 1f;
        foreach (var v in heldType) sensor.AddObservation(v);
    }

    // ──────────────────────────────────────────────────────────
    // 유틸리티
    // ──────────────────────────────────────────────────────────
    private void AddDirDistObs(VectorSensor s, string tag)
    {
        Vector3 dir; float dist = GetClosestTargetDistance(tag, out dir);
        s.AddObservation(dir); s.AddObservation(dist);
    }

    // 가장 가까운 대상 거리만
    private float GetClosestTargetDistance(string tag)
        => GetClosestTargetDistance(tag, out _);

    // 거리 + 방향
    private float GetClosestTargetDistance(string tag, out Vector3 direction)
    {
        direction = Vector3.zero;
        GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
        float min = float.MaxValue;
        Vector3 pos = transform.position;

        foreach (var o in objs)
        {
            float d = Vector3.Distance(pos, o.transform.position);
            if (d < min)
            {
                min = d;
                direction = (o.transform.position - pos).normalized;
            }
        }

        if (min == float.MaxValue) min = 0f;
        if (min == 0f) direction = Vector3.zero;
        return min;
    }
}
