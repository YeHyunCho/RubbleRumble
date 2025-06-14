using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverPopup : UIBase
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI gameoverTxt;
    [SerializeField] private TextMeshProUGUI winnerLabel;
    [SerializeField] private TextMeshProUGUI loserLabel;
    [SerializeField] private TextMeshProUGUI winnerScoreTxt;
    [SerializeField] private TextMeshProUGUI loserScoreTxt;
    [SerializeField] private TextMeshProUGUI nextBtnTxt;

    [Header("Button")]
    [SerializeField] private Button nextBtn;
    [SerializeField] private Button exitBtn;

    private const string menuSceneName = "MenuScene3";

    private void Awake()
    {
        if (StageManager.Instance.IsWin)
            SetWinInfo();
        else
            SetLoseInfo();
    }

    private void OnEnable()
    {
        if (StageManager.Instance.IsWin)
            // TODO: 다음 스테이지로 넘어가게 하는 로직 작성(현재 임시로 Retry랑 동일하게 설정)
            nextBtn.onClick.AddListener(OnNextBtnClicked);
        else
            nextBtn.onClick.AddListener(OnNextBtnClicked);

        // TODO: exitBtn 누르면 LobbyScene으로 가도록 작성
        exitBtn.onClick.AddListener(OnExitBtnClicked);  // exit 버튼 누르면 메뉴로 돌아가도록 연결
    }

    // 승리 시 게임 종료 팝업 세팅
    private void SetWinInfo()
    {
        gameoverTxt.text = "VICTORY";
        winnerLabel.text = "YOUR SCORE:";
        loserLabel.text = "AI SCORE:";
        winnerScoreTxt.text = StageManager.Instance.PlayerScore.ToString();
        loserScoreTxt.text = StageManager.Instance.AIScore.ToString();

        if (LevelManager.Instance.SelectedLevelIdx == 2)    // 마지막 레벨이면
        {
            nextBtn.gameObject.SetActive(false);    // 다음 레벨 버튼 비활성화
            // Exit 버튼 위치 중앙 하단으로 변경
            RectTransform exitBtnTransform = exitBtn.gameObject.GetComponent<RectTransform>();
            if (exitBtnTransform != null)
            {
                exitBtnTransform.localPosition = new Vector3(0, -200, 0);
            }
        }
        nextBtnTxt.text = "NEXT";    
    }

    // 패배 시 게임 종료 팝업 세팅
    private void SetLoseInfo()
    {
        gameoverTxt.text = "DEFEAT";
        winnerLabel.text = "AI SCORE:";
        loserLabel.text = "YOUR SCORE:";
        winnerScoreTxt.text = StageManager.Instance.AIScore.ToString();
        loserScoreTxt.text = StageManager.Instance.PlayerScore.ToString();
        nextBtnTxt.text = "RETRY";
    }

    // Retry 혹은 Next 버튼 눌렀을 때 실행할 내용
    private void OnNextBtnClicked()
    {
        MapManager.Instance.ReturnAllObstacles();   // 씬에 남아있는 모든 쓰레기 오브젝트 풀에 반환
        
        if (StageManager.Instance.IsWin)    // 클리어 한 경우
        {
            LevelManager.Instance.SelectedLevelIdx++;   // 다음 레벨에서 시작하도록 인덱스 증가
        }
        // 클리어 하지 못한 경우 현재 레벨에서 다시 시작
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);   // 현재 활성화된 씬 재로드
    }

    private void OnExitBtnClicked()
    {
        MapManager.Instance.ReturnAllObstacles();   // 씬에 남아있는 모든 쓰레기 오브젝트 풀에 반환
        SceneManager.LoadScene(menuSceneName);   // 메뉴 화면으로 돌아감
    }
}
