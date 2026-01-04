using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;
    [Tooltip("滑鼠靈敏度")]
    public float mouseSensitivity = 2f; // 建議這個值改小一點，例如 0.2

    [Header("必要元件")]
    public Camera playerCamera;

    [Header("音效設定")]
    public AudioClip runSound; // 拖入跑步/腳步聲的音效檔
    [Range(0f, 1f)]
    public float runVolume = 0.5f; // 音量大小

    // 內部變數
    private Rigidbody rb;
    private float xRotation = 0f;
    private Vector3 moveDirection;

    // 專門用來播腳步聲的喇叭 (跟吃豆子的喇叭分開)
    private AudioSource footstepSource;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 鎖定並隱藏滑鼠游標
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 確保剛體不會因為物理碰撞而翻倒
        rb.freezeRotation = true;

        // ---【新增】設定腳步聲專用的 AudioSource ---
        if (runSound != null)
        {
            // 動態新增一個 AudioSource 元件
            footstepSource = gameObject.AddComponent<AudioSource>();
            footstepSource.clip = runSound;
            footstepSource.loop = true; // 設定為循環播放
            footstepSource.volume = runVolume;
            footstepSource.playOnAwake = false;
        }
    }

    void Update()
    {
        // 1. 處理視角旋轉 (改用 GetAxisRaw 去除延遲感)
        HandleMouseLook();

        // 2. 接收鍵盤輸入
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // 計算移動方向
        moveDirection = (transform.right * x + transform.forward * z).normalized;

        // ---【新增】處理腳步聲 ---
        HandleFootsteps(x, z);
    }

    void FixedUpdate()
    {
        // 3. 處理物理移動
        MovePlayer();
    }

    void HandleMouseLook()
    {
        if (playerCamera == null) return;

        // 移除 Time.deltaTime 並改用 GetAxisRaw
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void MovePlayer()
    {
        // 1. 取得當前速度
        Vector3 currentVelocity = rb.linearVelocity; // Unity 6 使用 linearVelocity

        // 2. 計算目標水平速度
        Vector3 targetVelocity = moveDirection * moveSpeed;

        // 3. 保留垂直速度 (重力)
        targetVelocity.y = currentVelocity.y;

        // 4. 套用速度
        rb.linearVelocity = targetVelocity;
    }

    // ---【新增】控制腳步聲的函式 ---
    void HandleFootsteps(float xInput, float zInput)
    {
        if (footstepSource == null) return;

        // 判斷玩家是否有在移動 (有輸入訊號)
        bool isMoving = (xInput != 0 || zInput != 0);

        if (isMoving)
        {
            // 如果在移動，且聲音還沒開始播，就開始播
            if (!footstepSource.isPlaying)
            {
                // 可以加入一點隨機音調 (Pitch)，讓聲音聽起來不那麼死板
                footstepSource.pitch = Random.Range(0.9f, 1.1f);
                footstepSource.Play();
            }
        }
        else
        {
            // 如果沒在移動，且聲音正在播，就停止
            if (footstepSource.isPlaying)
            {
                footstepSource.Stop();
            }
        }
    }
}