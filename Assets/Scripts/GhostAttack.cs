using UnityEngine;

public class GhostAttack : MonoBehaviour
{
    //原本是 OnCollisionEnter，現在改成 OnTriggerEnter
    //參數類型也從 Collision 變成 Collider
    void OnTriggerEnter(Collider other)
    {
        // 檢查穿過這個 Trigger 的東西是不是玩家
        // 注意：這裡直接用 other.CompareTag，不用寫 .gameObject
        if (other.CompareTag("Player"))
        {
            // 嘗試取得玩家身上的生命系統
            LifeSystem playerLife = other.GetComponent<LifeSystem>();

            // 如果真的有找到生命系統，就扣血
            if (playerLife != null)
            {
                playerLife.TakeDamage();
            }
        }
    }
}