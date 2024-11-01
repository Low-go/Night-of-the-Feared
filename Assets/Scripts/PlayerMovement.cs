using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Start is called before the first frame update

    private CharacterController controller;
    public Camera playerCamera;
    public float speed = 5f;
    public float gravity = -9.81f;
    public float mouseSensitivity;
    private Vector3 velocity;
    private float xRotation = 0f;
    public GameObject spotlight;

    void Start()
    {
        // locks the curso so it cant be seen as is custom in fps games
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal"); // get both axis's
        float vertical = Input.GetAxis("Vertical");
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        controller.Move(move * speed * Time.deltaTime);

        // gravity? hopefully. Maybe delete for now if this dosent work

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }
        controller.Move(velocity * Time.deltaTime);


        // Mouse Input for Camera Rotation
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Prevent flipping

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        if (Input.GetMouseButtonDown(0))
        {
            if (spotlight != null)
            {
                spotlight.SetActive(!spotlight.activeSelf);
            }
        }
    }
}
