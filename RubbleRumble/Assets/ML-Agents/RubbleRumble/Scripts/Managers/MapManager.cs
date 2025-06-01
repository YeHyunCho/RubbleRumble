using System.Collections.Generic;
using UnityEngine;

public class MapManager : SingletonBase<MapManager>
{
    [Header("Map")]
    [SerializeField] private Transform playerMap;
    [SerializeField] private Transform aiMap;

    [Header("LevelInfo")]
    public List<Obstacle> playerObstacleList;   // 씬에 활성화된 플레이어의 쓰레기 리스트
    public List<Obstacle> aiObstacleList;       // 씬에 활성화된 AI의 쓰레기 리스트

    [SerializeField] private Level LevelInfo;   // 현재 레벨의 정보
    [SerializeField] private List<float> spawnTime = new List<float>(); // 쓰레기 재생성 하는 시간(반드시 빠른 시간 순으로 정렬)
    private float curTime;  // 현재 경과 시간

    [Header("Pool")]
    [SerializeField] private PoolManager.PoolConfig[] _poolConfigs; // 인스펙터에서 풀 설정

    protected override void Awake()
    {
        base.Awake();
        PoolManager.Instance.AddPools<Obstacle>(_poolConfigs);

        LevelInfo = LevelManager.Instance.GetLevelInfo();   // 현재 레벨 설정 정보 가져오기
        spawnTime = LevelInfo.spawnCooldowns;   // 쿨타임 리스트 설정
        SettingMap();   // 맵에 쓰레기 초기 세팅
        curTime = 0;
    }
    //장성우 추가
    public void ResetEnvironment() //학습 시 에피소드 종료마다 환경 초기화 필요.
    {
        PoolManager.Instance.DeleteAllPools(); // 이전 오브젝트 초기화
        
        PoolManager.Instance.AddPools<Obstacle>(_poolConfigs);
        LevelInfo = LevelManager.Instance.GetLevelInfo();
        spawnTime = LevelInfo.spawnCooldowns;
        SettingMap();
        curTime = 0;
    }


    private void Update()
    {
        if (!StageManager.Instance.IsPlaying) return;   // 플레이 상태가 아니면 바로 리턴
        if (spawnTime.Count == 0) return; // 더이상 쓰레기를 스폰하지 않아도 되면 리턴

        curTime += Time.deltaTime;  // 경과 시간 갱신

        if (curTime >= spawnTime[0])    // 쓰레기를 스폰해야 하는 시간이 되면
        {
            SettingMap();   // 쓰레기 생성
            spawnTime.RemoveAt(0);
        }
    }

    // 맵에 쓰레기 종류와 개수 설정
    private void SettingMap()
    {
        foreach (var obstacle in LevelInfo.ObstacleDict)
        {
            SpawnObstacle(playerMap, aiMap, obstacle.Key, obstacle.Value);
        }
    }

    private void SpawnObstacle(Transform playerMap, Transform aiMap, string name, int count)
    {
        for (int i = 0; i < count; i++)
        {
            // 랜덤 위치 설정
            // Vector3 randPos = new Vector3(Random.Range(-4.5f, 4.5f), 0.2f, Random.Range(-4.5f, 4.5f));
            Vector3 randPos = new Vector3(Random.Range(-10.0f, 10.0f), 0.2f, Random.Range(-10.0f, 10.0f));
            randPos = transform.TransformDirection(randPos);

            // 풀에서 가져오고 위치와 회전값 설정
            Obstacle playerObstacle = PoolManager.Instance.SpawnFromPool<Obstacle>(name, randPos + playerMap.transform.position, Quaternion.identity);
            playerObstacle.IsPlayer = true; // 쓰레기를 플레이어로 소유권 설정
            AddToList(playerObstacle);  // 플레이어 쓰레기 활성화 리스트에 추가

            Obstacle aiObstacle = PoolManager.Instance.SpawnFromPool<Obstacle>(name, randPos + aiMap.transform.position, Quaternion.identity);
            aiObstacle.IsPlayer = false; // 쓰레기를 AI로 소유권 설정
            AddToList(aiObstacle);  // AI 쓰레기 활성화 리스트에 추가
        }
    }

    // 활성화된 쓰레기 리스트에서 삭제
    public void RemoveFromList(Obstacle obstacle)
    {
        if (obstacle.IsPlayer)
        {
            playerObstacleList.Remove(obstacle);
            StageManager.Instance.PlayerObstacleCnt--;
        }
        else
        {
            aiObstacleList.Remove(obstacle);
            StageManager.Instance.AIObstacleCnt--;
        }
    }

    // 활성화된 쓰레기 리스트에 추가
    public void AddToList(Obstacle obstacle)
    {
        if (obstacle.IsPlayer)
        {
            playerObstacleList.Add(obstacle);
            StageManager.Instance.PlayerObstacleCnt++;
        }
        else
        {
            aiObstacleList.Add(obstacle);
            StageManager.Instance.AIObstacleCnt++;
        }
    }

    // 모든 활성화된 쓰레기 오브젝트를 풀에 반환
    public void ReturnAllObstacles()
    {
        for (int i = playerObstacleList.Count; i > 0; i--)
        {
            Obstacle playerObstacle = playerObstacleList[i - 1];
            if (playerObstacle.isActiveAndEnabled)
                PoolManager.Instance.ReturnToPool(playerObstacle.name, playerObstacle);
            playerObstacleList.Remove(playerObstacle);
        }
        for (int i = aiObstacleList.Count; i > 0; i--)
        {
            Obstacle aiObstacle = aiObstacleList[i - 1];
            if (aiObstacle.isActiveAndEnabled)
                PoolManager.Instance.ReturnToPool(aiObstacle.name, aiObstacle);
            aiObstacleList.Remove(aiObstacle);
        }
    }
}