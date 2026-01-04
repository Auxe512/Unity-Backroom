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

    [Header("天花板與燈光設定 (Backrooms)")]
    [SerializeField]
    private GameObject _ceilingPlainPrefab; // 普通天花板 (無燈)

    [SerializeField]
    private GameObject _ceilingLightPrefab; // 燈光天花板 (有燈)

    [SerializeField, Range(0f, 1f)]
    private float _lightSpawnChance = 0.05f;

    [Header("收集物設定")]
    [SerializeField]
    private GameObject _pelletPrefab; // 普通豆子 Prefab

    // --- 【修改】新增大力丸設定 ---
    [SerializeField]
    private GameObject _powerPelletPrefab; // 大力丸 Prefab

    [SerializeField, Range(0f, 1f)]
    private float _powerPelletChance = 0.05f; // 5% 機率變成大力丸
    // -------------------------

    [SerializeField, Range(0f, 1f)]
    private float _pelletSpawnChance = 0.5f;

    [Header("AI 與 生成設定")]
    [SerializeField]
    private NavMeshSurface _navSurface;

    [SerializeField]
    private GameObject _ghostPrefab;

    [SerializeField]
    private int _ghostCount = 4;

    [SerializeField]
    private GameObject _playerPrefab; // 拖入玩家 Prefab

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

                // 生成天花板
                SpawnCeiling(cell);
            }
        }

        // 2. 生成迷宮路徑
        GenerateMaze(null, _mazeGrid[0, 0]);

        // 3. 打通額外牆壁
        RemoveExtraWalls();

        // 4. 生成豆子 (包含大力丸邏輯)
        SpawnPellets();

        // 5. 烘焙 NavMesh
        if (_navSurface != null)
        {
            Physics.SyncTransforms();
            _navSurface.BuildNavMesh();
        }

        // 6. 生成鬼怪
        SpawnGhosts();
        /*
        // 7. 生成玩家 (解除註解)
        SpawnPlayer();
        */
    }

    // --- 天花板生成邏輯 ---
    private void SpawnCeiling(MazeCell parentCell)
    {
        if (_ceilingPlainPrefab == null || _ceilingLightPrefab == null) return;

        GameObject prefabToUse = _ceilingPlainPrefab;
        if (Random.value < _lightSpawnChance)
        {
            prefabToUse = _ceilingLightPrefab;
        }

        float ceilingHeight = 1.53f;
        Vector3 spawnPos = parentCell.transform.position + Vector3.up * ceilingHeight;
        
        // 轉 -90 度讓 Quad 面朝下
        GameObject ceiling = Instantiate(prefabToUse, spawnPos, Quaternion.Euler(-90f, 0f, 0f));
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

    // --- 【修改】生成豆子與大力丸 ---
    private void SpawnPellets()
    {
        if (_pelletPrefab == null) return;

        int count = 0;
        int powerCount = 0;

        for (int x = 0; x < _mazeWidth; x++)
        {
            for (int z = 0; z < _mazeDepth; z++)
            {
                if (x < 3 && z < 3) continue;

                if (Random.value < _pelletSpawnChance)
                {
                    // 預設生成普通豆子
                    GameObject prefabToUse = _pelletPrefab;

                    // 判斷是否生成大力丸
                    if (_powerPelletPrefab != null && Random.value < _powerPelletChance)
                    {
                        prefabToUse = _powerPelletPrefab;
                        powerCount++;
                    }

                    Vector3 pos = new Vector3(x, 0.15f, z);
                    GameObject pellet = Instantiate(prefabToUse, pos, Quaternion.identity);
                    pellet.transform.parent = transform;
                    count++;
                }
            }
        }
        Debug.Log($"生成了 {count} 顆收集物 (包含 {powerCount} 顆大力丸)");
    }

    // --- 生成鬼怪 ---
    private void SpawnGhosts()
    {
        if (_ghostPrefab == null) return;

        for (int i = 0; i < _ghostCount; i++)
        {
            int x, z;
            int attempts = 0;
            do
            {
                x = Random.Range(5, _mazeWidth - 1);
                z = Random.Range(5, _mazeDepth - 1);
                attempts++;
            } while (x < 5 && z < 5 && attempts < 100);

            Vector3 randomPos = new Vector3(x, 0, z);
            NavMeshHit hit;

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

    // --- 【修改】生成玩家 (解除註解) ---
    /*
    private void SpawnPlayer()
    {
        if (_playerPrefab == null)
        {
            Debug.LogWarning("注意：MazeGenerator 尚未設定 Player Prefab，無法生成玩家。");
            return;
        }

        // 生成在 (0, 1, 0) 避免卡在地板
        Vector3 startPos = new Vector3(0, 1.0f, 0);
        Instantiate(_playerPrefab, startPos, Quaternion.identity);
    }
    */
}