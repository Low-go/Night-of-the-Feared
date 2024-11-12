using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    public static MainManager Instance { get; private set; }

    [Header("Flashlight State")]
    public float currentBatteryLevel = 1f;
    public bool isFlashlightOn = false;

    [Header("Scene Management")]
    private string firstLevelName = "FirstLevel";

    // Optional: Add events for state changes
    public System.Action<float> OnBatteryLevelChanged;
    public System.Action<bool> OnFlashlightStateChanged;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Optional: Subscribe to scene loading events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == firstLevelName)
        {
            // Reset state for first level
            ResetToDefaultState();
        }
    }

    public void UpdateBatteryLevel(float newLevel)
    {
        currentBatteryLevel = Mathf.Clamp(newLevel, 0f, 1f);
        OnBatteryLevelChanged?.Invoke(currentBatteryLevel);
    }

    public void SetFlashlightState(bool state)
    {
        isFlashlightOn = state;
        OnFlashlightStateChanged?.Invoke(isFlashlightOn);
    }

    public bool IsFirstLevel()
    {
        return SceneManager.GetActiveScene().name == firstLevelName;
    }

    private void ResetToDefaultState()
    {
        currentBatteryLevel = 1f;
        isFlashlightOn = false;
        OnBatteryLevelChanged?.Invoke(currentBatteryLevel);
        OnFlashlightStateChanged?.Invoke(isFlashlightOn);
    }

    // Optional: Add save/load functionality for persistence between game sessions
    public void SaveState()
    {
        PlayerPrefs.SetFloat("BatteryLevel", currentBatteryLevel);
        PlayerPrefs.SetInt("FlashlightOn", isFlashlightOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void LoadState()
    {
        if (PlayerPrefs.HasKey("BatteryLevel"))
        {
            currentBatteryLevel = PlayerPrefs.GetFloat("BatteryLevel");
            isFlashlightOn = PlayerPrefs.GetInt("FlashlightOn") == 1;
        }
    }
}