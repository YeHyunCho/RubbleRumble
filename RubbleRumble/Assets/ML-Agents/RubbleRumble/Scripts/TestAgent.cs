using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//agent에 필요한 패키지
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class TestAgent : Agent
{
    private Rigidbody rBody;
    private Vector3 startPosition = new Vector3(-21.3f, -0.007575989f, -12.6f);

    private AgentMotor agents;
    private StageManager stageManager;
    private MapManager mapManager;
<<<<<<< HEAD
    private Mop mop;
=======
    private GameObject aimap;
>>>>>>> seunghee
    private AgentInputHandler agentInputHandler;

    private int score = 0;
    private int old_score = 0;
    private float previousTimeLeft;
<<<<<<< HEAD
    private Vector3 Sink_xz = Vector3.zero;

    // Observation scratch variables
    private Vector3 relSink;
=======

    private Vector3 workbench_xz = Vector3.zero;
    private Vector3 Sink_xz = Vector3.zero;
    private Vector3 trashbinred_xz = Vector3.zero;
    private Vector3 trashbingreen_xz = Vector3.zero;
    private Vector3 trashbinblue_xz = Vector3.zero;
    // Observation scratch variables
    private Vector3 relWorkbench;
    private Vector3 relSink;
    private Vector3 relTBred;
    private Vector3 relTBblue;
    private Vector3 relTBgreen;

>>>>>>> seunghee
    private int toolIdx;
    private float tNorm;

    private float moveX;
    private float moveZ;
    private bool isShiftDown;
    private int numkey;
    private bool qPressed;
    private bool ePressed;
    private bool qhold;
    private bool ehold;
    private float cur;
    private float delta;

    private float qHoldTime = 0f;
    private const float Q_HOLD_THRESHOLD = 2f; // 2초
<<<<<<< HEAD
=======
    private GameObject[] walls;
>>>>>>> seunghee

    protected override void Awake()
    {
        base.Awake();
        rBody = GetComponent<Rigidbody>();
        Debug.Log("in Awake!!!");
<<<<<<< HEAD
        agents = FindObjectOfType<AgentMotor>(); 
        agentInputHandler = GetComponent<AgentInputHandler>();

        stageManager = StageManager.Instance;
        mapManager = MapManager.Instance; 

        Sink_xz = new Vector3(5.6f, -15.66089f, 5.458333f);
=======
        agents = FindObjectOfType<AgentMotor>();
        agentInputHandler = GetComponent<AgentInputHandler>();

        stageManager = StageManager.Instance;
        mapManager = MapManager.Instance;

        aimap = GameObject.Find("AIMap");

        workbench_xz = new Vector3(1.31f, -13.61f, 0.36f);
        Sink_xz = new Vector3(-2.72f, -15.72f, -4.62f);
        trashbinred_xz = new Vector3(7.1f, 0f, -8.6f);
        trashbinblue_xz = new Vector3(-0.008140475f, 1.151607f, -0.3802902f);
        trashbingreen_xz = new Vector3(26.25f, 0f, -21.63f);
>>>>>>> seunghee

    }

    public override void OnEpisodeBegin() //에피소드 자동 실행
    {
        Debug.Log("in OnEpisodeBegin!!!");
        //RetryTestBtn.OnRetryTestButtonCliked();
        transform.position = startPosition;
        mapManager.ResetEnvironment(); //환경 초기화

        //속도 초기화
        this.rBody.velocity = Vector3.zero;
        this.rBody.angularVelocity = Vector3.zero;

<<<<<<< HEAD
        this.transform.localPosition = new Vector3(-21.3f, -0.007575989f, -12.6f); //위치 초기화
        this.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); //회전 초기화

=======
        this.transform.localPosition = new Vector3(-15.94f, -0.007575989f, 1.46f); //위치 초기화
        this.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); //회전 초기화

        walls = GameObject.FindGameObjectsWithTag("Wall");
