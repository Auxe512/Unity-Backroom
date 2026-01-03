using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class GhostAI : MonoBehaviour
{
    [Header("參數設定")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 2.3f;
    public float detectionRange = 10f;
    public float turnSpeed = 10f;

    [Header("狀態 (唯讀)")]
    public string currentState;

    private NavMeshAgent _agent;
    private Transform _player; // 這是空的，所以鬼不知道要追誰
    private Vector3 _wanderTarget;

    void Start()
    {
        // 1. 【關鍵修正】取得自身的 NavMeshAgent 元件
        _agent = GetComponent<NavMeshAgent>();

        // 2. 【關鍵修正】尋找玩家物件
        // 請確保你的玩家物件 Tag 是 "Player"
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }
        else
        {
            Debug.LogError("找不到 Tag 為 'Player' 的物件！鬼怪無法運作。");
        }

        // 3. 預設先關閉 Agent，避免生成時位置錯誤
        _agent.enabled = false;

        StartCoroutine(EnableAgent());
    }

    IEnumerator EnableAgent()
    {
        yield return null; // 等待一幀，讓位置同步

        if (_agent != null)
        {
            _agent.enabled = true; // 開啟導航

            // 一開始先給個隨機目標，避免它發呆
            SetWanderDestination();
        }
    }

    void Update()
    {
        // 如果沒抓到 Component 或沒找到玩家，就不執行
        if (_agent == null || !_agent.isOnNavMesh || _player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

        if (distanceToPlayer <= detectionRange)
        {
            // === 追擊模式 ===
            currentState = "Chase Mode";
            _agent.speed = chaseSpeed;
            _agent.SetDestination(_player.position);
            RotateTowards(_player.position);
        }
        else
        {
            // === 巡邏模式 ===
            currentState = "Patrol Mode";
            _agent.speed = patrolSpeed;

            if (_agent.velocity.sqrMagnitude > 0.1f)
            {
                RotateTowards(transform.position + _agent.velocity);
            }

            // 到達目的地後，找下一個點
            if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
            {
                SetWanderDestination();
            }
        }
    }

    void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
        }
    }

    void SetWanderDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 15f;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, 15f, NavMesh.AllAreas))
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