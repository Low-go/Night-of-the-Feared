using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeIntensity = 0.2f;
    [SerializeField] private float decreaseSpeed = 1.0f;

    private Vector3 originalPosition;
    private float currentShakeDuration = 0f;
    private bool isShaking = false;

    private void Start()
    {
        originalPosition = transform.localPosition;
    }

    private void Update()
    {
        if (isShaking)
        {
            if (currentShakeDuration > 0)
            {
                transform.localPosition = originalPosition + Random.insideUnitSphere * shakeIntensity;
                currentShakeDuration -= Time.deltaTime * decreaseSpeed;
            }
            else
            {
                isShaking = false;
                currentShakeDuration = 0f;
                transform.localPosition = originalPosition;
            }
        }
    }

    public void StartShake()
    {
        currentShakeDuration = shakeDuration;
        isShaking = true;
    }

    // Optional: Method to stop shake immediately
    public void StopShake()
    {
        isShaking = false;
        currentShakeDuration = 0f;
        transform.localPosition = originalPosition;
    }
}