using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;
    [Tooltip("滑鼠靈敏度")]
    public float mouseSensitivity = 2f;

    [Header("必要元件")]
    public Camera playerCamera;

    private Rigidbody rb;
    private float xRotation = 0f;
    private Vector3 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 鎖定並隱藏滑鼠游標
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 確保剛體不會因為物理碰撞而翻倒
        rb.freezeRotation = true;
    }

    void Update()
    {
        // 1. 處理視角旋轉
        HandleMouseLook();

        // 2. 接收鍵盤輸入
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // 計算移動方向 (總是相對於玩家的面向)
        moveDirection = (transform.right * x + transform.forward * z).normalized;
    }

    void FixedUpdate()
    {
        // 3. 處理物理移動
        MovePlayer();
    }

    void HandleMouseLook()
    {
        if (playerCamera == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * 100f * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * 100f * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    // 這就是剛剛修改過的新移動函式
    void MovePlayer()
    {
        // 取得當前垂直速度 (保留重力)
        // 注意：如果是 Unity 6，建議用 rb.linearVelocity；舊版用 rb.velocity
        // 這裡為了保險起見，先寫 rb.velocity (舊版相容寫法)
        float currentY = rb.linearVelocity.y;

        // 計算新的水平速度
        Vector3 targetVelocity = moveDirection * moveSpeed;

        // 把重力加回去
        targetVelocity.y = currentY;

        // 套用速度
        rb.linearVelocity = targetVelocity;
    }
}