using UnityEngine;
using System.Collections.Generic;

public class FlashlightDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float coneAngle = 30f;
    [SerializeField] private int rayCount = 8;
    [SerializeField] private LayerMask detectionMask = -1;

    [Header("Debug")]
    [SerializeField] private bool showDebugRays = true;

    private Light spotLight;
    private HashSet<AIMovement> litMonsters = new HashSet<AIMovement>();
    private Dictionary<AIMovement, bool> previousLightStates = new Dictionary<AIMovement, bool>();
    private const float MIN_DOT_PRODUCT = 0.5f;

    private void Start()
    {
        spotLight = transform.Find("Spot Light")?.GetComponent<Light>();
        if (spotLight)
        {
            detectionRange = spotLight.range;
            coneAngle = spotLight.spotAngle;
        }
        else
        {
            Debug.LogWarning("Spot Light child not found on Flashlight object!");
        }
    }

    private void Update()
    {
        if (spotLight == null || !spotLight.enabled || !spotLight.gameObject.activeSelf)
        {
            ResetMonsterLighting();
            return;
        }

        ClearPreviousLighting();
        CastDetectionRays();

        List<AIMovement> monstersToUpdate = new List<AIMovement>();

        foreach (var monster in previousLightStates.Keys)
        {
            bool currentState = monster.IsInLight;
            bool previousState = previousLightStates[monster];

            if (currentState != previousState)
            {
                Debug.Log($"[FlashlightDetector] {monster.gameObject.name} IsInLight changed to {currentState}");
                monstersToUpdate.Add(monster);
            }
        }

        foreach (var monster in monstersToUpdate)
        {
            previousLightStates[monster] = monster.IsInLight;
        }
    }

    private void CastDetectionRays()
    {
        CastRay(transform.forward);

        for (int i = 0; i < rayCount; i++)
        {
            float angle = ((float)i / rayCount) * coneAngle;

            Quaternion rotation = Quaternion.AngleAxis(angle, transform.up);
            Vector3 direction = rotation * transform.forward;
            CastRay(direction);

            rotation = Quaternion.AngleAxis(-angle, transform.up);
            direction = rotation * transform.forward;
            CastRay(direction);

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
                        previousLightStates[monster] = false;
                    }
                }
            }
        }

        if (showDebugRays)
        {
            Debug.DrawRay(transform.position, direction * detectionRange, Color.yellow);
        }
    }

    private void ClearPreviousLighting()
    {
        foreach (var monster in litMonsters)
        {
            if (monster != null)
            {
                monster.IsInLight = false;
            }
        }
        litMonsters.Clear();
    }

    private void ResetMonsterLighting()
    {
        foreach (var monster in litMonsters)
        {
            if (monster != null)
            {
                monster.IsInLight = false;
                previousLightStates[monster] = false;
            }
        }
        litMonsters.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugRays) return;

        Gizmos.color = Color.yellow;
        float halfAngle = coneAngle * 0.5f;
        Vector3 forward = transform.forward * detectionRange;

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
