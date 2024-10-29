using UnityEngine;

public class CameraCollisionCheck : MonoBehaviour
{
    public Transform playerHead; // Reference to the player's head transform
    public float maxCameraDistance = 0.5f; // Max distance from the head
    public LayerMask collisionMask; // Layer mask for objects that block the camera

    void Update()
    {
        // Desired position for the camera
        Vector3 desiredCameraPosition = playerHead.position - playerHead.forward * maxCameraDistance;

        // Check for any obstacles between the head and the desired camera position
        RaycastHit hit;
        if (Physics.Raycast(playerHead.position, -playerHead.forward, out hit, maxCameraDistance, collisionMask))
        {
            // Move camera to the collision point to avoid clipping
            transform.position = hit.point;
        }
        else
        {
            // No collision; set camera to the desired position
            transform.position = desiredCameraPosition;
        }
    }
}
