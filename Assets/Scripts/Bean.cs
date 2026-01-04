using UnityEngine;

public class Bean : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("碰到了: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("是玩家！準備銷毀");
            Destroy(gameObject);
        }
    }
}