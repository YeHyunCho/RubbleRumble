using UnityEngine;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;

[System.Serializable]
public class Level
{
    public int idx;
    public SerializedDictionary<string, int> ObstacleDict; // 스폰될 장애물 종류와 개수
    public List<float> spawnCooldowns; // 추가 스폰 시간 목록 (오름차순 정렬 권장)
}

public class LevelManager : SingletonBase<LevelManager>
{
    [Header("Level Setting")]
    public List<Level> levels; // 모든 레벨 설정 리스트

    public int SelectedLevelIdx { get; set; } // 선택된 레벨의 인덱스 (-1은 미선택)

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
        // SelectedLevelIdx = -1;
        /* 테스트용 코드 */
        // 모든 씬에서 레벨 3으로 시작하도록 설정
        SelectedLevelIdx = 2;
    }

    // 현재 선택된 레벨 정보 반환
    public Level GetLevelInfo()
    {
        if (SelectedLevelIdx >= 0 && SelectedLevelIdx < levels.Count)
        {
            return levels[SelectedLevelIdx];
        }
        else
        {
            Debug.LogWarning("No level selected or invalid index.");
            return null;
        }
    }
}
