using UnityEngine;
using UnityEngine.SceneManagement; // 這一行一定要加，才能切換場景

public class MainMenuController : MonoBehaviour
{
    // === 按鈕功能：開始遊戲 ===
    public void StartGame()
    {
        // "SampleScene" 請改成你真正的遊戲場景名稱 (一定要跟 Build Settings 裡的名字一樣)

     

        SceneManager.LoadScene("GameScene");

    }

    // === 按鈕功能：離開遊戲 ===
    public void QuitGame()
    {
        Debug.Log("遊戲已關閉！"); // 因為在編輯器按 Quit 沒反應，所以印出文字讓我們知道有成功
        Application.Quit();
    }
}