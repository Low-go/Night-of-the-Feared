using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class MazeRenderer : MonoBehaviour
{
    [SerializeField] MazeGenerator mazeGenerator;
    [SerializeField] GameObject MazeCellPrefab;
    private NavMeshSurface mazeNavMeshSurface;

    public float CellSize = 1f;

    private void Awake()
    {
        // Add and configure NavMeshSurface
        mazeNavMeshSurface = gameObject.AddComponent<NavMeshSurface>();
        if (mazeNavMeshSurface != null)
        {
            Debug.Log("NavMeshSurface component added successfully");
            mazeNavMeshSurface.collectObjects = CollectObjects.Children;
            mazeNavMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        }
        else
        {
            Debug.LogError("Failed to add NavMeshSurface component!");
        }
    }

    private void Start()
    {
        Debug.Log("Starting maze generation...");
        GenerateMaze();
        StartCoroutine(BakeNavMeshWhenReady());
    }

    private void GenerateMaze()
    {
        MazeCell[,] maze = mazeGenerator.GetMaze();

        for (int x = 0; x < mazeGenerator.mazeWidth; x++)
        {
            for (int y = 0; y < mazeGenerator.mazeHeight; y++)
            {
                GameObject newCell = Instantiate(MazeCellPrefab,
                    new Vector3((float)x * CellSize, 0f, (float)y * CellSize),
                    Quaternion.identity,
                    transform);

                MazeCellObject mazeCell = newCell.GetComponent<MazeCellObject>();

                bool top = maze[x, y].topWall;
                bool left = maze[x, y].leftWall;
                bool right = x == mazeGenerator.mazeWidth - 1;
                bool bottom = y == 0;

                mazeCell.Init(top, bottom, right, left);
            }
        }
        Debug.Log($"Maze generated with dimensions: {mazeGenerator.mazeWidth}x{mazeGenerator.mazeHeight}");
    }

    private IEnumerator BakeNavMeshWhenReady()
    {
        // Wait one frame to ensure all maze cells are properly instantiated
        yield return new WaitForEndOfFrame();

        Debug.Log("Starting NavMesh baking process...");

        if (mazeNavMeshSurface == null)
        {
            Debug.LogError("NavMeshSurface is null! Baking failed.");
            yield break;
        }

        // Clear any existing NavMesh data
        mazeNavMeshSurface.RemoveData();
        Debug.Log("Cleared existing NavMesh data");

        // Bake new NavMesh
        mazeNavMeshSurface.BuildNavMesh();

        // Verify the baking
        VerifyNavMeshBaking();
    }

    private void VerifyNavMeshBaking()
    {
        if (mazeNavMeshSurface.navMeshData == null)
        {
            Debug.LogError("NavMesh baking failed - no NavMesh data generated!");
            return;
        }

        // Test center point
        Vector3 mazeCenter = new Vector3(
            (mazeGenerator.mazeWidth * CellSize) / 2f,
            0f,
            (mazeGenerator.mazeHeight * CellSize) / 2f
        );

        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(mazeCenter, out hit, 1.0f, UnityEngine.AI.NavMesh.AllAreas))
        {
            Debug.Log("NavMesh baking successful! Center point is navigable.");
            Debug.Log($"Total maze area covered: {mazeGenerator.mazeWidth * mazeGenerator.mazeHeight * CellSize} square units");
        }
        else
        {
            Debug.LogWarning("NavMesh verification failed - center point is not navigable!");
        }
    }

    // Optional: Public method to force rebake if needed
    public void RebakeNavMesh()
    {
        Debug.Log("Manual NavMesh rebake requested...");
        StartCoroutine(BakeNavMeshWhenReady());
    }
}