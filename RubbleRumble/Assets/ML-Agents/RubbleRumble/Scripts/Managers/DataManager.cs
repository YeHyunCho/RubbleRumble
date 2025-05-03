using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class DataManager : SingletonBase<DataManager>
{
    private const string saveFileName = "PlayData.json"; // 데이터 저장 파일명
    private const int levelCnt = 3; // 총 레벨 수

    private List<bool> PlayInfoList = new List<bool>(); // 레벨 별 클리어 여부 저장하는 리스트

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
        LoadData();
    }

    // 저장 경로 가져오기
    private string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, saveFileName);
    }

    // 플레이 데이터 저장
    private void SaveData()
    {
        try
        {
            string json = JsonConvert.SerializeObject(PlayInfoList);
            File.WriteAllText(GetSavePath(), json);
            Debug.Log("Game data saved");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game data: {e.Message}");
        }
    }

    // 저장된 데이터 로드
    private void LoadData()
    {
        string path = GetSavePath();    // 저장 경로 가져오기
        if (File.Exists(path))  // 저장 파일 있으면
        {
            try
            {
                string json = File.ReadAllText(path);   // 파일 읽기
                PlayInfoList.Clear();
                PlayInfoList.AddRange(JsonConvert.DeserializeObject<List<bool>>(json)); // 역직렬화

                // 로드 후 데이터 유효성 검사 (null이거나 레벨 수가 다른 경우)
                if (PlayInfoList == null || PlayInfoList.Count != levelCnt)
                {
                    Debug.LogWarning("Loaded data is invalid. Initializing default data.");
                    InitializeDefaultData();    // 데이터 초기화
                }
                else
                {
                    Debug.Log("Game data loaded.");
                    Debug.Log(GetSavePath());
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load game data: {e.Message}. Initializing default data.");
                InitializeDefaultData();    // 데이터 초기화
            }
        }
        else    // 저장 파일 없으면
        {
            Debug.Log("No save file found. Initializing default data.");
            InitializeDefaultData();    // 데이터 초기화
        }
    }

    // 기본 데이터 생성하여 초기화
    private void InitializeDefaultData()
    {
        PlayInfoList = new List<bool>(levelCnt);

        for (int i = 0; i < levelCnt; i++)
        {
            PlayInfoList.Add(false);
        }
        Debug.Log($"Initialized default data");
        
        SaveData(); // 초기화 후 바로 저장
    }

    // 클리어 상태 갱신
    public void UpdateLevelCleared(int levelIdx)
    {
        if (levelIdx >= 0 && levelIdx < PlayInfoList.Count)
        {
            PlayInfoList[levelIdx] = true;  // 클리어 상태로 변경
            SaveData(); // 변경된 상태 저장
            Debug.Log($"Level {levelIdx} cleared status updated.");
        }
        else
        {
            Debug.LogWarning($"UpdateLevelCleared failed: Index {levelIdx} is invalid.");
        }
    }

    // 클리어 상태 확인
    public bool IsLevelCleared(int levelIdx)
    {
        if (levelIdx >= 0 && PlayInfoList.Count > levelIdx)
            return PlayInfoList[levelIdx];
        else
        {
            Debug.LogWarning($"LevelCleared failed: Index {levelIdx} is invalid.");
            return false;
        }
    }
}