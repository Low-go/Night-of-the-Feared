using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    private CharacterController controller;
    public Camera playerCamera;
    public float speed = 5f;
    public float mouseSensitivity;
    private float xRotation = 0f;


    [Header("FlashLight Stuff")]
    public GameObject spotlight;
    public Slider batterySlider;
    public float batteryDrainRate = 0.1f; // how fast battery drains
    private float currentBattery = 1f;
    public AudioClip clickSound;
    private AudioSource audioSource;

    void Start()
    {
        // locks the cursor so it cant be seen as is custom in fps games
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<CharacterController>();
        audioSource = gameObject.AddComponent<AudioSource>();

        currentBattery = 1f;
        batterySlider.value = currentBattery;
    }

    void Update()
    {
        HandleMovement();
        HandleCameraRotation();
        HandleFlashlight();
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        controller.Move(move * speed * Time.deltaTime);
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
        // Only allow toggling if we have battery
        if (Input.GetMouseButtonDown(0) && currentBattery > 0)
        {
            if (spotlight != null)
            {
                spotlight.SetActive(!spotlight.activeSelf);

                // Play click sound
                if (clickSound != null)
                {
                    audioSource.PlayOneShot(clickSound);
                }
            }
        }

        // Drain battery while flashlight is on
        if (spotlight != null && spotlight.activeSelf)
        {
            currentBattery = Mathf.Max(0, currentBattery - batteryDrainRate * Time.deltaTime);
            batterySlider.value = currentBattery;

            // Turn off flashlight if battery dies
            if (currentBattery <= 0)
            {
                spotlight.SetActive(false);
            }
        }
    }
}