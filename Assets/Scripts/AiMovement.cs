using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyVisionCone))]
public class AIMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float basePatrolSpeed = 2f;
    [SerializeField] private float baseChaseSpeed = 5f;
    [SerializeField] private float baseSearchSpeed = 3.5f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float accelerationRate = 2f;
    [SerializeField] private float waitTimeAtPatrolPoint = 2f;
    [SerializeField] private float searchDuration = 8f;
    [SerializeField] private float searchAreaRadius = 5f;
    [SerializeField] private float wallAvoidanceCheckInterval = 0.5f;

    [Header("Chase Settings")]
    [SerializeField] private float minChaseDistance = 30f;
    [SerializeField] private float pathUpdateInterval = 0.2f;

    [Header("Prediction Settings")]
    [SerializeField] private float predictionMultiplier = 0.8f;
    private Vector3 predictedPlayerPos;

    [Header("Audio Settings")]
    public AudioSource walkingAudioSource;
    public AudioSource growlAudioSource;
    public AudioSource screamAudioSource;
    public bool canScream = false;
    [Range(0f, 1f)] public float walkingVolume = 0.5f;
    [Range(0f, 1f)] public float idleVolume = 0.1f;
    [Range(0f, 1f)] public float growlVolume = 0.7f;
    [Range(0f, 1f)] public float screamVolume = 1f;

    private NavMeshAgent agent;
    private EnemyVisionCone visionCone;
    private List<Vector3> patrolPoints;
    private int currentPatrolIndex = 0;
    private Vector3 lastKnownPlayerPos;
    private bool isInLight = false;
    private Coroutine searchCoroutine;
    private float lastWallCheck = 0f;
    private float lastPathUpdate = 0f;

    private enum AIState { Patrolling, Chasing, Searching, WaitingAtPatrol }
    private AIState currentState;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        visionCone = GetComponent<EnemyVisionCone>();
        agent.speed = basePatrolSpeed;

        if (walkingAudioSource != null)
        {
            walkingAudioSource.loop = true;
            walkingAudioSource.Play();
        }
        InitializePatrolPoints();
        MoveToNextPatrolPoint();

        StartCoroutine(PlayRandomGrowls());
    }

    private void Update()
    {
        UpdateAudio();
        UpdateMovement();

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

    public bool IsInLight
    {
        get => isInLight;
        set
        {
            isInLight = value;
            agent.isStopped = isInLight;
        }
    }

    private void PredictTargetPosition()
    {
        if (visionCone.TargetInSight)
        {
            Vector3 targetVelocity = (visionCone.LastKnownTargetPosition - lastKnownPlayerPos) / Time.deltaTime;
            predictedPlayerPos = visionCone.LastKnownTargetPosition + targetVelocity * predictionMultiplier;

            if (NavMesh.SamplePosition(predictedPlayerPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                predictedPlayerPos = hit.position;
            }
            else
            {
                predictedPlayerPos = lastKnownPlayerPos;
            }
        }
    }

    private void UpdateMovement()
    {
        if (isInLight) return;

        agent.isStopped = false;
        agent.speed = GetCurrentStateSpeed();

        if (currentState != AIState.Chasing && Time.time - lastWallCheck > wallAvoidanceCheckInterval)
        {
            lastWallCheck = Time.time;

            if (visionCone.IsWallAhead(out Vector3 betterDirection))
            {
                Vector3 newTargetPosition = transform.position + betterDirection * 3f;
                NavMeshPath path = new NavMeshPath();

                if (NavMesh.CalculatePath(transform.position, newTargetPosition, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    agent.SetPath(path);
                    agent.speed *= 0.8f;
                }
            }
        }
    }

    private void UpdateAudio()
    {
        if (walkingAudioSource != null)
            walkingAudioSource.volume = agent.velocity.magnitude > 0.1f ? walkingVolume : idleVolume;

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

    private float GetCurrentStateSpeed()
    {
        return currentState switch
        {
            AIState.Chasing => baseChaseSpeed,
            AIState.Searching => baseSearchSpeed,
            _ => basePatrolSpeed,
        };
    }

    private void HandlePatrolState()
    {
        if (visionCone.TargetInSight)
        {
            TransitionToChasing();
        }
        else if (HasReachedDestination())
        {
            StartCoroutine(WaitAtPatrolPoint());
        }
    }

    private void HandleChaseState()
    {
        if (visionCone.TargetInSight)
        {
            lastKnownPlayerPos = visionCone.LastKnownTargetPosition;

            if (Time.time - lastPathUpdate > pathUpdateInterval)
            {
                lastPathUpdate = Time.time;
                PredictTargetPosition();

                if (Vector3.Distance(transform.position, predictedPlayerPos) <= minChaseDistance)
                {
                    agent.SetDestination(predictedPlayerPos);
                }
            }
        }
        else
        {
            TransitionToSearching();
        }
    }

    private void HandleSearchState()
    {
        if (visionCone.TargetInSight)
            TransitionToChasing();
    }

    private void HandleWaitState()
    {
        if (visionCone.TargetInSight)
            TransitionToChasing();
    }

    private void TransitionToChasing()
    {
        if (currentState != AIState.Chasing)
        {
            currentState = AIState.Chasing;
            agent.speed = baseChaseSpeed;
            agent.acceleration = accelerationRate * 2;
            agent.angularSpeed = rotationSpeed * 2;
            agent.stoppingDistance = 0.5f;

            if (searchCoroutine != null)
            {
                StopCoroutine(searchCoroutine);
            }

            lastPathUpdate = 0f;
            agent.SetDestination(visionCone.LastKnownTargetPosition);
        }
    }

    private void TransitionToSearching()
    {
        currentState = AIState.Searching;
        agent.speed = baseSearchSpeed;
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
            }
            yield return null;
        }

        currentState = AIState.Patrolling;
        agent.speed = basePatrolSpeed;
        MoveToNextPatrolPoint();
    }

    private IEnumerator WaitAtPatrolPoint()
    {
        currentState = AIState.WaitingAtPatrol;
        yield return new WaitForSeconds(waitTimeAtPatrolPoint);
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

    private IEnumerator PlayRandomGrowls()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(60f, 80f));
            if (Random.value <= 0.333f && currentState != AIState.Chasing && growlAudioSource != null && !growlAudioSource.isPlaying)
            {
                growlAudioSource.volume = growlVolume;
                growlAudioSource.Play();
            }
        }
    }

    private void InitializePatrolPoints()
    {
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
    }
}
