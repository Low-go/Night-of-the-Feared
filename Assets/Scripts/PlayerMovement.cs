using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    private CharacterController controller;
    public Camera playerCamera;
    private float walkSpeed = 0.8f;
    private float sprintSpeed = 1.3f; // Speed while sprinting
    private float currentSpeed;
    public float mouseSensitivity;
    private float xRotation = 0f;

    [Header("Stamina Settings")]
    private float staminaDrainRate = 0.22f; // How fast stamina drains while sprinting
    private float staminaRegenRate = 0.2f; // How fast stamina regenerates
    private float staminaRegenDelay = 3f; 
    private float currentStamina = 1f;
    private float lastSprintTime;
    public Slider staminaSlider;

    [Header("FlashLight Stuff")]
    public GameObject spotlight;
    private Slider batterySlider;
    public float batteryDrainRate = 0.02f;
    private float currentBattery = 1f;
    public AudioClip clickSound;
    private AudioSource audioSource;

    void Start()
    {
        // this is so the cursor does not appear when the player is in first person
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<CharacterController>();
        audioSource = gameObject.AddComponent<AudioSource>();
        batterySlider = GameObject.Find("Flashlight")?.GetComponent<Slider>();
        staminaSlider = GameObject.Find("Stamina")?.GetComponent<Slider>();
        currentBattery = 1f;
        currentSpeed = walkSpeed;
        currentStamina = 1f;

        if (batterySlider != null)
        {
            Debug.Log("Battery Slider successfully found and assigned!");
            batterySlider.value = currentBattery;
        }
        else
        {
            Debug.LogWarning("Battery Slider not found! Check the slider's name or ensure it is in the scene.");
        }

        if (staminaSlider != null)
        {
            Debug.Log("Stamina Slider successfully found and assigned!");
            staminaSlider.value = currentStamina;
        }
        else
        {
            Debug.LogWarning("Stamina Slider not found! Check the slider's name or ensure it is in the scene.");
        }

        spotlight.SetActive(false);
    }

    void Update()
    {
        HandleMovement();
        HandleCameraRotation();
        HandleFlashlight();
        HandleStamina();
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Handle sprinting
        if (Input.GetKey(KeyCode.LeftShift) && currentStamina > 0)
        {
            currentSpeed = sprintSpeed;
            lastSprintTime = Time.time;
        }
        else
        {
            currentSpeed = walkSpeed;
        }

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    void HandleStamina()
    {
        // Drain stamina while sprinting
        if (Input.GetKey(KeyCode.LeftShift) && currentStamina > 0)
        {
            currentStamina = Mathf.Max(0, currentStamina - staminaDrainRate * Time.deltaTime);
        }
        // Regenerate stamina after delay
        else if (Time.time - lastSprintTime > staminaRegenDelay)
        {
            currentStamina = Mathf.Min(1f, currentStamina + staminaRegenRate * Time.deltaTime);
        }

        // Update stamina slider
        if (staminaSlider != null)
        {
            staminaSlider.value = currentStamina;
        }
    }

    void HandleCameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleFlashlight()
    {
        if (Input.GetMouseButtonDown(0) && currentBattery > 0)
        {
            if (spotlight != null)
            {
                spotlight.SetActive(!spotlight.activeSelf);
                if (clickSound != null)
                {
                    audioSource.PlayOneShot(clickSound);
                }
            }
        }

        if (spotlight != null && spotlight.activeSelf)
        {
            currentBattery = Mathf.Max(0, currentBattery - batteryDrainRate * Time.deltaTime);
            batterySlider.value = currentBattery;
            if (currentBattery <= 0)
            {
                spotlight.SetActive(false);
            }
        }
    }
}