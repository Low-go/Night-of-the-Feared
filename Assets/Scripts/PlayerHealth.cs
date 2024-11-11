using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private float invincibilityDuration = 2f;

    [Header("Effect References")]
    [SerializeField] private AudioSource hitSoundEffect;
    //[SerializeField] private GameObject screenRedOverlay;
    [SerializeField] private CameraShake cameraShake;  // You'll need to create this script

    private int currentHealth;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;

    private void Start()
    {
        currentHealth = maxHealth;
        //if (screenRedOverlay != null)
        //    screenRedOverlay.SetActive(false);
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

        if (currentHealth <= 0)
        {
            Debug.Log("Game Over!");
            // Add game over logic here later
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
            cameraShake.StartShake();  // You'll need to implement this
        }
    }
}