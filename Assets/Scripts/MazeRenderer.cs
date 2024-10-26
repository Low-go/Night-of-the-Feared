using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeRenderer : MonoBehaviour
{
    [SerializeField] MazeGenerator mazeGenerator;
    [SerializeField] GameObject MazeCellPrefab;


    //physical size of each maze cell
    public float CellSize = 1f; // this needs to correspond to the cell size or else there could be overlap

    private void Start()
    {
        //get maze generator script
        MazeCell[,] maze = mazeGenerator.GetMaze();

        for (int x =  0; x < mazeGenerator.mazeWidth; x++)
        {
            for (int y = 0; y < mazeGenerator.mazeHeight; y++)
            {
                //Instantiate
                GameObject newCell = Instantiate(MazeCellPrefab, new Vector3((float)x * CellSize, 0f, (float)y * CellSize), Quaternion.identity, transform);

                MazeCellObject mazeCell = newCell.GetComponent<MazeCellObject>();

                // Determine which cells need to be active
                bool top = maze[x, y].topWall;
                bool left = maze[x, y].leftWall;

                bool right = false;
                bool bottom = false;

                if (x == mazeGenerator.mazeWidth - 1) right = true;
                if (y == 0) bottom = true;
                if (y == 0) bottom = true;

                mazeCell.Init(top, bottom, right, left);
            }
        }
    }
}
