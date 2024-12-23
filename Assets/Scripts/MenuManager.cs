using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class M : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource menuMusic;
    private UIDocument document;
    private Button playButton;
    private Button settingsButton;
    private Button quitButton;

    // Start is called before the first frame update
    void Start()
    {
        document = GetComponent<UIDocument>();
        playButton = document.rootVisualElement.Q<Button>("play-button");
        settingsButton = document.rootVisualElement.Q<Button>("settings-button");
        quitButton = document.rootVisualElement.Q<Button>("quit-button");

        // Add click event handlers
        playButton.clicked += PlayGame;
        settingsButton.clicked += OpenSettings;
        quitButton.clicked += OpenQuit;

        if (menuMusic != null)
        {
            menuMusic.Play();
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("FirstLevel"); //hopefully the string calling works
    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    private void OpenSettings()
    {
        // We'll implement this later
        Debug.Log("Settings clicked");
    }

    private void OpenQuit()
    {
        Debug.Log("clicked");

        // Check if running in the Unity editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stops play mode in the editor
        #else
        Application.Quit(); // Quit the application in a built game
        #endif
    }
}
