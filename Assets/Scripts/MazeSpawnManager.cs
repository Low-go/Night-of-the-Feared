using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;


public class MazeSpawnManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MazeGenerator mazeGenerator;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyPrefab;

    [Header("Enemy Spawn Settings")]
    [SerializeField] private int numberOfEnemies = 5;
    [SerializeField] private float minEnemyDistanceFromPlayer = 5f;
    [SerializeField] private float minDistanceBetweenPatrolPoints = 3f;
    [SerializeField] private int patrolPointsPerEnemy = 3;

    [Header("Size Settings")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float playerRadius = 0.4f;
    [SerializeField] private float enemyRadius = 0.4f;
    [SerializeField] private LayerMask wallLayer;

    // Structure to hold patrol route data
    public class PatrolRoute
    {
        public Vector3 spawnPoint;
        public List<Vector3> patrolPoints;
        public PatrolRoute(Vector3 spawn)
        {
            spawnPoint = spawn;
            patrolPoints = new List<Vector3>();
        }
    }

    private Vector3 playerPosition;
    private List<Vector3> usedPositions = new List<Vector3>();
    private List<PatrolRoute> patrolRoutes = new List<PatrolRoute>();

    public List<PatrolRoute> PatrolRoutes => patrolRoutes;
    private void Start()
    {
        StartCoroutine(InitializeSpawning());
    }


    private IEnumerator InitializeSpawning()
    {
        // Wait for one frame to ensure MazeRenderer has started
        yield return null;

        // Wait until NavMesh is available
        while (!IsNavMeshReady())
        {
            yield return new WaitForSeconds(0.1f);
        }

        // Additional safety delay
        yield return new WaitForSeconds(0.2f);

        Debug.Log("NavMesh is ready, proceeding with enemy spawning...");

        // Now proceed with spawning
        MazeCell[,] maze = mazeGenerator.GetMaze();
        SpawnPlayer(maze);
        GeneratePatrolRoutesAndSpawnEnemies(maze);
    }

    private bool IsNavMeshReady()
    {
        // Sample a point in the center of the maze
        Vector3 testPoint = new Vector3(
            (mazeGenerator.mazeWidth * cellSize) / 2f,
            0f,
            (mazeGenerator.mazeHeight * cellSize) / 2f
        );

        NavMeshHit hit;
        // Check if we can find a nearby nav mesh surface
        return NavMesh.SamplePosition(testPoint, out hit, 1.0f, NavMesh.AllAreas);
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

    private void GeneratePatrolRoutesAndSpawnEnemies(MazeCell[,] maze)
    {
        int enemiesSpawned = 0;
        int maxAttempts = 1000;
        int attempts = 0;

        while (enemiesSpawned < numberOfEnemies && attempts < maxAttempts)
        {
            // Generate spawn position
            Vector3? spawnPos = FindValidSpawnPosition(maze);
            if (!spawnPos.HasValue)
            {
                attempts++;
                continue;
            }

            // Generate patrol points for this enemy
            PatrolRoute route = new PatrolRoute(spawnPos.Value);
            if (GeneratePatrolPointsForRoute(route, maze))
            {
                // Spawn enemy and assign patrol route
                GameObject enemy = Instantiate(enemyPrefab, route.spawnPoint, Quaternion.identity);

                // Add NavMeshAgent if not already on prefab
                NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
                if (agent == null)
                    agent = enemy.AddComponent<NavMeshAgent>();

                patrolRoutes.Add(route);
                usedPositions.Add(route.spawnPoint);
                enemiesSpawned++;
            }

            attempts++;
        }

        if (enemiesSpawned < numberOfEnemies)
        {
            Debug.LogWarning($"Could only spawn {enemiesSpawned} out of {numberOfEnemies} enemies!");
        }
    }

    private Vector3? FindValidSpawnPosition(MazeCell[,] maze)
    {
        for (int i = 0; i < 100; i++)
        {
            int x = Random.Range(0, mazeGenerator.mazeWidth);
            int y = Random.Range(0, mazeGenerator.mazeHeight);

            if (!maze[x, y].topWall && !maze[x, y].leftWall)
            {
                float offsetX = Random.Range(-0.3f, 0.3f);
                float offsetZ = Random.Range(-0.3f, 0.3f);

                float worldX = x * cellSize + (cellSize / 2) + offsetX;
                float worldZ = y * cellSize + (cellSize / 2) + offsetZ;
                Vector3 potentialPos = new Vector3(worldX, 0.08f, worldZ);

                if (IsValidPatrolPoint(potentialPos))
                {
                    return potentialPos;
                }
            }
        }
        return null;
    }

    private bool GeneratePatrolPointsForRoute(PatrolRoute route, MazeCell[,] maze)
    {
        int maxAttempts = 50;

        for (int i = 0; i < patrolPointsPerEnemy; i++)
        {
            Vector3? patrolPoint = FindPatrolPoint(route, maze, maxAttempts);
            if (!patrolPoint.HasValue)
                return false;

            route.patrolPoints.Add(patrolPoint.Value);
        }

        return route.patrolPoints.Count == patrolPointsPerEnemy;
    }

    private Vector3? FindPatrolPoint(PatrolRoute route, MazeCell[,] maze, int maxAttempts)
    {
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Generate a random point within reasonable distance
            float angle = Random.Range(0f, 360f);
            float distance = Random.Range(minDistanceBetweenPatrolPoints, minDistanceBetweenPatrolPoints * 2f);

            Vector3 offset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
                0f,
                Mathf.Sin(angle * Mathf.Deg2Rad) * distance
            );

            Vector3 potentialPoint = route.spawnPoint + offset;

            // Convert to maze coordinates
            int mazeX = Mathf.FloorToInt(potentialPoint.x / cellSize);
            int mazeY = Mathf.FloorToInt(potentialPoint.z / cellSize);

            // Check if within maze bounds
            if (mazeX >= 0 && mazeX < mazeGenerator.mazeWidth - 1 &&
                mazeY >= 0 && mazeY < mazeGenerator.mazeHeight - 1)
            {
                if (IsValidPatrolPoint(potentialPoint))
                {
                    // Verify NavMesh path exists
                    NavMeshPath path = new NavMeshPath();
                    if (NavMesh.CalculatePath(route.spawnPoint, potentialPoint, NavMesh.AllAreas, path))
                    {
                        if (path.status == NavMeshPathStatus.PathComplete)
                        {
                            return potentialPoint;
                        }
                    }
                }
            }
        }

        return null;
    }

    private bool IsValidPatrolPoint(Vector3 point)
    {
        // Check if point is clear of obstacles
        if (!IsPositionClear(point, enemyRadius))
            return false;

        // Check distance from player
        if (Vector3.Distance(point, playerPosition) < minEnemyDistanceFromPlayer)
            return false;

        // Check distance from other patrol points
        foreach (Vector3 usedPos in usedPositions)
        {
            if (Vector3.Distance(point, usedPos) < minDistanceBetweenPatrolPoints)
                return false;
        }

        return true;
    }
}