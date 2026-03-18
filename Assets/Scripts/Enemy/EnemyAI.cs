using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyAI : MonoBehaviour
{

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

    [Header("Combat")]
    [SerializeField] private float attackDistance = 0.5f;
    [SerializeField] private float attackDelay = 1f;

    [Header("State Delays")]
    [SerializeField] private float lostSightDelay = 0.3f;
    [SerializeField] private float investigateWaitDelay = 1.5f;
    [SerializeField] private float chaseReactionDelay = 0.1f;

    [SerializeField] private float stuckCheckInterval = 0.5f;
    [SerializeField] private float stuckDistanceThreshold = 0.08f;

    [SerializeField] private GameObject exclamationMark;
 

    private Vector2 lastStuckCheckPosition;
    private float stuckCheckTimer;

    public UnityEvent OnAttackPressed;
    public UnityEvent<Vector2> OnMovementInput;
    public UnityEvent<Vector2> OnPointerInput;

    private Vector2 movementInput;
    private float lastAttackTime;

    private AIState currentState = AIState.Patrol;
    private float stateTimer = 0f;

 
    private AIState queuedStateAfterWait = AIState.Patrol;


    private void Start()
    {
        InvokeRepeating(nameof(PerformDetection), 0f, detectionDelay);
        ChangeState(AIState.Patrol);
    }

    private void PerformDetection()
    {
        foreach (Detector detector in detectors)
        {
            detector.Detect(aiData);
        }
    }

    private void Update()
    {
        bool seesPlayer = aiData.targets != null && aiData.targets.Count > 0;

       
        if (seesPlayer)
        {
            aiData.currentTarget = aiData.targets[0];
            aiData.lastSeenPosition = aiData.currentTarget.position;
            aiData.hasLastSeenPosition = true;
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


        stuckCheckTimer += Time.deltaTime;

        if (stuckCheckTimer >= stuckCheckInterval)
        {
            float movedDistance = Vector2.Distance(transform.position, lastStuckCheckPosition);

            if (movedDistance < stuckDistanceThreshold)
            {
                // If patrolling, skip to next waypoint
                if (currentState == AIState.Patrol && aiData.patrolPoints != null && aiData.patrolPoints.Count > 0)
                {
                    aiData.currentPatrolIndex = (aiData.currentPatrolIndex + 1) % aiData.patrolPoints.Count;
                }

                // If investigating, give up and go back to patrol
                if (currentState == AIState.Investigate)
                {
                    aiData.hasLastSeenPosition = false;
                    queuedStateAfterWait = AIState.Patrol;
                    ChangeState(AIState.Wait, 0.2f);
                }
            }

            lastStuckCheckPosition = transform.position;
            stuckCheckTimer = 0f;
        }
        Debug.Log($"State: {currentState} | SeesPlayer: {seesPlayer} | Targets: {aiData.targets?.Count}");

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

                    if (seesPlayer && aiData.currentTarget != null)
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
                return chaseBehaviours;

            case AIState.Attack:
                return new List<SteeringBehaviour>();

            case AIState.Investigate:
                return investigateBehaviours;

            case AIState.Wait:
                // Stand still during waits
                return new List<SteeringBehaviour>();

            case AIState.Patrol:
            default:
                return patrolBehaviours;
        }
    }

    private Transform GetLookTarget(bool seesPlayer)
    {
        if (seesPlayer && aiData.currentTarget != null)
            return aiData.currentTarget;

        if (currentState == AIState.Patrol && aiData.currentPatrolTarget != null)
            return aiData.currentPatrolTarget;

        return null;
    }

    private void HandleAttack(bool seesPlayer)
    {
        if (currentState != AIState.Attack)
            return;

        if (!seesPlayer || aiData.currentTarget == null)
            return;

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
                exclamationMark.SetActive(false);
                break;

            case AIState.Chase:
                exclamationMark.SetActive(false);
                break;

            case AIState.Investigate:
                exclamationMark.SetActive(false);
                break;

            case AIState.Wait:
                exclamationMark.SetActive(true);
                movementInput = Vector2.zero;
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
                exclamationMark.SetActive(true);
            else
                exclamationMark.SetActive(false);
        }
    }
}
