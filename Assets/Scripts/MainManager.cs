using UnityEngine;

public class MainManager : MonoBehaviour
{
    public static MainManager Instance { get; private set; }

    public float currentBatteryLevel = 1f;
    public bool isFlashlightOn = false;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void UpdateBatteryLevel(float newLevel)
    {
        currentBatteryLevel = Mathf.Clamp(newLevel, 0f, 1f);
    }

    public void SetFlashlightState(bool state)
    {
        isFlashlightOn = state;
    }
}