using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

static public class RetryTestBtn
{
    // 화면 우상단에 초록색 재시작 테스트 버튼이 클릭되면
    static public void OnRetryTestButtonCliked()
    {
        MapManager.Instance.ReturnAllObstacles();   // 씬에 남아있는 모든 쓰레기 오브젝트 풀에 반환
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);   // 현재 활성화된 씬 재로드
    }
}
