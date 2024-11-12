using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button quitButton;

    private bool isGameOver = false;

    private void Awake()
    {
        // Hide the GameOver panel at the start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Setup button listeners
        if (retryButton != null)
            retryButton.onClick.AddListener(RetryGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Show the game over UI and pause the game
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    private void RetryGame()
    {
        // Unfreeze time and reload the current scene
        Debug.Log("Retry pressed");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
        SceneManager.LoadScene("FirstLevel");
    }

    private void QuitGame()
    {
        Debug.Log("Quit pressed");
        // Unfreeze time and load the main menu scene
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu"); // Replace "Menu" with your main menu scene name if different

        // For standalone builds
#if UNITY_STANDALONE
        Application.Quit();
#endif
    }
}
