using UnityEngine;
using UnityEngine.SceneManagement; // 記得加這行

public class GameController : MonoBehaviour
{
    [Header("UI 面板")]
    public GameObject gameOverPanel; // 拖入失敗面板
    public GameObject winPanel;      // 拖入勝利面板

    // 遊戲開始時確保面板是關閉的
    void Start()
    {
        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);
        Time.timeScale = 1f; // 確保時間是流動的
    }

    // === 觸發失敗 ===
    public void TriggerGameOver()
    {
        gameOverPanel.SetActive(true); // 顯示失敗面板
        Time.timeScale = 0f; // 暫停遊戲時間（怪物停止移動）
        Cursor.lockState = CursorLockMode.None; // 解鎖滑鼠，讓玩家可以點按鈕
        Cursor.visible = true;
    }

    // === 觸發勝利 ===
    public void TriggerWin()
    {
        winPanel.SetActive(true); // 顯示勝利面板
        Time.timeScale = 0f; // 暫停遊戲
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // === 按鈕功能：重試 (Try Again) ===
    public void RestartGame()
    {
        // 重新讀取當前場景
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // === 按鈕功能：回到大廳 (Back) ===
    public void BackToMenu()
    {
        Time.timeScale = 1f; // 恢復時間，避免回到主選單後卡住
        SceneManager.LoadScene("UI scene"); // 這裡請填寫你主選單場景的"確切名稱"
    }
}