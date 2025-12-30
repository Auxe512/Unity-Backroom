using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;
    [Tooltip("滑鼠靈敏度")]
    public float mouseSensitivity = 2f;

    [Header("必要元件")]
    public Camera playerCamera; // 記得在 Inspector 把攝影機拖進來

    private Rigidbody rb;
    private float xRotation = 0f;
    private Vector3 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 鎖定並隱藏滑鼠游標 (按 Esc 可以跳出)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 確保剛體不會因為物理碰撞而翻倒
        rb.freezeRotation = true;
    }

    void Update()
    {
        // 1. 處理視角旋轉 (每一幀都執行，保證流暢)
        HandleMouseLook();

        // 2. 接收鍵盤輸入
        float x = Input.GetAxisRaw("Horizontal"); // A, D 鍵
        float z = Input.GetAxisRaw("Vertical");   // W, S 鍵

        // 計算移動方向 (總是相對於玩家的面向)
        moveDirection = (transform.right * x + transform.forward * z).normalized;
    }

    void FixedUpdate()
    {
        // 3. 處理物理移動 (在固定的物理時間執行)
        MovePlayer();
    }

    void HandleMouseLook()
    {
        if (playerCamera == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * 100f * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * 100f * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // 限制抬頭低頭角度 (-90度到90度)

        // 旋轉攝影機 (上下看)
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 旋轉玩家身體 (左右看)
        transform.Rotate(Vector3.up * mouseX);
    }

    void MovePlayer()
    {
        // 使用 Rigidbody 移動位置，比 transform.Translate 更適合物理碰撞
        Vector3 targetPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
    }
}