using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyVisionCone : MonoBehaviour
{
    [Header("Vision Settings")]
    [SerializeField] private float viewRadius = 5f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private LayerMask targetMask; // Layer for the player
    [SerializeField] private LayerMask obstacleMask; // Layer for walls/obstacles
    [SerializeField] private float meshResolution = 1f;
    [SerializeField] private int edgeResolveIterations = 4;
    [SerializeField] private float edgeDstThreshold = 0.5f;

    [Header("Debug Visualization")]
    [SerializeField] private bool showDebugVisuals = true;
    [SerializeField] private Color debugColorNoTarget = Color.yellow;
    [SerializeField] private Color debugColorTargetSpotted = Color.red;

    private Transform currentTarget;
    private Vector3 lastKnownTargetPosition;
    private bool targetInSight;

    public bool TargetInSight => targetInSight;
    public Vector3 LastKnownTargetPosition => lastKnownTargetPosition;

    private void Update()
    {
        FindVisibleTargets();
    }

    private void FindVisibleTargets()
    {
        targetInSight = false;
        currentTarget = null;

        // Check for targets within radius
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            // Check if target is within view angle
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);

                // Check for obstacles between enemy and target
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    targetInSight = true;
                    currentTarget = target;
                    lastKnownTargetPosition = target.position;
                }
            }
        }
    }

    // Method to check if a specific position is within the vision cone
    public bool IsPositionInSight(Vector3 position)
    {
        if (Vector3.Distance(transform.position, position) > viewRadius)
            return false;

        Vector3 dirToPosition = (position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, dirToPosition) > viewAngle / 2)
            return false;

        if (Physics.Raycast(transform.position, dirToPosition, out RaycastHit hit, viewRadius, obstacleMask))
        {
            if (hit.distance < Vector3.Distance(transform.position, position))
                return false;
        }

        return true;
    }

    private void OnDrawGizmos()
    {
        if (!showDebugVisuals) return;

        // Draw view radius
        Gizmos.color = targetInSight ? debugColorTargetSpotted : debugColorNoTarget;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        // Draw view angle
        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);

        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);

        // Draw line to last known position if target was spotted
        if (targetInSight && currentTarget != null)
        {
            Gizmos.DrawLine(transform.position, lastKnownTargetPosition);
        }
    }

    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}