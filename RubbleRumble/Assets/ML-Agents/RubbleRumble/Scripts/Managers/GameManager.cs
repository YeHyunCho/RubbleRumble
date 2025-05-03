using UnityEngine;

public class GameManager : SingletonBase<GameManager>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);  // 씬 넘어가도 파괴되지 않고 유지
    }

    private void Start()
    {
        Application.targetFrameRate = 60;   // 고정 프레임 설정(60)
    }

    public void GameOver()
    {
        Time.timeScale = 0f;    // 시간 일시 정지
        StageManager.Instance.IsPlaying = false;    // 플레이하고 있지 않은 상태로 설정
        UIManager.Instance.Show<GameOverPopup>();   // 게임 종료창 팝업

        if (StageManager.Instance.IsWin)
            DataManager.Instance.UpdateLevelCleared(LevelManager.Instance.SelectedLevelIdx);
    }
}
