using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player; // 就算這裡沒東西，程式也會自己找

    void LateUpdate()
    {
        // 1. 如果目前沒有鎖定玩家，就嘗試去尋找
        if (player == null)
        {
            // 去場景裡找標籤是 "Player" 的物件
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
            else
            {
                // 如果真的找不到 (例如玩家還沒生出來)，就先不執行移動
                return;
            }
        }

        // 2. 找到了玩家，開始跟隨
        // 只更新 X 和 Z 軸 (水平位置)，Y 軸保持原本攝影機的高度 (例如 20)
        Vector3 newPosition = player.position;
        newPosition.y = transform.position.y;

        transform.position = newPosition;
    }
}