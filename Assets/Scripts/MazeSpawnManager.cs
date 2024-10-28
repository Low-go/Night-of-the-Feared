using UnityEngine;

public class MazeSpawnManager : MonoBehaviour
{
    [SerializeField] private MazeGenerator mazeGenerator;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float playerRadius = 0.4f; // Adjust based on your player's collider size

    private void Start()
    {
        MazeCell[,] maze = mazeGenerator.GetMaze();

        // Find cell with enough clear space around it
        for (int x = 1; x < mazeGenerator.mazeWidth - 1; x++) // Start at 1 to avoid edge
        {
            for (int y = 1; y < mazeGenerator.mazeHeight - 1; y++)
            {
                MazeCell cell = maze[x, y];
                MazeCell cellRight = maze[x + 1, y];
                MazeCell cellUp = maze[x, y + 1];

                // Check if we have a 2x2 area of clear cells
                if (!cell.topWall && !cell.leftWall &&
                    !cellRight.leftWall && !cellUp.topWall)
                {
                    // Position player in center of clear area
                    float worldX = x * cellSize + cellSize;
                    float worldZ = y * cellSize + cellSize;

                    Instantiate(playerPrefab, new Vector3(worldX, 0.08f, worldZ), Quaternion.identity);
                    return;
                }
            }
        }

        // Fallback: find single clear cell and spawn in its center
        for (int x = 0; x < mazeGenerator.mazeWidth; x++)
        {
            for (int y = 0; y < mazeGenerator.mazeHeight; y++)
            {
                MazeCell cell = maze[x, y];
                if (!cell.topWall && !cell.leftWall)
                {
                    float worldX = x * cellSize + (cellSize / 2);
                    float worldZ = y * cellSize + (cellSize / 2);

                    Instantiate(playerPrefab, new Vector3(worldX, 0.08f, worldZ), Quaternion.identity);
                    return;
                }
            }
        }

        // Ultimate fallback
        Instantiate(playerPrefab, new Vector3(cellSize, 0.08f, cellSize), Quaternion.identity);
    }
}