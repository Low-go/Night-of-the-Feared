using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyVisionCone))]
public class AIMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float searchSpeed = 3f;
    [SerializeField] private float rotationSpeed = 120f;
    [SerializeField] private float waitTimeAtPatrolPoint = 2f;
    [SerializeField] private float searchDuration = 5f;
    [SerializeField] private float searchAreaRadius = 3f;

    [Header("Light Sensitivity")]
    private bool isInLight = false;

    [Header("Audio Settings")]
    public AudioSource walkingAudioSource;
    public AudioSource growlAudioSource;
    public AudioSource screamAudioSource;
    public bool canScream = false; // Set true for first fearful enemy types
    [Range(0f, 1f)]
    public float walkingVolume = 0.5f;
    [Range(0f, 1f)]
    public float idleVolume = 0.1f;
    [Range(0f, 1f)]
    public float growlVolume = 0.7f;
    [Range(0f, 1f)]
    public float screamVolume = 1f;

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

    public bool IsInLight
    {
        get => isInLight;
        set
        {
            isInLight = value;
        }
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
            foreach (var route in spawnManager.PatrolRoutes)
            {
                if (Vector3.Distance(route.spawnPoint, transform.position) < 0.1f)
                {
                    patrolPoints = route.patrolPoints;
                    break;
                }
            }
        }

        currentState = AIState.Patrolling;
        agent.speed = patrolSpeed;
        MoveToNextPatrolPoint();

        if (walkingAudioSource != null)
        {
            walkingAudioSource.loop = true;
            walkingAudioSource.Play();
        }

        StartCoroutine(PlayRandomGrowls());
    }

    private void Update()
    {
        UpdateAudio();
        UpdateMovement(); // Wall avoidance check

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

    private IEnumerator PlayRandomGrowls()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(60f, 80f));

            // Add probability check - 1/3 chance to growl
            if (Random.value <= 0.333f && currentState != AIState.Chasing)  // Don't play random growls during chase
            {
                if (growlAudioSource != null && !growlAudioSource.isPlaying)
                {
                    growlAudioSource.time = 0f;
                    growlAudioSource.volume = growlVolume;
                    growlAudioSource.Play();
                    StartCoroutine(StopGrowlAfterTime(6f));
                }
            }
        }
    }

    private IEnumerator StopGrowlAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (growlAudioSource != null && growlAudioSource.isPlaying)
        {
            growlAudioSource.Stop();
        }
    }

    private void UpdateAudio()
    {
        // Update walking sound volume based on movement
        if (walkingAudioSource != null)
        {
            walkingAudioSource.volume = agent.velocity.magnitude > 0.1f ? walkingVolume : idleVolume;
        }

        // Handle screaming during chase instead of light detection
        if (canScream && screamAudioSource != null)
        {
            if (currentState == AIState.Chasing && !screamAudioSource.isPlaying)
            {
                screamAudioSource.loop = true;
                screamAudioSource.volume = screamVolume;
                screamAudioSource.Play();
            }
            else if (currentState != AIState.Chasing && screamAudioSource.isPlaying)
            {
                screamAudioSource.Stop();
            }
        }
    }

    private void UpdateMovement()
    {
        if (isInLight)
        {
            agent.isStopped = true;
            return;
        }

        // Resume movement
        agent.isStopped = false;
        agent.speed = GetCurrentStateSpeed(); // Explicitly reset the speed

        // If we were interrupted by light, make sure we're still going to our destination
        if (!agent.hasPath || agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            switch (currentState)
            {
                case AIState.Patrolling:
                    MoveToNextPatrolPoint();
                    break;
                case AIState.Chasing:
                    agent.SetDestination(lastKnownPlayerPos);
                    break;
                case AIState.Searching:
                    // The search coroutine will handle this
                    break;
            }
        }

        if (visionCone.IsWallAhead(out Vector3 betterDirection))
        {
            Vector3 newTargetPosition = transform.position + betterDirection * 3f;
            NavMeshPath path = new NavMeshPath();

            if (NavMesh.CalculatePath(transform.position, newTargetPosition, NavMesh.AllAreas, path) &&
                path.status == NavMeshPathStatus.PathComplete)
            {
                agent.SetPath(path);
                agent.speed *= 0.8f;
            }
        }
        else
        {
            agent.speed = GetCurrentStateSpeed();
        }
    }

    private float GetCurrentStateSpeed()
    {
        switch (currentState)
        {
            case AIState.Chasing:
                return chaseSpeed;
            case AIState.Searching:
                return searchSpeed;
            default:
                return patrolSpeed;
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
            Vector3 searchPoint = lastKnownPlayerPos + Random.insideUnitSphere * searchAreaRadius;
            searchPoint.y = transform.position.y;

            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(transform.position, searchPoint, NavMesh.AllAreas, path))
            {
                agent.SetPath(path);
            }

            float searchPointTimeout = 0f;
            while (!HasReachedDestination() && searchPointTimeout < 2f)
            {
                searchPointTimeout += Time.deltaTime;
                yield return null;
            }

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

        currentState = AIState.Patrolling;
        agent.speed = patrolSpeed;
        MoveToNextPatrolPoint();
    }

    private IEnumerator WaitAtPatrolPoint()
    {
        isWaitingAtPatrol = true;
        currentState = AIState.WaitingAtPatrol;

        float waitedTime = 0f;
        while (waitedTime < waitTimeAtPatrolPoint)
        {
            waitedTime += Time.deltaTime;
            yield return null;
        }

        MoveToNextPatrolPoint();
    }

    private bool HasReachedDestination()
    {
        return agent.remainingDistance <= agent.stoppingDistance;
    }

    private void MoveToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Count == 0)
            return;

        if (currentPatrolIndex >= patrolPoints.Count)
            currentPatrolIndex = 0;

        agent.SetDestination(patrolPoints[currentPatrolIndex]);
        currentPatrolIndex++;
    }
}
