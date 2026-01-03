using UnityEngine; // 這一行就是解決 Vector3 找不到的關鍵

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

        // 計算移動方向
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

    void MovePlayer()
    {
        // 針對 Unity 6 的寫法：使用 linearVelocity 取代舊的 velocity

        // 1. 取得當前垂直速度 (保留重力)
        float currentY = rb.linearVelocity.y;

        // 2. 計算新的水平速度
        Vector3 targetVelocity = moveDirection * moveSpeed;

        // 3. 把重力加回去
        targetVelocity.y = currentY;

        // 4. 套用速度
        rb.linearVelocity = targetVelocity;
    }
}