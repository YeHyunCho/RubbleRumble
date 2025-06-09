using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//agent에 필요한 패키지
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.SceneManagement;

public class TestAgent : Agent
{
    private Rigidbody rBody;
    private Vector3 startPosition = new Vector3(-21.3f, -0.007575989f, -12.6f);

    private AgentMotor agents;
    private StageManager stageManager;
    private MapManager mapManager;
    private GameObject aimap;
    private AgentInputHandler agentInputHandler;

    private int score = 0;
    private int old_score = 0;
    private float previousTimeLeft;

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
    private GameObject[] walls;
    private int i = 0;

    protected override void Awake()
    {
        base.Awake();
        rBody = GetComponent<Rigidbody>();
        Debug.Log("in Awake!!!");
        agents = FindObjectOfType<AgentMotor>();
        agentInputHandler = GetComponent<AgentInputHandler>();

        stageManager = StageManager.Instance;
        mapManager = MapManager.Instance;

        aimap = GameObject.Find("AIMap");
        walls = GameObject.FindGameObjectsWithTag("Wall");
            
        workbench_xz = new Vector3(1.31f, -13.61f, 0.36f);
        Sink_xz = new Vector3(-2.72f, -15.72f, -4.62f);
        trashbinred_xz = new Vector3(7.1f, 0f, -8.6f);
        trashbinblue_xz = new Vector3(-0.008140475f, 1.151607f, -0.3802902f);
        trashbingreen_xz = new Vector3(26.25f, 0f, -21.63f);

    }

    public override void OnEpisodeBegin() //에피소드 자동 실행
    {
        Debug.Log("in OnEpisodeBegin!!!");
        //RetryTestBtn.OnRetryTestButtonCliked();
        transform.position = startPosition;
        if (i == 0)
            mapManager.ResetEnvironment(); //환경 초기화
        else if (i > 0)
        {
            Debug.Log(i);
            RetryTestBtn.OnRetryTestButtonCliked();
        }
            
        //속도 초기화
        this.rBody.velocity = Vector3.zero;
        this.rBody.angularVelocity = Vector3.zero;

        this.transform.localPosition = new Vector3(-15.94f, -0.007575989f, 1.46f); //위치 초기화
        this.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); //회전 초기화

        //잔여시간 초기화
        stageManager.TimeReset();
        previousTimeLeft = stageManager.TimeLeft;

        i++;
    }

    public override void CollectObservations(VectorSensor sensor) //환경관찰(행동에 필요한 데이터 수집) 벡터형
    {
        Vector3 agentPos = transform.localPosition;

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

        foreach (var wall in walls)//벽들과 상대 거리
        {
            if (wall == null) continue;
            Vector3 relWall = wall.transform.localPosition - agentPos;
            sensor.AddObservation(relWall.x);
            sensor.AddObservation(relWall.z);
        }
        sensor.AddObservation(GetClosestTargetDistance("Wall"));


        //작업대, 싱크대, 쓰레기통 위치 
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

        //현재 선택된 도구 (One-Hot 인코딩)
        toolIdx = agentInputHandler.GetCurrentTool();  // 0=맨손, 1=빗자루, 2=대걸레
        sensor.AddObservation(toolIdx == 0 ? 1f : 0f);
        sensor.AddObservation(toolIdx == 1 ? 1f : 0f);
        sensor.AddObservation(toolIdx == 2 ? 1f : 0f);

        //대걸레 세척 가능 유무
        sensor.AddObservation(agentInputHandler.GetMopUseCount());

        //쓰레기 소유
        bool isHoldingTrash = agentInputHandler.GetisHoldingTrash();
        sensor.AddObservation(isHoldingTrash);
        //쓰레기 종류
        int HoldingTrashName = agentInputHandler.GetHoldingTrashName();
        bool holds0 = (HoldingTrashName == 0); //can
        bool holds1 = (HoldingTrashName == 0); //can
        bool holds2 = (HoldingTrashName == 1); //box
        bool holds3 = (HoldingTrashName == 2); //unfoldebox
        sensor.AddObservation(holds0 ? 1f : 0f);
        sensor.AddObservation(holds1 ? 1f : 0f);
        sensor.AddObservation(holds2 ? 1f : 0f);
        sensor.AddObservation(holds3 ? 1f : 0f);
        
        //남은 시간 정규화된 값
        float tl = StageManager.Instance.TimeLimit;
        tNorm = (tl > 0f) ? StageManager.Instance.TimeLeft / tl : 0f;
        sensor.AddObservation(tNorm);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers) // 행동을 수신하고 보상을 할당
    {
        //Debug.Log("in OnActionReceived!!!!");
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

        Debug.Log("NOW key: " + numkey);

        // 2) Q 홀드 여부 
        if (qPressed)
        {
            qHoldTime += Time.deltaTime;
            if (qHoldTime >= Q_HOLD_THRESHOLD)
            {
                // 2초 이상 Q가 눌리면
                qhold = true;
                qHoldTime = 0f; // 한 번 실행 후 타이머 초기화
            }
        }
        else
        {
            qhold = false;
            qHoldTime = 0f; // Q를 뗐으면 누적 시간 리셋
        }

        agentInputHandler.HandleInput(numkey, qPressed, ePressed, qhold, ehold);

        score = stageManager.GetAiSocre();

        // 점수 획득 시
        if (old_score < score)
        {
            int c = score - old_score;

            switch (c)
            {
                case 100:
                    AddReward(3f);
                    break;
                case 220:
                    AddReward(6.5f);
                    break;
                case 200:
                    AddReward(6f);
                    break;
                case 150:
                    AddReward(5f);
                    break;
            }
        }

        AddReward(agentInputHandler.GetAddScore());

        //남은 시간 변화량만큼 패널티
        cur = stageManager.TimeLeft;
        delta = previousTimeLeft - cur;
        if (delta > 0f)
            AddReward(-0.05f * delta);
        previousTimeLeft = cur;

        agentInputHandler.Clear_Addscore();

        if (cur <= 0.1f)
        {
            Debug.Log("episode ending");
            EndEpisode(); // 에피소드 종료
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
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("wall HIT!!");
            SetReward(-0.02f);
        }
    }
}

// 현재 들고 있는 쓰레기 입력

// 선택지
// 쓰레기통 버릴까 말까 if 1: q press
// 대걸레 청소할까 말까 if 1: 2s e holding
// 상자 접을까 말까 if 1: e press, 2s q holding 