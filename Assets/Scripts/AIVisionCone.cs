using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

    [Header("Wall Avoidance")]
    [SerializeField] private float wallAvoidanceDistance = 2f;
    [SerializeField] private float sideCheckAngle = 45f;

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

        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    targetInSight = true;
                    currentTarget = target;
                    lastKnownTargetPosition = target.position;
                }
            }
        }
    }

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

    public bool IsWallAhead(out Vector3 betterDirection)
    {
        betterDirection = Vector3.zero;
        Vector3 currentPosition = transform.position;

        if (Physics.Raycast(currentPosition, transform.forward, out RaycastHit hit, wallAvoidanceDistance))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                bool leftClear = !Physics.Raycast(currentPosition, Quaternion.Euler(0, -sideCheckAngle, 0) * transform.forward, wallAvoidanceDistance);
                bool rightClear = !Physics.Raycast(currentPosition, Quaternion.Euler(0, sideCheckAngle, 0) * transform.forward, wallAvoidanceDistance);

                NavMeshAgent agent = GetComponent<NavMeshAgent>();
                Vector3 targetPosition = agent.destination;

                Vector3 leftDir = Quaternion.Euler(0, -sideCheckAngle, 0) * transform.forward;
                Vector3 rightDir = Quaternion.Euler(0, sideCheckAngle, 0) * transform.forward;

                float leftAngleToTarget = Vector3.Angle(leftDir, targetPosition - currentPosition);
                float rightAngleToTarget = Vector3.Angle(rightDir, targetPosition - currentPosition);

                if (leftClear && rightClear)
                {
                    betterDirection = leftAngleToTarget < rightAngleToTarget ? leftDir : rightDir;
                }
                else if (leftClear)
                {
                    betterDirection = leftDir;
                }
                else if (rightClear)
                {
                    betterDirection = rightDir;
                }
                else
                {
                    betterDirection = leftAngleToTarget < rightAngleToTarget ? Quaternion.Euler(0, -sideCheckAngle * 2, 0) * transform.forward : Quaternion.Euler(0, sideCheckAngle * 2, 0) * transform.forward;
                }
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        if (!showDebugVisuals) return;

        Gizmos.color = targetInSight ? debugColorTargetSpotted : debugColorNoTarget;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);

        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);

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
