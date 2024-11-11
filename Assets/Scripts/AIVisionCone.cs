using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyVisionCone : MonoBehaviour
{
    [Header("Vision Settings")]
    [SerializeField] private float viewRadius = 12f;
    [SerializeField] private float viewAngle = 140f;
    [SerializeField] private float peripheralViewDistance = 3f;
    [SerializeField] private float instantDetectionDistance = 2f;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Wall Avoidance")]
    [SerializeField] private float wallAvoidanceDistance = 2f;
    [SerializeField] private float sideCheckAngle = 45f;

    [Header("Jitter Settings")]
    [SerializeField] private float jitterCheckInterval = 0.5f;
    [SerializeField] private float stuckThreshold = 0.1f;
    [SerializeField] private float jitterRotationStrength = 45f;
    [SerializeField] private float jitterMoveStrength = 2f;

    [Header("Debug Visualization")]
    [SerializeField] private bool showDebugVisuals = true;
    [SerializeField] private Color debugColorNoTarget = Color.yellow;
    [SerializeField] private Color debugColorTargetSpotted = Color.red;

    private Transform currentTarget;
    private Vector3 lastKnownTargetPosition;
    private bool targetInSight;
    private Vector3 lastPosition;
    private float lastJitterTime;
    private NavMeshAgent agent;

    public bool TargetInSight => targetInSight;
    public Vector3 LastKnownTargetPosition => lastKnownTargetPosition;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        lastPosition = transform.position;
        lastJitterTime = Time.time;
        StartCoroutine(CheckIfStuck());
    }

    private void Update()
    {
        FindVisibleTargets();
    }

    private IEnumerator CheckIfStuck()
    {
        while (true)
        {
            yield return new WaitForSeconds(jitterCheckInterval);

            float distanceMoved = Vector3.Distance(transform.position, lastPosition);
            if (distanceMoved < stuckThreshold && Time.time - lastJitterTime > jitterCheckInterval)
            {
                ApplyJitter();
                lastJitterTime = Time.time;
            }

            lastPosition = transform.position;
        }
    }

    private void ApplyJitter()
    {
        if (agent == null) return;

        float randomRotation = Random.Range(-jitterRotationStrength, jitterRotationStrength);
        transform.Rotate(0, randomRotation, 0);

        Vector3 randomDirection = Random.insideUnitSphere;
        randomDirection.y = 0;
        randomDirection.Normalize();

        Vector3 jitterPosition = transform.position + randomDirection * jitterMoveStrength;
        if (NavMesh.SamplePosition(jitterPosition, out NavMeshHit hit, jitterMoveStrength, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        float originalSpeed = agent.speed;
        agent.speed *= 1.5f;
        StartCoroutine(ResetSpeed(originalSpeed));
    }

    private IEnumerator ResetSpeed(float originalSpeed)
    {
        yield return new WaitForSeconds(0.5f);
        if (agent != null)
        {
            agent.speed = originalSpeed;
        }
    }

    private void FindVisibleTargets()
    {
        targetInSight = false;
        currentTarget = null;

        // Instant detection for close-range
        Collider[] nearTargets = Physics.OverlapSphere(transform.position, instantDetectionDistance, targetMask);
        if (nearTargets.Length > 0)
        {
            Transform target = nearTargets[0].transform;
            if (!Physics.Raycast(transform.position, (target.position - transform.position).normalized, instantDetectionDistance, obstacleMask))
            {
                targetInSight = true;
                currentTarget = target;
                lastKnownTargetPosition = target.position;
                return;
            }
        }

        // Normal vision cone with peripheral vision
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        foreach (var targetCollider in targetsInViewRadius)
        {
            Transform target = targetCollider.transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            float dstToTarget = Vector3.Distance(transform.position, target.position);

            bool inPeripheralVision = dstToTarget <= peripheralViewDistance;
            float checkAngle = inPeripheralVision ? viewAngle * 1.5f : viewAngle;

            if (Vector3.Angle(transform.forward, dirToTarget) < checkAngle / 2)
            {
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    targetInSight = true;
                    currentTarget = target;
                    lastKnownTargetPosition = target.position;
                    return;
                }
            }
        }
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

                Vector3 leftDir = Quaternion.Euler(0, -sideCheckAngle, 0) * transform.forward;
                Vector3 rightDir = Quaternion.Euler(0, sideCheckAngle, 0) * transform.forward;

                betterDirection = leftClear ? leftDir : (rightClear ? rightDir : transform.forward);
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
