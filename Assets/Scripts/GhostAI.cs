using System.Collections;
using UnityEngine;
using UnityEngine.AI; // 引用 AI 導航系統

[RequireComponent(typeof(NavMeshAgent))]
public class GhostAI : MonoBehaviour
{
    public enum GhostState { Patrol, Chase } // 定義兩種狀態

    [Header("設定")]
    public Transform player;          // 玩家的 Transform
    public Vector3 patrolCorner;      // 這隻鬼魂負責巡邏的角落座標
    public float chaseDuration = 20f; // 追擊時間 (秒)
    public float patrolDuration = 10f;// 巡邏時間 (秒)

    [Header("狀態監控 (唯讀)")]
    public GhostState currentState;

    private NavMeshAgent agent;
    private Coroutine aiRoutine;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // 確保 Ghost 不會旋轉導致穿模
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        // 啟動 AI 循環
        aiRoutine = StartCoroutine(AI_Logic_Loop());
    }

    void Update()
    {
        // 每一幀根據當前狀態設定目的地
        if (currentState == GhostState.Chase)
        {
            if (player != null)
            {
                agent.SetDestination(player.position);
            }
        }
        else if (currentState == GhostState.Patrol)
        {
            agent.SetDestination(patrolCorner);
        }

        // 簡單的面對方向邏輯 (如果模型是 3D 的)
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(agent.velocity.normalized);
        }
    }

    // AI 核心狀態切換迴圈
    IEnumerator AI_Logic_Loop()
    {
        while (true)
        {
            // --- 進入追擊模式 ---
            currentState = GhostState.Chase;
            Debug.Log($"{name} 進入追擊模式！");

            // 增加速度 (可選，讓追擊更有威脅)
            agent.speed = 3.5f;

            yield return new WaitForSeconds(chaseDuration);

            // --- 進入巡邏模式 ---
            currentState = GhostState.Patrol;
            Debug.Log($"{name} 退回角落巡邏...");

            // 稍微減速 (可選，給玩家喘息)
            agent.speed = 3.0f;

            yield return new WaitForSeconds(patrolDuration);
        }
    }

    // 讓外部 (如 GameController) 可以強制進入追擊 (用於第二階段)
    public void ForceChaseMode()
    {
        if (aiRoutine != null) StopCoroutine(aiRoutine);
        currentState = GhostState.Chase;
        agent.speed = 4.0f; // 第二階段可以更快
        Debug.Log("強制進入第二階段追擊！");
    }
}