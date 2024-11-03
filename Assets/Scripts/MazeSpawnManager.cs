using UnityEngine;
using System.Collections.Generic;

public class MazeSpawnManager : MonoBehaviour
{
    [SerializeField] private MazeGenerator mazeGenerator;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int numberOfEnemies = 5;
    [SerializeField] private float minEnemyDistanceFromPlayer = 5f;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float playerRadius = 0.4f;
    [SerializeField] private float enemyRadius = 0.4f; // Add this for enemy collision check
    [SerializeField] private LayerMask wallLayer; // Add this for wall detection

    private Vector3 playerPosition;
    private List<Vector3> usedPositions = new List<Vector3>();

    private void Start()
    {
        MazeCell[,] maze = mazeGenerator.GetMaze();
        SpawnPlayer(maze);
        SpawnEnemies(maze);
    }

    private bool IsPositionClear(Vector3 position, float radius)
    {
        // Check overlap with a slightly smaller radius to ensure no wall touching
        float checkRadius = radius * 0.9f;

        // Check if position overlaps with any colliders
        Collider[] hitColliders = Physics.OverlapSphere(position, checkRadius, wallLayer);
        if (hitColliders.Length > 0)
            return false;

        // Cast rays in main directions to ensure enough clearance
        Vector3[] directions = new Vector3[]
        {
            Vector3.forward, Vector3.back, Vector3.left, Vector3.right,
            (Vector3.forward + Vector3.right).normalized,
            (Vector3.forward + Vector3.left).normalized,
            (Vector3.back + Vector3.right).normalized,
            (Vector3.back + Vector3.left).normalized
        };

        foreach (Vector3 dir in directions)
        {
            if (Physics.Raycast(position, dir, radius * 1.2f, wallLayer))
                return false;
        }

        return true;
    }

    private void SpawnPlayer(MazeCell[,] maze)
    {
        for (int x = 1; x < mazeGenerator.mazeWidth - 1; x++)
        {
            for (int y = 1; y < mazeGenerator.mazeHeight - 1; y++)
            {
                MazeCell cell = maze[x, y];
                MazeCell cellRight = maze[x + 1, y];
                MazeCell cellUp = maze[x, y + 1];

                if (!cell.topWall && !cell.leftWall &&
                    !cellRight.leftWall && !cellUp.topWall)
                {
                    float worldX = x * cellSize + cellSize;
                    float worldZ = y * cellSize + cellSize;
                    Vector3 potentialPos = new Vector3(worldX, 0.08f, worldZ);

                    if (IsPositionClear(potentialPos, playerRadius))
                    {
                        playerPosition = potentialPos;
                        Instantiate(playerPrefab, playerPosition, Quaternion.identity);
                        usedPositions.Add(playerPosition);
                        return;
                    }
                }
            }
        }

        // Fallback spawn logic
        for (int x = 0; x < mazeGenerator.mazeWidth; x++)
        {
            for (int y = 0; y < mazeGenerator.mazeHeight; y++)
            {
                MazeCell cell = maze[x, y];
                if (!cell.topWall && !cell.leftWall)
                {
                    float worldX = x * cellSize + (cellSize / 2);
                    float worldZ = y * cellSize + (cellSize / 2);
                    Vector3 potentialPos = new Vector3(worldX, 0.08f, worldZ);

                    if (IsPositionClear(potentialPos, playerRadius))
                    {
                        playerPosition = potentialPos;
                        Instantiate(playerPrefab, playerPosition, Quaternion.identity);
                        usedPositions.Add(playerPosition);
                        return;
                    }
                }
            }
        }

        Debug.LogWarning("Could not find clear position for player!");
    }

    private void SpawnEnemies(MazeCell[,] maze)
    {
        int enemiesSpawned = 0;
        int maxAttempts = 1000;
        int attempts = 0;

        while (enemiesSpawned < numberOfEnemies && attempts < maxAttempts)
        {
            int x = Random.Range(0, mazeGenerator.mazeWidth);
            int y = Random.Range(0, mazeGenerator.mazeHeight);

            MazeCell cell = maze[x, y];

            if (!cell.topWall && !cell.leftWall)
            {
                // Add some randomness to position within the cell
                float offsetX = Random.Range(-0.3f, 0.3f);
                float offsetZ = Random.Range(-0.3f, 0.3f);

                float worldX = x * cellSize + (cellSize / 2) + offsetX;
                float worldZ = y * cellSize + (cellSize / 2) + offsetZ;
                Vector3 potentialPos = new Vector3(worldX, 0.08f, worldZ);

                // Check distance from player and other enemies
                bool tooClose = false;
                foreach (Vector3 usedPos in usedPositions)
                {
                    if (Vector3.Distance(potentialPos, usedPos) < minEnemyDistanceFromPlayer)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose && IsPositionClear(potentialPos, enemyRadius))
                {
                    Instantiate(enemyPrefab, potentialPos, Quaternion.identity);
                    usedPositions.Add(potentialPos);
                    enemiesSpawned++;
                }
            }
            attempts++;
        }

        if (enemiesSpawned < numberOfEnemies)
        {
            Debug.LogWarning($"Could only spawn {enemiesSpawned} out of {numberOfEnemies} enemies!");
        }
    }
}