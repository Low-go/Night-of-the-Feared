using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{

    [Range(5, 500)]
    public int mazeWidth = 20, mazeHeight = 30;   // dimensions
    public int startX, startY;
    MazeCell[,] maze;

    Vector2Int currentCell;

    public MazeCell[,] GetMaze()
    {
        maze = new MazeCell[mazeWidth, mazeHeight];

        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                maze[x, y] = new MazeCell(x, y);
            }
        }

        //start carving path
        CarvePath(startX, startY);

        return maze;
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


    Vector2Int CheckNeighbours()
    {
        List<Direction> rndDir = GetRandomDirections();

        for (int i = 0; i < rndDir.Count; i++)
        {

            Vector2Int neighbour = currentCell;

            //modify neighbour coordinates depending on where we are at
            switch (rndDir[i])
            {

                case Direction.Up:
                    neighbour.y++;
                    break;
                case Direction.Down:
                    neighbour.y--;
                    break;
                case Direction.Right:
                    neighbour.x++;
                    break;
                case Direction.Left:
                    neighbour.x--;
                    break;

            }

            if (IsCellValid(neighbour.x, neighbour.y)) return neighbour;

        }

        return currentCell;
    }

    //works out which walls we are going to break at a time
    void BreakWalls(Vector2Int primaryCell, Vector2Int secondaryCell)
    {
        if(primaryCell.x > secondaryCell.x){ //primary cell's left wall
            maze[primaryCell.x, primaryCell.y].leftWall = false;
        }
        else if (primaryCell.x < secondaryCell.x)
        {
            maze[secondaryCell.x, secondaryCell.y].leftWall = false;
        }
        else if (primaryCell.y < secondaryCell.y)
        { 
            maze[primaryCell.x, primaryCell.y].topWall = false;
        }
        else if (primaryCell.y > secondaryCell.y)
        {
            maze[secondaryCell.x, secondaryCell.y].topWall = false;
        }
    }

    void CarvePath(int x, int y)
    {
        if (x <0 || y < 0 || x > mazeWidth -1 || y > mazeWidth - 1) //just checking if they gave negative number, if so we throw to 0
        {
            x = y = 0;
            Debug.LogWarning("Starting position is out of bounds, defaulting to 0, 0");
        }

        // set current cell to starting position we were passed
        currentCell = new Vector2Int(x, y);

        List<Vector2Int> path = new List<Vector2Int>();
        

        // loop until we hit a dead end
        bool deadEnd = false;

        while (!deadEnd)
        {
            //we will try the next cell
            Vector2Int nextCell = CheckNeighbours(); // function call

            if (nextCell == currentCell)
            {
                // if the cell has no valid neighbours set deadend to true so we break out of loop
                for (int i = path.Count - 1; i >= 0; i--)
                {
                    currentCell = path[i];
                    path.RemoveAt(i);
                    nextCell = CheckNeighbours();

                    //if we find valid neighbours break out of loop
                    if (nextCell != currentCell) break;
                }

                if (nextCell == currentCell) {
                    deadEnd = true;
                }
            }
            else
            {
                BreakWalls(currentCell, nextCell); // set wall flags
                maze[currentCell.x, currentCell.y].visited = true; // set cells visited before moving on
                currentCell = nextCell;
                path.Add(currentCell);  // add cell to path
            }
        }
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
