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
    private Vector3 startPosition = new Vector3(-43.5f, -0.007575989f, -9f);

    private AgentMotor agents;
    private ToolManager toolManager;
    private StageManager stageManager;
    private MapManager mapManager;
    private Mop mop;
    private AgentInputHandler agentInputHandler;

    private int score = 0;
    private int old_score = 0;
    private float previousTimeLeft;
    private Vector3 Sink_xz = Vector3.zero;

    // Observation scratch variables
    private Vector3 relSink;
    private int toolIdx;
    private float tNorm;
    // Action reception scratch variables
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

    // Start is called before the first frame update

    private float qHoldTime = 0f;
    private const float Q_HOLD_THRESHOLD = 2f; // 2초


    protected override void Awake()
    {
        base.Awake();
        rBody = GetComponent<Rigidbody>();

        agents = FindObjectOfType<AgentMotor>();
        agentInputHandler = GetComponent<AgentInputHandler>();

        var mgr = GameObject.Find("Managers");
        toolManager = mgr.GetComponent<ToolManager>();
        stageManager = mgr.GetComponent<StageManager>();
        mapManager = mgr.GetComponent<MapManager>();
        
        Sink_xz = new Vector3(5.6f, -15.66089f, 5.458333f);

    }
    
    public override void OnEpisodeBegin() //에피소드 자동 실행
    {
        transform.position = startPosition;
        mapManager.ResetEnvironment(); //환경 초기화
               
        //속도 초기화
        this.rBody.velocity = Vector3.zero;
        this.rBody.angularVelocity = Vector3.zero;

        this.transform.localPosition = new Vector3(14.09f, -0.76f, -10.71f); //위치 초기화
        this.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); //회전 초기화
        
        //잔여시간 초기화
        stageManager.TimeReset();
        previousTimeLeft = stageManager.TimeLeft;
    }

    public override void CollectObservations(VectorSensor sensor) //환경관찰(행동에 필요한 데이터 수집) 벡터형
    {
        Vector3 agentPos = transform.localPosition;
        
        sensor.AddObservation(agentPos); //에이전트 위치
        sensor.AddObservation(rBody.velocity); //속도
        sensor.AddObservation(rBody.angularVelocity); //각속도

        List<Obstacle> obstacles = MapManager.Instance.aiObstacleList; //쓰레기 리스트??
        
        foreach (var obs in obstacles) //에이전트와 쓰레기들 상대 위치
        {
            if (obs == null) continue; //제거된 쓰레기 건너뜀
            Vector3 rel = obs.transform.localPosition - agentPos;
            sensor.AddObservation(rel.x);
            sensor.AddObservation(rel.z);
        }

        //쓰레기통들 위치 
        //작업대, 싱크대, 분리수거함 위치 
        relSink = Sink_xz - transform.localPosition;
        sensor.AddObservation(relSink.x);
        sensor.AddObservation(relSink.z);

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
        cur = stageManager.TimeLeft;
        delta = previousTimeLeft - cur;
        if (delta > 0f)
            SetReward(-0.05f * delta);
        previousTimeLeft = cur;

        //쓰레기 종류별로 설정
        if (cur <= 0f)
            EndEpisode(); // 에피소드 종료
    }
}
