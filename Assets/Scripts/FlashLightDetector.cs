using UnityEngine;
using System.Collections.Generic;

public class FlashlightDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float coneAngle = 30f;
    [SerializeField] private int rayCount = 8;
    [SerializeField] private LayerMask detectionMask = -1; // Will detect on all layers by default

    [Header("Debug")]
    [SerializeField] private bool showDebugRays = true;

    private Light spotLight;
    private HashSet<AIMovement> litMonsters = new HashSet<AIMovement>();
    private Dictionary<AIMovement, bool> previousLightStates = new Dictionary<AIMovement, bool>(); //not sure if its working
    private const float MIN_DOT_PRODUCT = 0.5f; // Cosine of maximum angle to be considered "in light"

    private void Start()
    {
        spotLight = GetComponent<Light>();
        if (spotLight)
        {
            // Match the spotlight's range and angle if available
            detectionRange = spotLight.range;
            coneAngle = spotLight.spotAngle;
        }
    }

    private void Update()
    {
        // Clear previous state
        foreach (var monster in litMonsters)
        {
            if (monster != null)
            {
                monster.IsInLight = false;
            }
        }
        litMonsters.Clear();

        // Cast detection rays
        CastDetectionRays();

        // Collect changes to update later
        List<AIMovement> monstersToUpdate = new List<AIMovement>();

        foreach (var monster in previousLightStates.Keys)
        {
            bool currentState = monster.IsInLight;
            bool previousState = previousLightStates[monster];

            if (currentState != previousState)
            {
                Debug.Log($"[FlashlightDetector] {monster.gameObject.name} IsInLight changed to {currentState}");
                monstersToUpdate.Add(monster); // Track monster state changes
            }
        }

        // Now update the light state after the loop
        foreach (var monster in monstersToUpdate)
        {
            previousLightStates[monster] = monster.IsInLight;
        }
    }

    private void CastDetectionRays()
    {
        // Cast central ray
        CastRay(transform.forward);

        // Cast surrounding rays
        for (int i = 0; i < rayCount; i++)
        {
            float angle = ((float)i / rayCount) * coneAngle;

            // Create rotations around the forward axis
            Quaternion rotation = Quaternion.AngleAxis(angle, transform.up);
            Vector3 direction = rotation * transform.forward;
            CastRay(direction);

            // Cast rays in a cone pattern
            rotation = Quaternion.AngleAxis(-angle, transform.up);
            direction = rotation * transform.forward;
            CastRay(direction);

            // Add some rays rotated around the right axis for better coverage
            rotation = Quaternion.AngleAxis(angle, transform.right);
            direction = rotation * transform.forward;
            CastRay(direction);

            rotation = Quaternion.AngleAxis(-angle, transform.right);
            direction = rotation * transform.forward;
            CastRay(direction);
        }
    }

    private void CastRay(Vector3 direction)
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, detectionRange, detectionMask);

        foreach (RaycastHit hit in hits)
        {
            AIMovement monster = hit.collider.GetComponent<AIMovement>();
            if (monster != null)
            {
                Vector3 directionToMonster = (hit.point - transform.position).normalized;
                float dotProduct = Vector3.Dot(transform.forward, directionToMonster);

                if (dotProduct > MIN_DOT_PRODUCT &&
                    !Physics.Raycast(transform.position, directionToMonster, hit.distance - 0.1f, detectionMask))
                {
                    monster.IsInLight = true;
                    litMonsters.Add(monster);

                    if (!previousLightStates.ContainsKey(monster))
                    {
                        previousLightStates[monster] = false; // Initialize if not tracked
                    }
                }
            }
        }

        if (showDebugRays)
        {
            Debug.DrawRay(transform.position, direction * detectionRange, Color.yellow);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugRays) return;

        Gizmos.color = Color.yellow;
        float halfAngle = coneAngle * 0.5f;
        Vector3 forward = transform.forward * detectionRange;

        // Draw cone visualization
        Vector3 right = Quaternion.Euler(0, halfAngle, 0) * forward;
        Vector3 left = Quaternion.Euler(0, -halfAngle, 0) * forward;
        Vector3 up = Quaternion.Euler(-halfAngle, 0, 0) * forward;
        Vector3 down = Quaternion.Euler(halfAngle, 0, 0) * forward;

        Gizmos.DrawRay(transform.position, forward);
        Gizmos.DrawRay(transform.position, right);
        Gizmos.DrawRay(transform.position, left);
        Gizmos.DrawRay(transform.position, up);
        Gizmos.DrawRay(transform.position, down);
    }
}
