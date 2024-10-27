using UnityEngine;
public class CeilingGenerator : MonoBehaviour
{
    [SerializeField] GameObject ceilingTilePrefab;
    [SerializeField] MazeGenerator mazeGenerator;
    private float ceilingHeight = 1.07f;  // This is your desired height
    public int tilesPerCell = 4;

    private void Start()
    {
        StartCoroutine(GenerateCeilingNextFrame());
    }

    private System.Collections.IEnumerator GenerateCeilingNextFrame()
    {
        yield return new WaitForEndOfFrame();
        GenerateCeiling();
    }

    public void GenerateCeiling()
    {
        int ceilingWidth = Mathf.CeilToInt((float)mazeGenerator.mazeWidth / tilesPerCell);
        int ceilingLength = Mathf.CeilToInt((float)mazeGenerator.mazeHeight / tilesPerCell);  // Renamed to avoid confusion

        for (int x = 0; x < ceilingWidth; x++)
        {
            for (int z = 0; z < ceilingLength; z++)
            {
                Vector3 position = new Vector3(
                    x * 4f,
                    ceilingHeight,  // Now using the class field for height
                    z * 4f
                );
                GameObject tile = Instantiate(ceilingTilePrefab, position, Quaternion.identity);
                tile.transform.parent = transform;
            }
        }
    }
}
