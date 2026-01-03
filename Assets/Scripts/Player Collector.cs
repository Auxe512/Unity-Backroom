using UnityEngine;
using UnityEngine.UI; // 如果之後要做 UI 顯示分數需要這個

public class PlayerCollector : MonoBehaviour
{
    [Header("收集進度")]
    public int currentPellets = 0;
    public int targetPellets = 30; // 企劃書設定的目標

    [Header("音效 (選填)")]
    public AudioClip eatSound; // 吃豆子的聲音
    private AudioSource _audioSource;

    void Start()
    {
        // 如果有掛 AudioSource 就抓取
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            // 動態新增一個，方便播放音效
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // 當玩家進入 "Is Trigger" 的碰撞體時會觸發
    void OnTriggerEnter(Collider other)
    {
        // 檢查碰到的東西是不是豆子 (依靠 Tag 判斷)
        if (other.CompareTag("Pellet"))
        {
            CollectPellet(other.gameObject);
        }
    }

    void CollectPellet(GameObject pellet)
    {
        // 1. 增加分數
        currentPellets++;
        Debug.Log($"收集進度: {currentPellets} / {targetPellets}");

        // 2. 播放音效 (如果有設定)
        if (eatSound != null)
        {
            _audioSource.PlayOneShot(eatSound);
        }

        // 3. 銷毀豆子物件 (代表吃掉了)
        Destroy(pellet);

        // 4. 檢查是否達成企劃書的階段轉換條件
        CheckWinCondition();
    }

    void CheckWinCondition()
    {
        if (currentPellets >= targetPellets)
        {
            Debug.Log("【達成目標】收集足夠的幼的 (Pellets)！準備進入階段二 (熄燈/傳送)...");
            // 之後這裡會呼叫 GameManager 觸發第二階段
        }
    }
}