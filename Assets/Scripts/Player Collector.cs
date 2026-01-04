using UnityEngine;
using System.Collections;

public class PlayerCollector : MonoBehaviour
{
    [Header("收集進度")]
    public int currentPellets = 0;
    public int targetPellets = 50; //

    [Header("一般豆子音效")]
    public AudioClip eatSound;

    [Header("大力丸設定")]
    [Tooltip("透視鬼魂的時間 (秒)")]
    public float powerDuration = 5.0f;
    public AudioClip powerUpSound;

    [Header("勝利畫面設定")]
    [Tooltip("請把 Canvas 裡面的 WinPanel (勝利圖片) 拖進來")]
    public GameObject winUI;

    private AudioSource _audioSource;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 確保勝利畫面一開始是關閉的
        if (winUI != null) winUI.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pellet"))
        {
            CollectPellet(other.gameObject);
        }
        else if (other.CompareTag("PowerPellet"))
        {
            StartCoroutine(CollectPowerPellet(other.gameObject));
        }
    }

    void CollectPellet(GameObject pellet)
    {
        currentPellets++;

        // 這裡幫你加個 Log，讓你在 Console 可以確認目標是對的
        Debug.Log($"收集進度: {currentPellets} / {targetPellets}");

        if (eatSound != null) _audioSource.PlayOneShot(eatSound);

        Destroy(pellet);
        CheckWinCondition();
    }

    // --- 勝利判斷與結算 ---
    void CheckWinCondition()
    {
        // 只要收集數量 >= 目標 (50)，就獲勝
        if (currentPellets >= targetPellets)
        {
            Debug.Log("【達成目標】恭喜獲勝！");
            YouWin();
        }
    }

    void YouWin()
    {
        // 1. 顯示勝利圖片
        if (winUI != null)
        {
            winUI.SetActive(true);
        }

        // 2. 解鎖滑鼠 (讓你之後可以按下一關或重來)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 3. 暫停遊戲時間
        Time.timeScale = 0f;
    }

    // ... 大力丸相關 ...
    IEnumerator CollectPowerPellet(GameObject pellet)
    {
        if (powerUpSound != null) _audioSource.PlayOneShot(powerUpSound);
        Destroy(pellet);
        ToggleGhostIndicators(true);
        yield return new WaitForSeconds(powerDuration);
        ToggleGhostIndicators(false);
    }

    void ToggleGhostIndicators(bool show)
    {
        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject ghost in ghosts)
        {
            Transform icon = ghost.transform.Find("MinimapIcon");
            if (icon != null) icon.gameObject.SetActive(show);
        }
    }
}