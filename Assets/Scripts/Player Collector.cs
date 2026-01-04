using UnityEngine;
using System.Collections; // 為了使用協程 (Coroutine)

public class PlayerCollector : MonoBehaviour
{
    [Header("收集進度")]
    public int currentPellets = 0;
    public int targetPellets = 30;

    [Header("一般豆子音效")]
    public AudioClip eatSound;

    [Header("大力丸設定")]
    [Tooltip("透視鬼魂的時間 (秒)")]
    public float powerDuration = 5.0f;
    public AudioClip powerUpSound; // 吃大力丸的音效

    private AudioSource _audioSource;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 1. 吃到普通豆子
        if (other.CompareTag("Pellet"))
        {
            CollectPellet(other.gameObject);
        }
        // 2. 吃到大力丸 (PowerPellet)
        else if (other.CompareTag("PowerPellet"))
        {
            StartCoroutine(CollectPowerPellet(other.gameObject));
        }
    }

    void CollectPellet(GameObject pellet)
    {
        currentPellets++;
        Debug.Log($"收集進度: {currentPellets} / {targetPellets}");

        if (eatSound != null) _audioSource.PlayOneShot(eatSound);

        Destroy(pellet);
        CheckWinCondition();
    }

    // --- 處理大力丸的協程 ---
    IEnumerator CollectPowerPellet(GameObject pellet)
    {
        Debug.Log("吃到大力丸！鬼魂現形！");

        // A. 播放音效
        if (powerUpSound != null) _audioSource.PlayOneShot(powerUpSound);

        // B. 銷毀大力丸 (隱藏並延遲銷毀，避免協程中斷，或是直接 Destroy 但把協程掛在別的地方)
        // 為了簡單起見，我們先把大力丸移到遠處並關閉顯示，等時間到再銷毀，
        // 或者直接 Destroy(pellet) 然後這段邏輯繼續跑 (因為協程是在 Player 身上跑的，沒問題)
        Destroy(pellet);

        // C. 開啟所有鬼魂的透視眼
        ToggleGhostIndicators(true);

        // D. 等待指定時間
        yield return new WaitForSeconds(powerDuration);

        // E. 關閉透視眼
        ToggleGhostIndicators(false);
        Debug.Log("大力丸失效，鬼魂隱藏...");
    }

    // 控制鬼魂頭頂標記的開關
    void ToggleGhostIndicators(bool show)
    {
        // 1. 找出場景中所有 Tag 為 "Ghost" 的物件
        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");

        foreach (GameObject ghost in ghosts)
        {
            // --- 【修改這裡】搜尋我們剛剛改名的 "MinimapIcon" ---
            Transform icon = ghost.transform.Find("Indicator");

            if (icon != null)
            {
                icon.gameObject.SetActive(show);
            }
        }
    }

    void CheckWinCondition()
    {
        if (currentPellets >= targetPellets)
        {
            Debug.Log("【達成目標】進入下一階段！");
        }
    }
}