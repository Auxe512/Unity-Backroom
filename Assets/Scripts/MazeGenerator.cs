using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class MazeGenerator : MonoBehaviour
{
    [Header("基礎設定")]
    [SerializeField]
    private MazeCell _mazeCellPrefab; // 迷宮格子的 Prefab

    [SerializeField]
    private int _mazeWidth = 20;

    [SerializeField]
    private int _mazeDepth = 20;

    [Header("迷宮風格設定")]
    [SerializeField, Range(0f, 1f)]
    private float _straightPathChance = 0.8f;

    [SerializeField, Range(0f, 1f)]
    private float _extraWallRemovalChance = 0.3f;

    // --- 【新增】天花板與燈光設定 ---
    [Header("天花板與燈光設定 (Backrooms)")]
    [SerializeField]
    private GameObject _ceilingPlainPrefab; // 普通天花板 (無燈)

    [SerializeField]
    private GameObject _ceilingLightPrefab; // 燈光天花板 (有燈)

    [SerializeField, Range(0f, 1f)]
    private float _lightSpawnChance = 0.05f; // 燈光機率 (建議設低一點，例如 0.05，讓場景更暗更恐怖)

    [Header("收集物設定")]
    [SerializeField]
    private GameObject _pelletPrefab; // 豆子 Prefab

    [SerializeField, Range(0f, 1f)]
    private float _pelletSpawnChance = 0.5f;

    [Header("AI 與 生成設定")]
    [SerializeField]
    private NavMeshSurface _navSurface;

    [SerializeField]
    private GameObject _ghostPrefab;

    [SerializeField]
    private int _ghostCount = 4;

    private MazeCell[,] _mazeGrid;

    void Start()
    {
        // 0. 清除舊場景物件
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // 1. 初始化網格
        _mazeGrid = new MazeCell[_mazeWidth, _mazeDepth];

        for (int x = 0; x < _mazeWidth; x++)
        {
            for (int z = 0; z < _mazeDepth; z++)
            {
                Vector3 cellPos = new Vector3(x, 0, z);

                // 生成地板與牆壁
                var cell = Instantiate(_mazeCellPrefab, cellPos, Quaternion.identity);
                cell.transform.parent = transform;
                _mazeGrid[x, z] = cell;

                // 【生成天花板】
                SpawnCeiling(cell);
            }
        }

        // 2. 生成迷宮路徑
        GenerateMaze(null, _mazeGrid[0, 0]);

        // 3. 打通額外牆壁
        RemoveExtraWalls();

        // 4. 生成豆子
        SpawnPellets();

        // 5. 烘焙 NavMesh
        if (_navSurface != null)
        {
            Physics.SyncTransforms(); // 強制同步位置，避免烘焙錯誤
            _navSurface.BuildNavMesh();
        }

        // 6. 生成鬼怪
        SpawnGhosts();
    }

    // --- 天花板生成邏輯 ---
    // --- 天花板生成邏輯 (使用你測試出的完美數值) ---
    private void SpawnCeiling(MazeCell parentCell)
    {
        if (_ceilingPlainPrefab == null || _ceilingLightPrefab == null) return;

        // 1. 決定用哪種天花板
        GameObject prefabToUse = _ceilingPlainPrefab;
        if (Random.value < _lightSpawnChance)
        {
            prefabToUse = _ceilingLightPrefab;
        }

        // 2. 設定高度：使用你找到的 1.53
        float ceilingHeight = 1.53f;
        Vector3 spawnPos = parentCell.transform.position + Vector3.up * ceilingHeight;

        // 3. 生成物件：使用你截圖中的旋轉角度 -90 度
        // 注意：因為你是用 Quad (平面)，所以要轉 -90 度才會面朝下
        GameObject ceiling = Instantiate(prefabToUse, spawnPos, Quaternion.Euler(-90f, 0f, 0f));

        // 4. 設定父物件
        ceiling.transform.parent = parentCell.transform;
    }

    // --- 迷宮演算法 (DFS) ---
    private void GenerateMaze(MazeCell previousCell, MazeCell currentCell)
    {
        currentCell.Visit();
        ClearWalls(previousCell, currentCell);

        MazeCell nextCell;
        do
        {
            nextCell = GetNextUnvisitedCell(currentCell, previousCell);
            if (nextCell != null) GenerateMaze(currentCell, nextCell);
        } while (nextCell != null);
    }

    private MazeCell GetNextUnvisitedCell(MazeCell currentCell, MazeCell previousCell)
    {
        var unvisitedCells = GetUnvisitedCells(currentCell).ToList();
        if (unvisitedCells.Count == 0) return null;

        if (previousCell != null && Random.value < _straightPathChance)
        {
            int xDir = (int)(currentCell.transform.position.x - previousCell.transform.position.x);
            int zDir = (int)(currentCell.transform.position.z - previousCell.transform.position.z);

            var forwardCell = unvisitedCells.FirstOrDefault(cell =>
                (int)(cell.transform.position.x - currentCell.transform.position.x) == xDir &&
                (int)(cell.transform.position.z - currentCell.transform.position.z) == zDir
            );

            if (forwardCell != null) return forwardCell;
        }
        return unvisitedCells.OrderBy(_ => Random.Range(1, 10)).FirstOrDefault();
    }

    private void RemoveExtraWalls()
    {
        for (int x = 0; x < _mazeWidth; x++)
        {
            for (int z = 0; z < _mazeDepth; z++)
            {
                if (Random.value < _extraWallRemovalChance)
                {
                    var current = _mazeGrid[x, z];
                    var neighbors = GetNeighbors(current).ToList();
                    if (neighbors.Count > 0)
                    {
                        var randomNeighbor = neighbors[Random.Range(0, neighbors.Count)];
                        ClearWalls(current, randomNeighbor);
                    }
                }
            }
        }
    }

    private void ClearWalls(MazeCell previousCell, MazeCell currentCell)
    {
        if (previousCell == null) return;

        if (previousCell.transform.position.x < currentCell.transform.position.x)
        {
            previousCell.ClearRightWall();
            currentCell.ClearLeftWall();
            return;
        }
        if (previousCell.transform.position.x > currentCell.transform.position.x)
        {
            previousCell.ClearLeftWall();
            currentCell.ClearRightWall();
            return;
        }
        if (previousCell.transform.position.z < currentCell.transform.position.z)
        {
            previousCell.ClearFrontWall();
            currentCell.ClearBackWall();
            return;
        }
        if (previousCell.transform.position.z > currentCell.transform.position.z)
        {
            previousCell.ClearBackWall();
            currentCell.ClearFrontWall();
            return;
        }
    }

    private IEnumerable<MazeCell> GetNeighbors(MazeCell currentCell)
    {
        int x = (int)currentCell.transform.position.x;
        int z = (int)currentCell.transform.position.z;
        if (x + 1 < _mazeWidth) yield return _mazeGrid[x + 1, z];
        if (x - 1 >= 0) yield return _mazeGrid[x - 1, z];
        if (z + 1 < _mazeDepth) yield return _mazeGrid[x, z + 1];
        if (z - 1 >= 0) yield return _mazeGrid[x, z - 1];
    }

    private IEnumerable<MazeCell> GetUnvisitedCells(MazeCell currentCell)
    {
        return GetNeighbors(currentCell).Where(c => c.IsVisited == false);
    }

    // --- 生成豆子 ---
    private void SpawnPellets()
    {
        if (_pelletPrefab == null) return;

        int count = 0;
        for (int x = 0; x < _mazeWidth; x++)
        {
            for (int z = 0; z < _mazeDepth; z++)
            {
                if (x < 3 && z < 3) continue;

                if (Random.value < _pelletSpawnChance)
                {
                    // 高度設為 0.15f，貼近地板
                    Vector3 pos = new Vector3(x, 0.15f, z);
                    GameObject pellet = Instantiate(_pelletPrefab, pos, Quaternion.identity);
                    pellet.transform.parent = transform;
                    count++;
                }
            }
        }
        Debug.Log($"生成了 {count} 顆豆子");
    }

    // --- 生成鬼怪 (修復 loop 錯誤) ---
    private void SpawnGhosts()
    {
        if (_ghostPrefab == null) return;

        for (int i = 0; i < _ghostCount; i++)
        {
            int x, z;
            int attempts = 0;
            // 隨機找位置，避開起點 (0~5 區域)
            do
            {
                x = Random.Range(5, _mazeWidth - 1);
                z = Random.Range(5, _mazeDepth - 1);
                attempts++;
            } while (x < 5 && z < 5 && attempts < 100);

            Vector3 randomPos = new Vector3(x, 0, z);
            NavMeshHit hit;

            // 確保生在 NavMesh 上
            if (NavMesh.SamplePosition(randomPos, out hit, 2.0f, NavMesh.AllAreas))
            {
                Instantiate(_ghostPrefab, hit.position, Quaternion.identity);
            }
            else
            {
                Instantiate(_ghostPrefab, new Vector3(x, 0.1f, z), Quaternion.identity);
            }
        }
    }
}