>>>>>>> seunghee
        //잔여시간 초기화
        stageManager.TimeReset();
        previousTimeLeft = stageManager.TimeLeft;
    }

    public override void CollectObservations(VectorSensor sensor) //환경관찰(행동에 필요한 데이터 수집) 벡터형
    {
        Vector3 agentPos = transform.localPosition;
<<<<<<< HEAD
        Debug.Log("agentPos: " + agentPos);
        Debug.Log("rBody.velocity: " + rBody.velocity);
        Debug.Log("rBody.angularVelocity: " + rBody.angularVelocity);
=======

>>>>>>> seunghee
        sensor.AddObservation(agentPos); //에이전트 위치
        sensor.AddObservation(rBody.velocity); //속도
        sensor.AddObservation(rBody.angularVelocity); //각속도

        List<Obstacle> obstacles = mapManager.aiObstacleList; //쓰레기 리스트??

        foreach (var obs in obstacles) //에이전트와 쓰레기들 상대 위치
        {
            if (obs == null) continue; //제거된 쓰레기 건너뜀
            Vector3 rel = obs.transform.localPosition - agentPos;
            sensor.AddObservation(rel.x);
            sensor.AddObservation(rel.z);
        }

<<<<<<< HEAD
        //쓰레기통들 위치 
        //작업대, 싱크대, 분리수거함 위치 
        relSink = Sink_xz - transform.localPosition;
        sensor.AddObservation(relSink.x);
        sensor.AddObservation(relSink.z);
=======
        // foreach (var wall in walls)//벽들과 상대 거리
        // {
        //     if (wall == null) continue;
        //     Vector3 relWall = wall.transform.localPosition - agentPos;
        //     sensor.AddObservation(relWall.x);
        //     sensor.AddObservation(relWall.z);
        // }
        sensor.AddObservation(GetClosestTargetDistance("Wall"));


        //쓰레기통들 위치 
        //작업대, 싱크대, 분리수거함 위치 
        relWorkbench = workbench_xz - agentPos;
        relSink = Sink_xz - agentPos;
        relTBred = trashbinred_xz - agentPos;
        relTBblue = trashbinblue_xz - agentPos;
        relTBgreen = trashbingreen_xz - agentPos;

        sensor.AddObservation(relWorkbench.x);
        sensor.AddObservation(relWorkbench.z);
        sensor.AddObservation(relSink.x);
        sensor.AddObservation(relSink.z);
        sensor.AddObservation(relTBred.x);
        sensor.AddObservation(relTBred.z);
        sensor.AddObservation(relTBblue.x);
        sensor.AddObservation(relTBblue.z);
        sensor.AddObservation(relTBgreen.x);
        sensor.AddObservation(relTBgreen.z);
>>>>>>> seunghee

        //현재 선택된 도구 (One-Hot 또는 정규화)
        toolIdx = agentInputHandler.GetCurrentTool();  // 0=맨손, 1=빗자루, 2=대걸레
        // 정규화 0, 0.5, 1.0
        sensor.AddObservation(toolIdx / 2f);

        //남은 시간 정규화된 값
        tNorm = StageManager.Instance.TimeLeft / StageManager.Instance.TimeLimit;
        sensor.AddObservation(tNorm);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers) // 행동을 수신하고 보상을 할당
    {
<<<<<<< HEAD
        Debug.Log("in OnActionReceived!!!!");
=======
        //Debug.Log("in OnActionReceived!!!!");
>>>>>>> seunghee
        old_score = score;

        moveX = actionBuffers.ContinuousActions[0];
        moveZ = actionBuffers.ContinuousActions[1];
        // Shift 확인 (달리기 상태 감지)
        isShiftDown = (actionBuffers.DiscreteActions[0] == 1); // Shift?

        agents.move_update(moveX, moveZ, isShiftDown);

        numkey = actionBuffers.DiscreteActions[3];
        qPressed = (actionBuffers.DiscreteActions[1] == 1);
        ePressed = (actionBuffers.DiscreteActions[2] == 1);
        qhold = false;
        ehold = false;

<<<<<<< HEAD
=======
        Debug.Log("NOW key: " + numkey);

>>>>>>> seunghee
        // 2) Q 홀드 여부 
        if (qPressed)
        {
            qHoldTime += Time.deltaTime;
            if (qHoldTime >= Q_HOLD_THRESHOLD)
            {
                // 2초 이상 Q가 눌려졌을 때 실행할 동작
                qhold = true;
                qHoldTime = 0f; // 한 번 실행 후 타이머 초기화
            }
        }
        else
        {
            qhold = false;
            qHoldTime = 0f; // Q를 뗐으면 누적 시간 리셋
        }
        //mop.SetHoldingTime(qHoldTime);
        //mop.WashMopNearSink(qhold, qHoldTime)

        agentInputHandler.HandleInput(numkey, qPressed, ePressed, qhold, ehold);

        score = stageManager.AIScore;

        // 점수 획득 시
        if (old_score < score)
            SetReward(1f);

        //남은 시간 변화량만큼 패널티
        cur = StageManager.Instance.TimeLeft;
        delta = previousTimeLeft - cur;
        if (delta > 0f)
            SetReward(-0.05f * delta);
        previousTimeLeft = cur;

<<<<<<< HEAD
=======
        //Debug.Log($"Current Reward: {GetCumulativeReward()}");

>>>>>>> seunghee
        if (cur <= 0f)
        {
            Debug.Log("episode ending");
            EndEpisode(); // 에피소드 종료
<<<<<<< HEAD
        
        }

    }
=======

        }

    }

    // 특정 태그를 가진 오브젝트들과의 거리 중 가장 가까운 거리 반환 (방향 무시 버전)
    private float GetClosestTargetDistance(string tag)
        => GetClosestTargetDistance(tag, out _);

    // 특정 태그를 가진 오브젝트들과의 거리 중 가장 가까운 거리와 방향을 반환
    private float GetClosestTargetDistance(string tag, out Vector3 direction)
    {
        direction = Vector3.zero;

        // 해당 태그를 가진 모든 오브젝트를 찾음
        GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
        float min = float.MaxValue;
        Vector3 agentPos = transform.position;

        // 가장 가까운 오브젝트를 찾기 위한 반복문
        foreach (var o in objs)
        {
            float d = Vector3.Distance(agentPos, o.transform.position);
            if (d < min)
            {
                min = d;
                direction = (o.transform.position - agentPos).normalized; // 방향 벡터 계산
            }
        }

        // 아무것도 발견되지 않은 경우 예외 처리
        if (min == float.MaxValue) min = 0f;
        if (min == 0f) direction = Vector3.zero;

        return min; // 가장 가까운 거리 반환
    }

    //벽 충돌시 -0.5점
    // private void OnCollisionEnter(Collision collision)
    // {
    //     if (collision.gameObject.CompareTag("Wall"))
    //     {
    //         Debug.Log("wall HIT!!");
    //         SetReward(-0.02f);
    //     }
    // }
>>>>>>> seunghee
}