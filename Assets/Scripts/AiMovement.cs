using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyVisionCone))]
public class AIMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float searchSpeed = 3f;
    [SerializeField] private float rotationSpeed = 120f; // Degrees per second
    [SerializeField] private float waitTimeAtPatrolPoint = 2f;
    [SerializeField] private float searchDuration = 5f;
    [SerializeField] private float searchAreaRadius = 3f;

    private NavMeshAgent agent;
    private EnemyVisionCone visionCone;
    private List<Vector3> patrolPoints;
    private int currentPatrolIndex = 0;
    private bool isWaitingAtPatrol = false;
    private bool isSearching = false;
    private Vector3 lastKnownPlayerPos;
    private Coroutine searchCoroutine;

    private enum AIState
    {
        Patrolling,
        Chasing,
        Searching,
        WaitingAtPatrol
    }

    private AIState currentState;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        visionCone = GetComponent<EnemyVisionCone>();

        // Get patrol points from spawn manager
        MazeSpawnManager spawnManager = FindObjectOfType<MazeSpawnManager>();
        if (spawnManager != null)
        {
            // Find the patrol route for this specific enemy
            foreach (var route in spawnManager.PatrolRoutes)
            {
                if (Vector3.Distance(route.spawnPoint, transform.position) < 0.1f)
                {
                    patrolPoints = route.patrolPoints;
                    break;
                }
            }
        }

        // Set initial state and speed
        currentState = AIState.Patrolling;
        agent.speed = patrolSpeed;
        MoveToNextPatrolPoint();
    }

    private void Update()
    {
        switch (currentState)
        {
            case AIState.Patrolling:
                HandlePatrolState();
                break;
            case AIState.Chasing:
                HandleChaseState();
                break;
            case AIState.Searching:
                HandleSearchState();
                break;
            case AIState.WaitingAtPatrol:
                HandleWaitState();
                break;
        }
    }

    private void HandlePatrolState()
    {
        if (visionCone.TargetInSight)
        {
            TransitionToChasing();
            return;
        }

        if (!isWaitingAtPatrol && HasReachedDestination())
        {
            StartCoroutine(WaitAtPatrolPoint());
        }
    }

    private void HandleChaseState()
    {
        if (visionCone.TargetInSight)
        {
            lastKnownPlayerPos = visionCone.LastKnownTargetPosition;
            agent.SetDestination(lastKnownPlayerPos);
        }
        else
        {
            TransitionToSearching();
        }
    }

    private void HandleSearchState()
    {
        if (visionCone.TargetInSight)
        {
            TransitionToChasing();
        }
    }

    private void HandleWaitState()
    {
        if (visionCone.TargetInSight)
        {
            isWaitingAtPatrol = false;
            TransitionToChasing();
        }
    }

    private void TransitionToChasing()
    {
        if (currentState != AIState.Chasing)
        {
            currentState = AIState.Chasing;
            agent.speed = chaseSpeed;
            if (searchCoroutine != null)
            {
                StopCoroutine(searchCoroutine);
            }
        }
    }

    private void TransitionToSearching()
    {
        currentState = AIState.Searching;
        agent.speed = searchSpeed;
        searchCoroutine = StartCoroutine(SearchForPlayer());
    }

    private IEnumerator SearchForPlayer()
    {
        float searchTime = 0f;
        while (searchTime < searchDuration)
        {
            // Generate random points around last known position
            Vector3 searchPoint = lastKnownPlayerPos + Random.insideUnitSphere * searchAreaRadius;
            searchPoint.y = transform.position.y; // Keep on the same Y level

            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(transform.position, searchPoint, NavMesh.AllAreas, path))
            {
                agent.SetPath(path);
            }

            // Wait until reaching the search point or timeout
            float searchPointTimeout = 0f;
            while (!HasReachedDestination() && searchPointTimeout < 2f)
            {
                searchPointTimeout += Time.deltaTime;
                yield return null;
            }

            // Look around at search point
            float rotationTime = 0f;
            while (rotationTime < 1f)
            {
                transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
                rotationTime += Time.deltaTime;
                yield return null;
            }

            searchTime += searchPointTimeout + rotationTime;
            yield return null;
        }

        // Return to patrol after searching
        currentState = AIState.Patrolling;
        agent.speed = patrolSpeed;
        MoveToNextPatrolPoint();
    }

    private IEnumerator WaitAtPatrolPoint()
    {
        isWaitingAtPatrol = true;
        currentState = AIState.WaitingAtPatrol;

        // Look around while waiting
        float waitedTime = 0f;
        while (waitedTime < waitTimeAtPatrolPoint)
        {
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
            waitedTime += Time.deltaTime;
            yield return null;
        }

        isWaitingAtPatrol = false;
        currentState = AIState.Patrolling;
        MoveToNextPatrolPoint();
    }

    private void MoveToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Count == 0) return;

        agent.SetDestination(patrolPoints[currentPatrolIndex]);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
    }

    private bool HasReachedDestination()
    {
        return !agent.pathPending &&
               agent.remainingDistance <= agent.stoppingDistance &&
               (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Handle player catch event
            Debug.Log("Caught the player!");
            // Add your game over or player caught logic here
        }
    }
}