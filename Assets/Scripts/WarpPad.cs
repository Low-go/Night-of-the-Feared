using UnityEngine;
using UnityEngine.SceneManagement;

public class WarpPad : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "SecondLevel";
    [SerializeField] private float transitionDelay = 0.5f;
    private GameOverManager gameOverManager;

    private void Start()
    {
        gameOverManager = FindObjectOfType<GameOverManager>();
        Debug.Log("WarpPad Start - Current Scene: " + SceneManager.GetActiveScene().name);
        Debug.Log("GameOverManager found: " + (gameOverManager != null));
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Enter with: " + other.gameObject.name + " Tag: " + other.tag);

        if (other.CompareTag("Player"))
        {
            string currentScene = SceneManager.GetActiveScene().name;
            Debug.Log("Player entered trigger in scene: " + currentScene);

            if (currentScene == "ThirdLevel" || currentScene == "Level3" || currentScene == "Level_3")
            {
                Debug.Log("Attempting to trigger game over...");
                if (gameOverManager != null)
                {
                    Debug.Log("Calling TriggerGameOver()");
                    gameOverManager.TriggerGameOver();
                }
                else
                {
                    Debug.LogError("GameOverManager is null!");
                    // Try finding it again in case it was added later
                    gameOverManager = FindObjectOfType<GameOverManager>();
                    if (gameOverManager != null)
                    {
                        Debug.Log("Found GameOverManager on second attempt - triggering game over");
                        gameOverManager.TriggerGameOver();
                    }
                }
            }
            else
            {
                Debug.Log("Loading next scene: " + targetSceneName);
                StartCoroutine(LoadSceneWithDelay());
            }
        }
    }

    private System.Collections.IEnumerator LoadSceneWithDelay()
    {
        yield return new WaitForSeconds(transitionDelay);
        SceneManager.LoadScene(targetSceneName);
    }
}