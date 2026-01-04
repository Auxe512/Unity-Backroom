using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // ⚠️ 重要：要引用 UI 函式庫
using System.Collections;

public class LifeSystem : MonoBehaviour
{
    [Header("生命設定")]
    public int maxLives = 3;
    private int currentLives;

    [Header("受傷特效 (UI)")]
    [Tooltip("請把 Canvas 裡面的那個紅色 Image 拖進來")]
    public Image damageImage;
    [Tooltip("閃紅光之後淡出的速度")]
    public float flashSpeed = 5f;
    [Tooltip("受傷時要顯示的顏色 (建議紅色半透明)")]
    public Color flashColor = new Color(1f, 0f, 0f, 0.5f);

    [Header("受傷音效")]
    public AudioClip hurtSound; // 請把受傷的聲音檔拖進來
    private AudioSource audioSource;

    [Header("無敵設定")]
    public float invincibilityDuration = 2.0f;
    private bool isInvincible = false;

    void Start()
    {
        currentLives = maxLives;

        // 自動抓取或是新增 AudioSource 元件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        // --- 處理受傷紅屏淡出效果 ---
        if (damageImage != null)
        {
            // 利用 Lerp 插值運算，讓顏色隨時間慢慢變成 "透明 (Color.clear)"
            damageImage.color = Color.Lerp(damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
        }
    }

    public void TakeDamage()
    {
        if (isInvincible) return;

        currentLives--;

        // 1. 播放受傷音效
        if (hurtSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }

        // 2. 視覺特效：瞬間把圖片變成紅色 (Update 會負責把它慢慢變回透明)
        if (damageImage != null)
        {
            damageImage.color = flashColor;
        }

        Debug.Log("阿！被抓到了！剩餘生命：" + currentLives);

        if (currentLives <= 0)
        {
            GameOver();
        }
        else
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    void GameOver()
    {
        Debug.Log("遊戲結束！重新開始...");
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        // 這裡未來可以加入角色閃爍效果
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }
}