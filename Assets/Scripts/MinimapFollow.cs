using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player; // 拖入 Player 物件

    void LateUpdate()
    {
        if (player == null) return;

        // 只更新 X 和 Z 軸 (水平位置)，Y 軸保持在高空
        Vector3 newPosition = player.position;
        newPosition.y = transform.position.y; // 保持原本的高度 (例如 20)

        transform.position = newPosition;

        // 如果你希望地圖跟著玩家旋轉，把下面這行取消註解
        // transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
    }
}