using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    [Header("FlashLight Settings")]
    public GameObject spotlight;
    private Slider batterySlider;
    public float batteryDrainRate = 0.02f;
    private float currentBattery = 1f;
    public AudioClip clickSound;
    public AudioClip cellCollectSound;
    private AudioSource audioSource;
    private float batteryRechargeAmount = 0.35f; // Amount to recharge when collecting a cell

    // Reference to MainManager
    private MainManager mainManager;

    void Start()
    {
        InitializeComponents();
        LoadFlashlightState();
    }

    void InitializeComponents()
    {
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<CharacterController>();
        audioSource = gameObject.AddComponent<AudioSource>();
        mainManager = MainManager.Instance;

        batterySlider = GameObject.Find("Flashlight")?.GetComponent<Slider>();
        staminaSlider = GameObject.Find("Stamina")?.GetComponent<Slider>();

        currentSpeed = walkSpeed;
        currentStamina = 1f;

        ValidateComponents();
    }

    void ValidateComponents()
    {
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

    void LoadFlashlightState()
    {
        if (mainManager != null)
        {
            if (SceneManager.GetActiveScene().name == "FirstLevel")
            {
                currentBattery = 1f;
                mainManager.UpdateBatteryLevel(currentBattery);
                spotlight.SetActive(false);
            }
            else
            {
                currentBattery = mainManager.currentBatteryLevel;
                spotlight.SetActive(mainManager.isFlashlightOn);
            }

            if (batterySlider != null)
            {
                batterySlider.value = currentBattery;
            }
        }
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
        if (Input.GetKey(KeyCode.LeftShift) && currentStamina > 0)
        {
            currentStamina = Mathf.Max(0, currentStamina - staminaDrainRate * Time.deltaTime);
        }
        else if (Time.time - lastSprintTime > staminaRegenDelay)
        {
            currentStamina = Mathf.Min(1f, currentStamina + staminaRegenRate * Time.deltaTime);
        }

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
                bool newState = !spotlight.activeSelf;
                spotlight.SetActive(newState);

                if (mainManager != null)
                {
                    mainManager.SetFlashlightState(newState);
                }

                if (clickSound != null)
                {
                    audioSource.PlayOneShot(clickSound);
                }
            }
        }

        if (spotlight != null && spotlight.activeSelf)
        {
            currentBattery = Mathf.Max(0, currentBattery - batteryDrainRate * Time.deltaTime);

            if (mainManager != null)
            {
                mainManager.UpdateBatteryLevel(currentBattery);
            }

            if (batterySlider != null)
            {
                batterySlider.value = currentBattery;
            }

            if (currentBattery <= 0)
            {
                spotlight.SetActive(false);
                if (mainManager != null)
                {
                    mainManager.SetFlashlightState(false);
                }
            }
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Cell"))
        {
            if (cellCollectSound != null)
            {
                audioSource.volume = 0.5f;
                audioSource.PlayOneShot(cellCollectSound);
            }

            currentBattery = Mathf.Min(1f, currentBattery + batteryRechargeAmount);

            if (mainManager != null)
            {
                mainManager.UpdateBatteryLevel(currentBattery);
            }

            if (batterySlider != null)
            {
                batterySlider.value = currentBattery;
            }

            Destroy(hit.gameObject);
        }
    }

    void OnDisable()
    {
        if (mainManager != null)
        {
            mainManager.UpdateBatteryLevel(currentBattery);
            mainManager.SetFlashlightState(spotlight != null && spotlight.activeSelf);
        }
    }
}
