using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUIHandler : MonoBehaviour
{
    [SerializeField] GameObject playBtnPrefab;
    [SerializeField] GameObject lockBtnPrefab;

    [Header("Level Button")]
    [SerializeField] private List<GameObject> BtnList = new List<GameObject>();
    [SerializeField] private List<Button> levelBtnList = new List<Button>();
    //private const string titleSceneName = "TitleUITestScene"; // 타이틀씬 이름
    private const string titleSceneName = "TitleScene2"; // 타이틀씬 이름
    //private const string nextSceneName = "YhTestScene"; // 플레이씬 이름
    private const string nextSceneName = "ShTestScene"; // 플레이씬 이름

    private void Start()
    {
        // 버튼 클릭시 이벤트 연결
        for (int i = 0; i < levelBtnList.Count; i++)
        {
            if (i == 0 || DataManager.Instance.IsLevelCleared(i - 1))
            {
                int levelIndex = i;
                GameObject playBtn = Instantiate(playBtnPrefab, levelBtnList[i].gameObject.transform.parent);
                Destroy(BtnList[i].gameObject);
                BtnList.RemoveAt(i);
                levelBtnList.RemoveAt(i);
                BtnList.Insert(i, playBtn);
                levelBtnList.Insert(i, playBtn.GetComponent<Button>());
                levelBtnList[i].onClick.AddListener(() => OnClickedMenuBtn(levelIndex));
            }
        }
    }

    public void ClickBackButton()
    {
        //SceneManager.LoadScene(0);
        SceneManager.LoadScene(titleSceneName);
    }

    public void OnClickedMenuBtn(int levelIdx)
    {
        LevelManager.Instance.SelectedLevelIdx = levelIdx;
        SceneManager.LoadScene(nextSceneName);
    }
}
