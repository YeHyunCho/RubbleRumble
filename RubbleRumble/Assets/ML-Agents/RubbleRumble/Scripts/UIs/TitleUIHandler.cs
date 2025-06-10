using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEditor;
#if UNITI_EDITOR
using UnityEditor;
#endif

public class TitleUIHandler : MonoBehaviour
{
    //    public void ClickStartButton()
    //    {
    //        SceneManager.LoadScene(1);
    //    }

    //    public void ClickExitButton()
    //    {
    //#if UNITY_EDITOR
    //        EditorApplication.ExitPlaymode();
    //#else
    //        Application.Quit();
    //#endif
    //    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            SceneManager.LoadScene(1);
        }
    }
}
