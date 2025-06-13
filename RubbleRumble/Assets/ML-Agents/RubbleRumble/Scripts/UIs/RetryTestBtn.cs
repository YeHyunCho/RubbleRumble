using UnityEngine;
using UnityEngine.SceneManagement;

public static class RetryTestBtn
{
    private static bool _isReloading; // �̹� ���ε� ������?

    static RetryTestBtn()
    {
        // �� ���� �� �ε�Ǹ� �÷��� ����
        SceneManager.sceneLoaded += (_, __) => _isReloading = false;
    }

    public static void OnRetryTestButtonCliked()
    {
        if (_isReloading) return;     // �� �� �̻� ������ ����
        _isReloading = true;

        MapManager.Instance?.ReturnAllObstacles();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
