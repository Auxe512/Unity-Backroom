using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.AI.Navigation; // 1. 務必引用這個命名空間 (用於導航)

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
    [Tooltip("越高代表走廊越長、轉彎越少")]
    private float _straightPathChance = 0.8f; // 預設 80% 直線

    [SerializeField, Range(0f, 1f)]
    [Tooltip("越高代表牆壁越少、空間越空曠")]
    private float _extraWallRemovalChance = 0.3f; // 預設 30% 打通牆壁

    [Header("AI 與 生成設定")]
    [SerializeField]
    private NavMeshSurface _navSurface; // 拖入掛有 NavMeshSurface 的物件 (通常是 MazeGenerator 自己)

    [SerializeField]
    private GameObject _ghostPrefab; // 拖入鬼怪的 Prefab

    [SerializeField]
    private int _ghostCount = 4; // 要生成幾隻鬼

    [Header("Bean 設定")]
    [SerializeField]
    private GameObject _beanPrefab;

    [SerializeField]
    private float _beanHeight = 0.3f; // 豆子離地高度

    private MazeCell[,] _mazeGrid;

    void Start()
    {
        // 0. 清除舊場景 (防止重複生成)
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
                // 生成格子並設定父物件
                var cell = Instantiate(_mazeCellPrefab, new Vector3(x, 0, z), Quaternion.identity);
                cell.transform.parent = transform;
                _mazeGrid[x, z] = cell;
            }
        }

        // 2. 生成主要路徑 (遞迴回溯法)
        GenerateMaze(null, _mazeGrid[0, 0]);

        // 3. 後期處理：隨機打通牆壁，讓空間更空曠
        RemoveExtraWalls();

        // 3.5 生成bean
        SpawnBeans();

        // 4. 烘焙導航網格 (讓鬼知道路)
        if (_navSurface != null)
        {
            _navSurface.BuildNavMesh();
        }
        else
        {
            Debug.LogError("請在 Inspector 中指派 NavMeshSurface！");
        }

        // 5. 生成鬼怪
        SpawnGhosts();
    }

    private void GenerateMaze(MazeCell previousCell, MazeCell currentCell)
    {
        currentCell.Visit();
        ClearWalls(previousCell, currentCell);

        MazeCell nextCell;

        do
        {
            nextCell = GetNextUnvisitedCell(currentCell, previousCell);

            if (nextCell != null)
            {
                GenerateMaze(currentCell, nextCell);
            }
        } while (nextCell != null);
    }

    private MazeCell GetNextUnvisitedCell(MazeCell currentCell, MazeCell previousCell)
    {
        var unvisitedCells = GetUnvisitedCells(currentCell).ToList();

        if (unvisitedCells.Count == 0) return null;

        // --- 直線優先邏輯 ---
        if (previousCell != null && Random.value < _straightPathChance)
        {
            int xDir = (int)(currentCell.transform.position.x - previousCell.transform.position.x);
            int zDir = (int)(currentCell.transform.position.z - previousCell.transform.position.z);

            var forwardCell = unvisitedCells.FirstOrDefault(cell =>
                (int)(cell.transform.position.x - currentCell.transform.position.x) == xDir &&
                (int)(cell.transform.position.z - currentCell.transform.position.z) == zDir
            );

            if (forwardCell != null)
            {
                return forwardCell;
            }
        }
        // ------------------

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
                    // 這裡使用 GetNeighbors 取得所有鄰居 (包含走過的)
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

    // --- 輔助函式 ---

    // 1. 取得所有鄰居 (不管有無訪問) - 用於打通牆壁
    private IEnumerable<MazeCell> GetNeighbors(MazeCell currentCell)
    {
        int x = (int)currentCell.transform.position.x;
        int z = (int)currentCell.transform.position.z;

        if (x + 1 < _mazeWidth) yield return _mazeGrid[x + 1, z];
        if (x - 1 >= 0) yield return _mazeGrid[x - 1, z];
        if (z + 1 < _mazeDepth) yield return _mazeGrid[x, z + 1];
        if (z - 1 >= 0) yield return _mazeGrid[x, z - 1];
    }

    // 2. 取得未訪問的鄰居 - 用於迷宮生成
    private IEnumerable<MazeCell> GetUnvisitedCells(MazeCell currentCell)
    {
        return GetNeighbors(currentCell).Where(c => c.IsVisited == false);
    }

    // 3. 生成鬼怪
    private void SpawnGhosts()
    {
        if (_ghostPrefab == null) return;

        for (int i = 0; i < _ghostCount; i++)
        {
            int x, z;
            // 簡單防呆：避開 (0,0) ~ (5,5) 的玩家出生區域
            // 同時確保不會生在牆壁裡 (雖然 NavMeshAgent 會自動修正，但以防萬一)
            do
            {
                x = Random.Range(5, _mazeWidth - 1); // 避開邊界
                z = Random.Range(5, _mazeDepth - 1);
            } while (x < 5 && z < 5);

            // 修改這裡：把 Y 軸從 0 改成 0.5f 或 1.0f
            // 這樣鬼怪會從空中掉下來，確保不會卡在地板裡
            Vector3 spawnPos = new Vector3(x, 0.5f, z);

            Instantiate(_ghostPrefab, spawnPos, Quaternion.identity);
        }
    }

    // 3.5 生成豆子
    private void SpawnBeans()
    {
        if (_beanPrefab == null)
        {
            Debug.LogWarning("尚未指派 Bean Prefab");
            return;
        }

        for (int x = 0; x < _mazeWidth; x++)
        {
            for (int z = 0; z < _mazeDepth; z++)
            {
                MazeCell cell = _mazeGrid[x, z];

                // 可選：避免在起點附近放豆子
                if (x < 2 && z < 2) continue;

                // 直接放在格子中心
                Vector3 beanPos = cell.transform.position + Vector3.up * _beanHeight;

                Instantiate(_beanPrefab, beanPos, Quaternion.identity, transform);
            }
        }
    }
}