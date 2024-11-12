using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private float invincibilityDuration = 2f;

    [Header("Effect References")]
    [SerializeField] private AudioSource hitSoundEffect;
    //[SerializeField] private GameObject screenRedOverlay;
    [SerializeField] private CameraShake cameraShake;
    
    private GameOverManager gameOverManager;
    private int currentHealth;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;

    private void Start()
    {
        currentHealth = maxHealth;


    }

    private void Update()
    {
        HandleInvincibility();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            TakeDamage();
        }
    }

    private void TakeDamage()
    {
        if (isInvincible) return;

        currentHealth--;
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        ApplyHitEffects();

        // Trigger Game Over only if health is zero
        if (currentHealth <= 0)
        {
            GameObject gameOverCanvas = GameObject.Find("GameOverCanvas");
            if (gameOverCanvas != null)
            {
                gameOverManager = gameOverCanvas.GetComponent<GameOverManager>();
                if (gameOverManager != null)
                {
                    Debug.Log("Found GameOverManager, triggering game over...");
                    gameOverManager.TriggerGameOver();
                }
                else
                {
                    Debug.LogError("GameOverManager component not found on GameOverCanvas!");
                }
            }
            else
            {
                Debug.LogError("GameOverCanvas not found in the scene!");
            }
        }
    }

    private void HandleInvincibility()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
                //if (screenRedOverlay != null)
                //    screenRedOverlay.SetActive(false);
            }
        }
    }

    private void ApplyHitEffects()
    {
        // Play hit sound
        if (hitSoundEffect != null)
        {
            hitSoundEffect.Play();
        }

        // Show red overlay
        //if (screenRedOverlay != null)
        //{
        //    screenRedOverlay.SetActive(true);
        //}

        // Shake camera
        if (cameraShake != null)
        {
            cameraShake.StartShake();
        }
    }
}
