using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeRender : MonoBehaviour
{
    [SerializeField] MazeGenerator mazeGenerator;
    [SerializeField] GameObject MazeCellPrefab;

    public float CellSize = 1f;

    private void Start()
    {
        MazeCell[,] maze = mazeGenerator.GetMaze();

        for (int x = 0; x < mazeGenerator.mazeWidth; x++)
        {
            for (int y = 0; y < mazeGenerator.mazeHeight; y++)
            {
                GameObject newCell = Instantiate(MazeCellPrefab, new Vector3((float)x * CellSize, 0f, (float)y * CellSize), Quaternion.identity, transform);

                MazeCellObject mazeCell = newCell.GetComponent<MazeCellObject>();

                bool top = maze[x, y].topWall;
                bool left = maze[x, y].leftWall;

                bool right;
                if( x == mazeGenerator.mazeWidth - 1)
                {
                    right = true;
                }
                else
                {
                    right = maze[x + 1, y].leftWall;
                }
                bool bottom;
                if (y == 0)
                    bottom = true;
                else
                    bottom = maze[x, y - 1].topWall;
                
                mazeCell.Init(top, bottom, right, left);
            }
        }
    }
}
