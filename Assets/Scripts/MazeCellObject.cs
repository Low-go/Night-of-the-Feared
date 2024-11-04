using UnityEngine;
using UnityEngine.AI;

public class MazeCellObject : MonoBehaviour
{
    [SerializeField] GameObject topWall;
    [SerializeField] GameObject bottomWall;
    [SerializeField] GameObject leftWall;
    [SerializeField] GameObject rightWall;
    [SerializeField] GameObject floor; // Reference to your floor object

    private void Awake()
    {
        SetupWall(topWall);
        SetupWall(bottomWall);
        SetupWall(leftWall);
        SetupWall(rightWall);

        // Setup floor if you have one
        if (floor != null)
        {
            // Ensure floor has collider
            if (floor.GetComponent<BoxCollider>() == null)
            {
                floor.AddComponent<BoxCollider>();
            }
            // Mark floor layer as walkable (typically layer 0 "Default")
            floor.layer = 0;
        }
    }

    private void SetupWall(GameObject wall)
    {
        if (wall != null)
        {
            // Ensure wall has collider
            if (wall.GetComponent<BoxCollider>() == null)
            {
                wall.AddComponent<BoxCollider>();
            }

            // Ensure wall has NavMeshObstacle
            NavMeshObstacle obstacle = wall.GetComponent<NavMeshObstacle>();
            if (obstacle == null)
            {
                obstacle = wall.AddComponent<NavMeshObstacle>();
                obstacle.carving = true;
                obstacle.carveOnlyStationary = true;
                // Set the size to match your wall's actual size
                obstacle.size = wall.GetComponent<BoxCollider>().size;
            }

            // Set to obstacle layer
            wall.layer = LayerMask.NameToLayer("Obstacle");
        }
    }

    public void Init(bool top, bool bottom, bool right, bool left)
    {
        topWall.SetActive(top);
        bottomWall.SetActive(bottom);
        leftWall.SetActive(left);
        rightWall.SetActive(right);
    }
}