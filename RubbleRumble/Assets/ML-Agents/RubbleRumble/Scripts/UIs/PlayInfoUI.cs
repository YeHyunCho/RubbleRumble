using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerScoreTxt;
    [SerializeField] private TextMeshProUGUI aiScoreTxt;
    [SerializeField] private Image timerBarImg;


    private void Awake()
    {
        if (playerScoreTxt == null)
        {
            playerScoreTxt = GameObject.Find("PlayInfoCanvas/TopPanel/PlayerScoreBG/PlayerScoreTxt").GetComponent<TextMeshProUGUI>();
        }
        if (aiScoreTxt == null)
        {
            aiScoreTxt = GameObject.Find("PlayInfoCanvas/TopPanel/AIScoreBG/AIScoreTxt").GetComponent<TextMeshProUGUI>();
        }
    }
    private void Update()
    {
        // 점수 UI 갱신
        playerScoreTxt.text = StageManager.Instance.PlayerScore.ToString();
        aiScoreTxt.text = StageManager.Instance.AIScore.ToString();

        timerBarImg.fillAmount = StageManager.Instance.TimeLeft / StageManager.Instance.TimeLimit; // 제한 시간 UI 갱신
    }
}