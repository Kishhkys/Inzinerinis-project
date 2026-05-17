using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyAI : MonoBehaviour
{
    private static readonly List<SteeringBehaviour> EmptyBehaviours = new List<SteeringBehaviour>();

    private enum AIState
    {
        Patrol,
        Chase,
        Investigate,
        Wait,
        Attack
    }

    [Header("Behaviour Sets")]
    [SerializeField] private List<SteeringBehaviour> chaseBehaviours;
    [SerializeField] private List<SteeringBehaviour> investigateBehaviours;
    [SerializeField] private List<SteeringBehaviour> patrolBehaviours;

    [Header("Detectors")]
    [SerializeField] private List<Detector> detectors;

    [Header("References")]
    [SerializeField] private AIData aiData;
    [SerializeField] private ContextSolver movementDirectionSolver;

    [Header("Detection")]
    [SerializeField] private float detectionDelay = 0.05f;

    [Header("Line Of Sight")]
    [SerializeField] private LayerMask obstacleLayerMask;

    [Header("Combat")]
    [SerializeField] private float attackDistance = 0.5f;
    [SerializeField] private float attackDelay = 1f;

    [Header("State Delays")]
    [SerializeField] private float lostSightDelay = 0.3f;
    [SerializeField] private float investigateWaitDelay = 1.5f;
    [SerializeField] private float chaseReactionDelay = 0.1f;

    [Header("Stuck Check")]
    [SerializeField] private float stuckCheckInterval = 0.5f;
    [SerializeField] private float stuckDistanceThreshold = 0.08f;
    [SerializeField] private float stuckRecoveryGracePeriod = 1.5f;
    [SerializeField] private int stuckEventsBeforeSkipPatrol = 2;

    [Header("UI")]
    [SerializeField] private GameObject exclamationMark;

    [SerializeField] private UnityEvent OnAttackPressed;
    [SerializeField] private UnityEvent<Vector2> OnMovementInput;
    [SerializeField] private UnityEvent<Vector2> OnPointerInput;

    private Vector2 lastStuckCheckPosition;
    private float stuckCheckTimer;
    private float stuckGraceUntil = 0f;
    private int patrolStuckCount = 0;

    private Vector2 movementInput;
    private float lastAttackTime;

    private AIState currentState = AIState.Patrol;
    private float stateTimer = 0f;
    private AIState queuedStateAfterWait = AIState.Patrol;

    private float ignorePlayerUntil = 0f;

    private Coroutine alertSoundCoroutine;

    private void Start()
    {
        lastStuckCheckPosition = transform.position;

        InvokeRepeating(nameof(PerformDetection), 0f, detectionDelay);

        ChangeState(AIState.Patrol);
    }

    private void PerformDetection()
    {
        if (detectors == null)
        {
            return;
        }

        foreach (Detector detector in detectors)
        {
            if (detector != null)
            {
                detector.Detect(aiData);
            }
        }
    }

    private void Update()
    {
        if (aiData == null || movementDirectionSolver == null)
        {
            return;
        }

        bool rawSeesPlayer = aiData.targets != null && aiData.targets.Count > 0;

        bool hasLineOfSight = false;

        if (rawSeesPlayer)
        {
            hasLineOfSight = CanSeeTarget(aiData.targets[0]);
        }

        bool seesPlayer = rawSeesPlayer && hasLineOfSight && Time.time >= ignorePlayerUntil;

        if (rawSeesPlayer && hasLineOfSight)
        {
            aiData.currentTarget = aiData.targets[0];

            if (Time.time >= ignorePlayerUntil)
            {
                aiData.lastSeenPosition = aiData.currentTarget.position;
                aiData.hasLastSeenPosition = true;
            }
        }
        else
        {
            aiData.currentTarget = null;
        }

        HandleTransitions(seesPlayer);

        List<SteeringBehaviour> activeBehaviours = GetActiveBehaviours();
        movementInput = movementDirectionSolver.GetDirectionToMove(activeBehaviours, aiData);

        Transform lookTarget = GetLookTarget(seesPlayer);

        if (lookTarget != null)
        {
            OnPointerInput?.Invoke(lookTarget.position);
        }
        else if (currentState == AIState.Investigate && aiData.hasLastSeenPosition)
        {
            OnPointerInput?.Invoke(aiData.lastSeenPosition);
        }

        HandleAttack(seesPlayer);

        OnMovementInput?.Invoke(movementInput);

        CheckIfStuck(rawSeesPlayer && hasLineOfSight);

        Debug.Log($"State: {currentState} | MoveInput: {movementInput} | PatrolIdx: {aiData.currentPatrolIndex} | StuckCount: {patrolStuckCount}");
    }

    private bool CanSeeTarget(Transform target)
    {
        if (target == null)
        {
            return false;
        }

        Vector2 origin = transform.position;
        Vector2 targetPosition = target.position;

        Vector2 direction = targetPosition - origin;
        float distance = direction.magnitude;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            direction.normalized,
            distance,
            obstacleLayerMask
        );

        return hit.collider == null;
    }

    private void CheckIfStuck(bool rawSeesPlayer)
    {
        if (currentState == AIState.Wait || currentState == AIState.Attack)
        {
            lastStuckCheckPosition = transform.position;
            stuckCheckTimer = 0f;
            return;
        }

        if (Time.time < stuckGraceUntil)
        {
            lastStuckCheckPosition = transform.position;
            stuckCheckTimer = 0f;
            return;
        }

        if (currentState != AIState.Patrol)
        {
            patrolStuckCount = 0;
        }

        stuckCheckTimer += Time.deltaTime;

        if (stuckCheckTimer < stuckCheckInterval)
        {
            return;
        }

        float movedDistance = Vector2.Distance(transform.position, lastStuckCheckPosition);

        if (movedDistance < stuckDistanceThreshold)
        {
            Debug.Log("Enemy stuck");

            stuckGraceUntil = Time.time + stuckRecoveryGracePeriod;

            if (currentState == AIState.Patrol)
            {
                patrolStuckCount++;

                if (patrolStuckCount >= stuckEventsBeforeSkipPatrol)
                {
                    if (aiData.patrolPoints != null && aiData.patrolPoints.Count > 0)
                    {
                        aiData.currentPatrolIndex = (aiData.currentPatrolIndex + 1) % aiData.patrolPoints.Count;
                    }

                    patrolStuckCount = 0;
                }

                if (patrolBehaviours != null)
                {
                    foreach (SteeringBehaviour b in patrolBehaviours)
                    {
                        PatrolBehaviour patrol = b as PatrolBehaviour;

                        if (patrol != null)
                        {
                            patrol.ForceRepath();
                        }
                    }
                }
            }
            else if (currentState == AIState.Investigate)
            {
                aiData.hasLastSeenPosition = false;
                queuedStateAfterWait = AIState.Patrol;
                ChangeState(AIState.Wait, 0.2f);
            }
            else if (currentState == AIState.Chase)
            {
                if (rawSeesPlayer && aiData.currentTarget != null)
                {
                    aiData.lastSeenPosition = aiData.currentTarget.position;
                    aiData.hasLastSeenPosition = true;
                }

                ignorePlayerUntil = Time.time + 1.2f;

                Vector2 randomDir = Random.insideUnitCircle.normalized;
                Rigidbody2D rb = GetComponent<Rigidbody2D>();

                if (rb != null)
                {
                    rb.AddForce(randomDir * 2f, ForceMode2D.Impulse);
                }

                ChangeState(AIState.Investigate);
            }
        }
        else
        {
            patrolStuckCount = 0;
        }

        lastStuckCheckPosition = transform.position;
        stuckCheckTimer = 0f;
    }

    private void HandleTransitions(bool seesPlayer)
    {
        switch (currentState)
        {
            case AIState.Patrol:
                {
                    if (seesPlayer)
                    {
                        queuedStateAfterWait = AIState.Chase;
                        ChangeState(AIState.Wait, chaseReactionDelay);
                    }

                    break;
                }

            case AIState.Chase:
                {
                    if (!seesPlayer)
                    {
                        queuedStateAfterWait = AIState.Investigate;
                        ChangeState(AIState.Wait, lostSightDelay);
                        break;
                    }

                    if (aiData.currentTarget != null)
                    {
                        float dist = Vector2.Distance(transform.position, aiData.currentTarget.position);

                        if (dist <= attackDistance)
                        {
                            ChangeState(AIState.Attack);
                        }
                    }

                    break;
                }

            case AIState.Attack:
                {
                    if (!seesPlayer)
                    {
                        queuedStateAfterWait = AIState.Investigate;
                        ChangeState(AIState.Wait, lostSightDelay);
                        break;
                    }

                    if (aiData.currentTarget != null)
                    {
                        float dist = Vector2.Distance(transform.position, aiData.currentTarget.position);

                        if (dist > attackDistance)
                        {
                            ChangeState(AIState.Chase);
                        }
                    }

                    break;
                }

            case AIState.Investigate:
                {
                    if (seesPlayer)
                    {
                        queuedStateAfterWait = AIState.Chase;
                        ChangeState(AIState.Wait, chaseReactionDelay);
                        return;
                    }

                    if (aiData.hasLastSeenPosition)
                    {
                        float distToLastSeen = Vector2.Distance(transform.position, aiData.lastSeenPosition);

                        if (distToLastSeen <= 0.4f)
                        {
                            queuedStateAfterWait = AIState.Patrol;
                            ChangeState(AIState.Wait, investigateWaitDelay);
                        }
                    }
                    else
                    {
                        ChangeState(AIState.Patrol);
                    }

                    break;
                }

            case AIState.Wait:
                {
                    if (seesPlayer && queuedStateAfterWait != AIState.Chase)
                    {
                        queuedStateAfterWait = AIState.Chase;
                        stateTimer = Time.time + chaseReactionDelay;
                    }

                    if (Time.time >= stateTimer)
                    {
                        ChangeState(queuedStateAfterWait);
                    }

                    break;
                }
        }
    }

    private List<SteeringBehaviour> GetActiveBehaviours()
    {
        switch (currentState)
        {
            case AIState.Chase:
                return chaseBehaviours ?? EmptyBehaviours;

            case AIState.Investigate:
                return investigateBehaviours ?? EmptyBehaviours;

            case AIState.Attack:
            case AIState.Wait:
                return EmptyBehaviours;

            case AIState.Patrol:
            default:
                return patrolBehaviours ?? EmptyBehaviours;
        }
    }

    private Transform GetLookTarget(bool seesPlayer)
    {
        if (seesPlayer && aiData.currentTarget != null)
        {
            return aiData.currentTarget;
        }

        if (currentState == AIState.Patrol && aiData.currentPatrolTarget != null)
        {
            return aiData.currentPatrolTarget;
        }

        return null;
    }

    private void HandleAttack(bool seesPlayer)
    {
        if (currentState != AIState.Attack)
        {
            return;
        }

        if (!seesPlayer || aiData.currentTarget == null)
        {
            return;
        }

        movementInput = Vector2.zero;

        if (Time.time >= lastAttackTime + attackDelay)
        {
            OnAttackPressed?.Invoke();
            lastAttackTime = Time.time;
        }
    }

    private void ChangeState(AIState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case AIState.Patrol:
                SetExclamation(false);
                break;

            case AIState.Chase:
                SetExclamation(false);
                break;

            case AIState.Investigate:
                SetExclamation(false);
                break;

            case AIState.Attack:
                SetExclamation(false);
                break;

            case AIState.Wait:
                movementInput = Vector2.zero;
                SetExclamation(queuedStateAfterWait == AIState.Chase);
                break;
        }
    }

    private void ChangeState(AIState newState, float delay)
    {
        currentState = newState;
        stateTimer = Time.time + delay;

        if (currentState == AIState.Wait)
        {
            movementInput = Vector2.zero;

            if (queuedStateAfterWait == AIState.Chase)
            {
                SetExclamation(true);

                if (alertSoundCoroutine == null)
                {
                    alertSoundCoroutine = StartCoroutine(PlayStingerThenGrowl());
                }
            }
            else
            {
                SetExclamation(false);
            }
        }
        else
        {
            SetExclamation(false);
        }
    }

    private IEnumerator PlayStingerThenGrowl()
    {
        SoundEffectManager.PlayClip("Monster", "Stinger", 0.3f);

        yield return new WaitForSeconds(1f);

        SoundEffectManager.PlayClip("Monster", "Monster_growl", 0.3f);

        alertSoundCoroutine = null;
    }

    private void SetExclamation(bool active)
    {
        if (exclamationMark != null)
        {
            exclamationMark.SetActive(active);
        }
    }



}
