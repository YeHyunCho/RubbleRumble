using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//agent에 필요한 패키지
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class TestAgent : Agent
{
    private ToolManager toolManager;
    private Mop mop;

    AgentController agentController;
    private float previousTimeLeft;
    private bool sink_True;

    // Start is called before the first frame update
    Rigidbody rBody;

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        if (rBody == null)
            Debug.LogError("TestAgent: Rigidbody 컴포넌트를 찾을 수 없습니다.");

        var mgr = GameObject.Find("Managers");
        toolManager = mgr.GetComponent<ToolManager>();
        toolManager.EquipTool(2);

        mop = FindObjectOfType<Mop>();
        if (mop == null)
            Debug.LogError("TestAgent: 씬에 Mop 스크립트가 붙은 오브젝트가 없습니다.");


        if (toolManager == null)
            Debug.LogError("TestAgent: Managers 오브젝트 또는 ToolManager 컴포넌트를 찾을 수 없습니다.");

        agentController = GetComponent<AgentController>();
        if (agentController == null)
            Debug.LogError("TestAgent: AgentController 컴포넌트를 찾을 수 없습니다.");
    }

    //public Transform Target;

    public override void OnEpisodeBegin() //에피소드 자동 실행
    {
        MapManager.Instance.ResetEnvironment(); //환경 초기화
        //속도 초기화
        this.rBody.velocity = Vector3.zero;
        this.rBody.angularVelocity = Vector3.zero;

        this.transform.localPosition = new Vector3(14.09f, -0.76f, -10.71f); //위치 초기화
        this.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); //회전 초기화
        //처음 잔여시간
        if (StageManager.Instance != null)
                {
                    previousTimeLeft = StageManager.Instance.TimeLeft;
                }
                else
                {
                    Debug.LogError("TestAgent: StageManager 싱글톤이 존재하지 않습니다.");
                    previousTimeLeft = 0f;
                }

        // 싱크대 체크
        bool sink_True = mop != null && mop.sink != null 
                        ? mop.sink.transform 
                        : null; 
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

        // 싱크대 상대 위치 관측 (널 체크 포함)
        Vector3 relSink = Vector3.zero;

        if (sink_True)
            relSink = mop.sink.transform.localPosition - transform.localPosition;
        sensor.AddObservation(relSink.x);
        sensor.AddObservation(relSink.z);

        //현재 선택된 도구 (One-Hot 또는 정규화)
        int toolIdx = toolManager.currentTool;   // 0=맨손, 1=빗자루, 2=대걸레
        // 정규화 0, 0.5, 1.0
        sensor.AddObservation(toolIdx / 2f);

        //대걸레 세탁 필요 여부
        bool mopDirty = (toolManager.currentTool == 2) &&
                    (mop != null && mop.GetUseCount() >= 2);
        sensor.AddObservation(mopDirty ? 1f : 0f);


        //남은 시간 정규화된 값
        float tNorm = StageManager.Instance.TimeLeft / StageManager.Instance.TimeLimit;
        sensor.AddObservation(tNorm);
    }

    public float forceMultiplier = 10; //이속
    public float AgentRunSpeedMultiplier = 1.8f; //달리기 시 이속증가배율

    public override void OnActionReceived(ActionBuffers actionBuffers) // 행동을 수신하고 보상을 할당
    {
        //1) 이동
        float moveX = actionBuffers.ContinuousActions[0];
        float moveZ = actionBuffers.ContinuousActions[1];

        // 2) Q 홀드 여부 (Branch 0)
        bool qPressed = actionBuffers.DiscreteActions[0] == 1;
        agentController.SetQPressed(qPressed);

        // 2) 대걸레 세척 시도
        if (mop.TryWashWithHold(qPressed))
        {
            AddReward(+0.4f);
        }

         // 3) Shift 홀드 여부 (Branch 1)
        bool shiftPressed = actionBuffers.DiscreteActions[1] == 1;
        agentController.SetShiftPressed(shiftPressed);

        // 이동 속도 계산
        float speedMult = shiftPressed
            ? AgentRunSpeedMultiplier
            : 1f;
        rBody.AddForce(new Vector3(moveX, 0, moveZ) * forceMultiplier * speedMult);

        // // 4) 분리수거 시? E 키 (Branch 2) 
        // if (actionBuffers.DiscreteActions[2] == 1)
        //     agentController.HandleEKey();

        // 대걸레 사용 E 키
        int toolIdx = toolManager.currentTool;
        if (toolIdx == 2 && actionBuffers.DiscreteActions[2] == 1)
        {
            // mop.TryCleanDust()가 true면 보상
            if (mop.AgentUpdate())
            {
                SetReward(+1.0f);
            }

             // 단, 만약 대걸레가 더러운 상태인데도 청소 시도하면 페널티
            if (mop.GetUseCount() >= 2)
            {
                SetReward(-0.5f);
            }
        }

        // 5) 숫자키 (Branch 3)
        int numKey = actionBuffers.DiscreteActions[3]; // 0,1,2
        switch (numKey)
        {
            case 1: toolManager.EquipTool(0); break;
            case 2: toolManager.EquipTool(1); break;
            case 3: toolManager.EquipTool(2); break;
        }
        //남은 시간 변화량만큼 패널티
        float cur = StageManager.Instance.TimeLeft;
        float delta = previousTimeLeft - cur;
        if (delta > 0f)
            SetReward(-0.05f * delta);
        previousTimeLeft = cur;

        //쓰레기 종류별로 설정
        
        if (cur <= 0f)
            EndEpisode(); // 에피소드 종료
    }
}
