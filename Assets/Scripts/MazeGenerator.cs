using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    // 起點設在底部外環的中間
    public int startX = 10, startY = 1;

    // --- 極簡版地圖 (21x21) ---
    // 1 = 牆壁, 0 = 路
    // 這個地圖非常單純，只有 "外環" + "大十字" + "四個方塊區"
    string[] mapLayout = new string[]
    {
        "111111111111111111111", // [20] 頂部邊界 (封死)
        "100000000010000000001", // [19] 北部外環 (全通)
        "101111111010111111101", // [18] 牆壁
        "101000001010100000101", // [17] 
        "101011101010101110101", // [16] 左上 & 右上 的方形房間
        "101011101010101110101", // [15]
        "101000000000000000101", // [14] 寬敞通道
        "101111101000101111101", // [13] 
        "100000000010000000001", // [12] 接近中心
        "111111101101101111111", // [11] ★ 中央區 (只有左右通)
        "100000000010000000001", // [10] 接近中心
        "101111101000101111101", // [09] 
        "101000000000000000101", // [08] 寬敞通道
        "101011101010101110101", // [07] 
        "101011101010101110101", // [06] 左下 & 右下 的方形房間
        "101000001010100000101", // [05] 
        "101111111010111111101", // [04] 牆壁
        "100000000010000000001", // [03] 入口分流
        "101111101111101111101", // [02] 底部屏風牆
        "100000000010000000001", // [01] 底部外環 (全通，起點在這)
        "111111111111111111111"  // [00] 底部邊界 (封死)
    };

    public int mazeWidth
    {
        get
        {
            if (mapLayout == null || mapLayout.Length == 0) return 0;
            return mapLayout[0].Length;
        }
    }

    public int mazeHeight
    {
        get
        {
            if (mapLayout == null) return 0;
            return mapLayout.Length;
        }
    }

    MazeCell[,] maze;

    public MazeCell[,] GetMaze()
    {
        int height = mazeHeight;
        int width = mazeWidth;

        maze = new MazeCell[width, height];

        // 1. 初始化
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                maze[x, y] = new MazeCell(x, y);
                // 預設全部通暢，只有遇到 '1' 才加牆
                maze[x, y].topWall = false;
                maze[x, y].leftWall = false;
            }
        }

        // 2. 讀取地圖
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 倒序讀取
                char cellType = mapLayout[height - 1 - y][x];

                if (cellType == '1')
                {
                    maze[x, y].topWall = true;
                    maze[x, y].leftWall = true;
                }
            }
        }

        return maze;
    }
}

// 基礎類別定義 (必須保留)
public enum Direction { Up, Down, Left, Right }

public class MazeCell
{
    public bool visited;
    public int x, y;
    public bool topWall;
    public bool leftWall;

    public Vector2Int position { get { return new Vector2Int(x, y); } }

    public MazeCell(int x, int y)
    {
        this.x = x;
        this.y = y;
        visited = false;
        topWall = leftWall = false;
    }
}