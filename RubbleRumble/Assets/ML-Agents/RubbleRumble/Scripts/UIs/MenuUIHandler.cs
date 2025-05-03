using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUIHandler : MonoBehaviour
{
    [Header("Level Button")]
    [SerializeField] Button levelBtn1;
    [SerializeField] Button levelBtn2;
    [SerializeField] Button levelBtn3;

    private const string nextSceneName = "UITestScene"; // 플레이씬 이름

    private void OnEnable()
    {
        // 버튼 클릭시 이벤트 연결
        levelBtn1.onClick.AddListener(() => OnClickedMenuBtn(0));
        levelBtn2.onClick.AddListener(() => OnClickedMenuBtn(1));
        levelBtn3.onClick.AddListener(() => OnClickedMenuBtn(2));
    }

    public void ClickBackButton()
    {
        SceneManager.LoadScene(0);
    }

    public void OnClickedMenuBtn(int levelIdx)
    {
        LevelManager.Instance.SelectedLevelIdx = levelIdx;
        SceneManager.LoadScene(nextSceneName);
    }
}
