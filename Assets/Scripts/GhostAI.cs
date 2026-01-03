using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class GhostAI : MonoBehaviour
{
    [Header("參數設定")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 2.3f; // 調整過的速度
    public float detectionRange = 10f;
    public float turnSpeed = 10f; // [新增] 轉身速度，越大轉越快

    [Header("狀態 (唯讀)")]
    public string currentState;

    private NavMeshAgent _agent;
    private Transform _player;
    private Vector3 _wanderTarget;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();

        // [關鍵 1] 關閉 NavMesh 的自動旋轉，我們要自己寫程式控制
        _agent.updateRotation = false;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }

        SetWanderDestination();
    }

    void Update()
    {
        if (_player == null) return;

        // --- 狀態切換邏輯 ---
        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

        if (distanceToPlayer <= detectionRange)
        {
            // === 模式 1: 追擊 (Chase) ===
            currentState = "Chase Mode";
            _agent.speed = chaseSpeed;
            _agent.SetDestination(_player.position);

            // [關鍵 2] 追擊時，強制「面朝玩家」
            RotateTowards(_player.position);
        }
        else
        {
            // === 模式 2: 巡邏 (Patrol) ===
            currentState = "Patrol Mode";
            _agent.speed = patrolSpeed;

            // 巡邏時，面朝「移動方向」 (不然會像月球漫步一樣滑行)
            if (_agent.velocity.sqrMagnitude > 0.1f)
            {
                // 計算前方一點點的位置
                RotateTowards(transform.position + _agent.velocity);
            }

            if (_agent.remainingDistance < 0.5f)
            {
                SetWanderDestination();
            }
        }
    }

    // [關鍵 3] 自定義旋轉函式
    void RotateTowards(Vector3 targetPosition)
    {
        // 1. 計算方向向量 (目標位置 - 自己位置)
        Vector3 direction = (targetPosition - transform.position).normalized;

        // 2. 鎖定 Y 軸 (防止鬼怪因為玩家比較高或低而歪頭看天/看地)
        direction.y = 0;

        // 3. 如果方向不是零 (避免報錯)
        if (direction != Vector3.zero)
        {
            // 4. 計算目標旋轉角度
            Quaternion lookRotation = Quaternion.LookRotation(direction);

            // 5. 平滑旋轉 (Slerp) 過去，看起來比較自然
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
        }
    }

    void SetWanderDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 15f;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, 15f, 1))
        {
            _wanderTarget = hit.position;
            _agent.SetDestination(_wanderTarget);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}