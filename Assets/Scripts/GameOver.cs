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
    [SerializeField] private Image overlayImage;

    [Header("Settings")]
    [SerializeField] private float overlayAlpha = 0.7f;
    [SerializeField] private Color overlayColor = Color.gray;
    [SerializeField] private Animator overlayAnimator;

    private bool isGameOver = false;

    private void Awake()
    {
        // Ensure the panel is hidden at start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Setup button listeners
        if (retryButton != null)
            retryButton.onClick.AddListener(RetryGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        // Configure overlay
        if (overlayImage != null)
        {
            Color newColor = overlayColor;
            newColor.a = overlayAlpha;
            overlayImage.color = newColor;
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        // Show the game over UI
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            // Trigger the animation
            if (overlayAnimator != null)
                overlayAnimator.SetTrigger("GameOverOpen");  // Your animation trigger name
        }

        Time.timeScale = 0f;
    }
    private void RetryGame()
    {
        // Unfreeze time
        Time.timeScale = 1f;

        // Reload current scene
        SceneManager.LoadScene("FirstLevel");
    }

    private void QuitGame()
    {
        // Unfreeze time before quitting
        Time.timeScale = 1f;

        // Load your main menu scene
        SceneManager.LoadScene("Menu"); // Replace with your menu scene name

        // Or quit the application if this is a built game
        #if UNITY_STANDALONE
        Application.Quit();
        #endif
    }
}