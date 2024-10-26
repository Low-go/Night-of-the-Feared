using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{

    [Range(5, 500)]
    public int mazeWidth = 5, mazeHeight = 5;   // dimensions
    public int startX, startY;
    MazeCell[,] maze;

    Vector2Int currentCell;

    private void Start()
    {
        maze = new MazeCell[mazeWidth, mazeHeight];

        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                maze[x, y] = new MazeCell(x, y);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    List<Direction> directions = new List<Direction>
    {
        Direction.Up, Direction.Down, Direction.Left, Direction.Right,
    };

    List<Direction> GetRandomDirections()
    {
        //make a copy of our directions list that we can mess with
        List<Direction> dir = new List<Direction>(directions);

        //make a directions list to put our randomised directions into
        List<Direction> rndDir = new List<Direction>();

        while (dir.Count > 0) // loop until our list is empty
        {
            int rnd = Random.Range(0, dir.Count);
            rndDir.Add(dir[rnd]);
            dir.RemoveAt(rnd); // remove that direction so that we can't choose it again
        }

        return rndDir;
    }

    bool IsCellValid(int x, int y)
    {
        if (x < 0 || y < 0 || x > mazeWidth - 1 || y > mazeHeight - 1 || maze[x, y].visited) return false;
        else return true;
    }


}


public enum Direction
{
    Up,
    Down,
    Left,
    Right,
}

public class MazeCell
{
    public bool visited;
    public int x, y;

    public bool topWall;
    public bool leftWall;

    //reurn x and y as a vector2int

    public Vector2Int position
    {

        get
        {
            return new Vector2Int(x, y);
        }
    }

    public MazeCell(int x, int y)
    {
        this.x = x;
        this.y = y;

        //whether the algorith has been visisted
        visited = false;

        //All walls present until algorithm removes them
        topWall = leftWall = true;
    }
}
