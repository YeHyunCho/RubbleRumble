using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreTxt;
    [SerializeField] private Image timerBarImg;

    private void Update()
    {
        // 점수 UI 갱신
        scoreTxt.text = $"{StageManager.Instance.AIScore.ToString()} / {StageManager.Instance.PlayerScore.ToString()} ";

        timerBarImg.fillAmount = StageManager.Instance.TimeLeft / StageManager.Instance.TimeLimit; // 제한 시간 UI 갱신
    }
}
