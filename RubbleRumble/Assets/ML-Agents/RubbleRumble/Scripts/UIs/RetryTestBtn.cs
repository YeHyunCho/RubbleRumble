using UnityEngine.SceneManagement;

public static class RetryTestBtn
{
    private static bool _isReloading; // 이미 리로드 중인지?

    static RetryTestBtn()
    {
        // 새 씬이 다 로드되면 플래그 해제
        SceneManager.sceneLoaded += (_, __) => _isReloading = false;
    }

    public static void OnRetryTestButtonCliked()
    {
        if (_isReloading) return;     // 두 번 이상 눌러도 무시
        _isReloading = true;

        MapManager.Instance?.ReturnAllObstacles();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
