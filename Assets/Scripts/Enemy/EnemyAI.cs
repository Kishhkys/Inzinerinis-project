using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    private static readonly List<SteeringBehaviour> EmptyBehaviours = new();

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
    [SerializeField] private float investigateArrivalDistance = 0.4f;

    [Header("Stuck Check")]
    [SerializeField] private float stuckCheckInterval = 0.5f;
    [SerializeField] private float stuckDistanceThreshold = 0.08f;
    [SerializeField] private float stuckRecoveryGracePeriod = 1.5f;
    [SerializeField] private int stuckEventsBeforeSkipPatrol = 2;
    [SerializeField] private float stuckIgnoreDuration = 1.2f;
    [SerializeField] private float stuckImpulseForce = 2f;

    [Header("Alert Audio")]
    [SerializeField] private float alertSoundVolume = 0.3f;
    [SerializeField] private float growlDelay = 1f;

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

    private Rigidbody2D rb;
    private Coroutine alertSoundCoroutine;

    private void Awake()
    {
        TryGetComponent(out rb);
    }

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

        bool rawSeesPlayer = HasDetectedTarget();
        bool hasLineOfSight = rawSeesPlayer && CanSeeTarget(aiData.targets[0]);
        bool seesPlayer = rawSeesPlayer && hasLineOfSight && Time.time >= ignorePlayerUntil;

        UpdateTargetMemory(rawSeesPlayer, hasLineOfSight);
        HandleTransitions(seesPlayer);
        UpdateMovementInput();
        UpdateLookDirection(seesPlayer);
        HandleAttack(seesPlayer);
        OnMovementInput?.Invoke(movementInput);
        CheckIfStuck(rawSeesPlayer && hasLineOfSight);
    }

    private bool HasDetectedTarget()
    {
        return aiData.targets != null && aiData.targets.Count > 0;
    }

    private void UpdateTargetMemory(bool rawSeesPlayer, bool hasLineOfSight)
    {
        if (rawSeesPlayer && hasLineOfSight)
        {
            aiData.currentTarget = aiData.targets[0];

            if (Time.time >= ignorePlayerUntil)
            {
                aiData.lastSeenPosition = aiData.currentTarget.position;
                aiData.hasLastSeenPosition = true;
            }

            return;
        }

        aiData.currentTarget = null;
    }

    private void UpdateMovementInput()
    {
        List<SteeringBehaviour> activeBehaviours = GetActiveBehaviours();
        movementInput = movementDirectionSolver.GetDirectionToMove(activeBehaviours, aiData);
    }

    private void UpdateLookDirection(bool seesPlayer)
    {
        Transform lookTarget = GetLookTarget(seesPlayer);

        if (lookTarget != null)
        {
            OnPointerInput?.Invoke(lookTarget.position);
        }
        else if (currentState == AIState.Investigate && aiData.hasLastSeenPosition)
        {
            OnPointerInput?.Invoke(aiData.lastSeenPosition);
        }
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

                ignorePlayerUntil = Time.time + stuckIgnoreDuration;

                Vector2 randomDir = Random.insideUnitCircle.normalized;
                if (rb != null)
                {
                    rb.AddForce(randomDir * stuckImpulseForce, ForceMode2D.Impulse);
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

                        if (distToLastSeen <= investigateArrivalDistance)
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
        return currentState switch
        {
            AIState.Chase => chaseBehaviours ?? EmptyBehaviours,
            AIState.Investigate => investigateBehaviours ?? EmptyBehaviours,
            AIState.Attack or AIState.Wait => EmptyBehaviours,
            AIState.Patrol => patrolBehaviours ?? EmptyBehaviours,
            _ => patrolBehaviours ?? EmptyBehaviours
        };
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

                alertSoundCoroutine ??= StartCoroutine(PlayStingerThenGrowl());
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
        SoundEffectManager.PlayClip("Monster", "Stinger", alertSoundVolume);

        yield return new WaitForSeconds(growlDelay);

        SoundEffectManager.PlayClip("Monster", "Monster_growl", alertSoundVolume);

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
