using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyAI : MonoBehaviour
{
    //private enum AIState
    //{
    //    Patrol,
    //    Chase,
    //    Investigate,
    //    Wait,
    //    Attack
    //}

    //[Header("Behaviour Sets")]
    //[SerializeField] private List<SteeringBehaviour> chaseBehaviours;
    //[SerializeField] private List<SteeringBehaviour> investigateBehaviours;
    //[SerializeField] private List<SteeringBehaviour> patrolBehaviours;

    //[Header("Detectors")]
    //[SerializeField] private List<Detector> detectors;

    //[Header("References")]
    //[SerializeField] private AIData aiData;
    //[SerializeField] private ContextSolver movementDirectionSolver;

    //[Header("Detection")]
    //[SerializeField] private float detectionDelay = 0.05f;

    //[Header("Combat")]
    //[SerializeField] private float attackDistance = 0.5f;
    //[SerializeField] private float attackDelay = 1f;

    //[Header("State Delays")]
    //[SerializeField] private float lostSightDelay = 0.3f;
    //[SerializeField] private float investigateWaitDelay = 1.5f;
    //[SerializeField] private float chaseReactionDelay = 0.1f;

    //[Header("Stuck Check")]
    //[SerializeField] private float stuckCheckInterval = 0.5f;
    //[SerializeField] private float stuckDistanceThreshold = 0.08f;

    //[SerializeField] private GameObject exclamationMark;

    //private Vector2 lastStuckCheckPosition;
    //private float stuckCheckTimer;

    //public UnityEvent OnAttackPressed;
    //public UnityEvent<Vector2> OnMovementInput;
    //public UnityEvent<Vector2> OnPointerInput;

    //private Vector2 movementInput;
    //private float lastAttackTime;

    //private AIState currentState = AIState.Patrol;
    //private float stateTimer = 0f;
    //private AIState queuedStateAfterWait = AIState.Patrol;

    //private float ignorePlayerUntil = 0f;

    //private void Start()
    //{
    //    lastStuckCheckPosition = transform.position;
    //    InvokeRepeating(nameof(PerformDetection), 0f, detectionDelay);
    //    ChangeState(AIState.Patrol);
    //}

    //private void PerformDetection()
    //{
    //    foreach (Detector detector in detectors)
    //    {
    //        detector.Detect(aiData);
    //    }
    //}

    //private void Update()
    //{
    //    bool rawSeesPlayer = aiData.targets != null && aiData.targets.Count > 0;
    //    bool seesPlayer = rawSeesPlayer && Time.time >= ignorePlayerUntil;

    //    if (rawSeesPlayer)
    //    {
    //        aiData.currentTarget = aiData.targets[0];

    //        if (Time.time >= ignorePlayerUntil)
    //        {
    //            aiData.lastSeenPosition = aiData.currentTarget.position;
    //            aiData.hasLastSeenPosition = true;
    //        }
    //    }
    //    else
    //    {
    //        aiData.currentTarget = null;
    //    }

    //    HandleTransitions(seesPlayer);

    //    List<SteeringBehaviour> activeBehaviours = GetActiveBehaviours();
    //    movementInput = movementDirectionSolver.GetDirectionToMove(activeBehaviours, aiData);

    //    Transform lookTarget = GetLookTarget(seesPlayer);
    //    if (lookTarget != null)
    //    {
    //        OnPointerInput?.Invoke(lookTarget.position);
    //    }
    //    else if (currentState == AIState.Investigate && aiData.hasLastSeenPosition)
    //    {
    //        OnPointerInput?.Invoke(aiData.lastSeenPosition);
    //    }

    //    HandleAttack(seesPlayer);

    //    OnMovementInput?.Invoke(movementInput);

    //    CheckIfStuck(rawSeesPlayer);

    //    Debug.Log($"State: {currentState} | SeesPlayer: {seesPlayer} | RawSeesPlayer: {rawSeesPlayer} | Targets: {aiData.targets?.Count}");
    //}

    //private void CheckIfStuck(bool rawSeesPlayer)
    //{
    //    if (currentState == AIState.Wait || currentState == AIState.Attack)
    //    {
    //        lastStuckCheckPosition = transform.position;
    //        stuckCheckTimer = 0f;
    //        return;
    //    }

    //    stuckCheckTimer += Time.deltaTime;

    //    if (stuckCheckTimer < stuckCheckInterval)
    //        return;

    //    float movedDistance = Vector2.Distance(transform.position, lastStuckCheckPosition);

    //    if (movedDistance < stuckDistanceThreshold)
    //    {
    //        Debug.Log("Enemy stuck");

    //        if (currentState == AIState.Patrol)
    //        {
    //            if (aiData.patrolPoints != null && aiData.patrolPoints.Count > 0)
    //            {
    //                aiData.currentPatrolIndex = (aiData.currentPatrolIndex + 1) % aiData.patrolPoints.Count;
    //            }
    //        }
    //        else if (currentState == AIState.Investigate)
    //        {
    //            aiData.hasLastSeenPosition = false;
    //            queuedStateAfterWait = AIState.Patrol;
    //            ChangeState(AIState.Wait, 0.2f);
    //        }
    //        else if (currentState == AIState.Chase)
    //        {
    //            if (rawSeesPlayer && aiData.currentTarget != null)
    //            {
    //                aiData.lastSeenPosition = aiData.currentTarget.position;
    //                aiData.hasLastSeenPosition = true;
    //            }

    //            ignorePlayerUntil = Time.time + 0.75f;
    //            ChangeState(AIState.Investigate);
    //        }
    //    }

    //    lastStuckCheckPosition = transform.position;
    //    stuckCheckTimer = 0f;
    //}

    //private void HandleTransitions(bool seesPlayer)
    //{
    //    switch (currentState)
    //    {
    //        case AIState.Patrol:
    //            {
    //                if (seesPlayer)
    //                {
    //                    queuedStateAfterWait = AIState.Chase;
    //                    ChangeState(AIState.Wait, chaseReactionDelay);
    //                }
    //                break;
    //            }

    //        case AIState.Chase:
    //            {
    //                if (!seesPlayer)
    //                {
    //                    queuedStateAfterWait = AIState.Investigate;
    //                    ChangeState(AIState.Wait, lostSightDelay);
    //                    break;
    //                }

    //                if (aiData.currentTarget != null)
    //                {
    //                    float dist = Vector2.Distance(transform.position, aiData.currentTarget.position);
    //                    if (dist <= attackDistance)
    //                    {
    //                        ChangeState(AIState.Attack);
    //                    }
    //                }
    //                break;
    //            }

    //        case AIState.Attack:
    //            {
    //                if (!seesPlayer)
    //                {
    //                    queuedStateAfterWait = AIState.Investigate;
    //                    ChangeState(AIState.Wait, lostSightDelay);
    //                    break;
    //                }

    //                if (aiData.currentTarget != null)
    //                {
    //                    float dist = Vector2.Distance(transform.position, aiData.currentTarget.position);
    //                    if (dist > attackDistance)
    //                    {
    //                        ChangeState(AIState.Chase);
    //                    }
    //                }
    //                break;
    //            }

    //        case AIState.Investigate:
    //            {
    //                if (seesPlayer)
    //                {
    //                    queuedStateAfterWait = AIState.Chase;
    //                    ChangeState(AIState.Wait, chaseReactionDelay);
    //                    return;
    //                }

    //                if (aiData.hasLastSeenPosition)
    //                {
    //                    float distToLastSeen = Vector2.Distance(transform.position, aiData.lastSeenPosition);
    //                    if (distToLastSeen <= 0.4f)
    //                    {
    //                        queuedStateAfterWait = AIState.Patrol;
    //                        ChangeState(AIState.Wait, investigateWaitDelay);
    //                    }
    //                }
    //                else
    //                {
    //                    ChangeState(AIState.Patrol);
    //                }

    //                break;
    //            }

    //        case AIState.Wait:
    //            {
    //                if (seesPlayer && queuedStateAfterWait != AIState.Chase)
    //                {
    //                    queuedStateAfterWait = AIState.Chase;
    //                    stateTimer = Time.time + chaseReactionDelay;
    //                }

    //                if (Time.time >= stateTimer)
    //                {
    //                    ChangeState(queuedStateAfterWait);
    //                }
    //                break;
    //            }
    //    }
    //}

    //private List<SteeringBehaviour> GetActiveBehaviours()
    //{
    //    switch (currentState)
    //    {
    //        case AIState.Chase:
    //            return chaseBehaviours;

    //        case AIState.Investigate:
    //            return investigateBehaviours;

    //        case AIState.Attack:
    //        case AIState.Wait:
    //            return new List<SteeringBehaviour>();

    //        case AIState.Patrol:
    //        default:
    //            return patrolBehaviours;
    //    }
    //}

    //private Transform GetLookTarget(bool seesPlayer)
    //{
    //    if (seesPlayer && aiData.currentTarget != null)
    //        return aiData.currentTarget;

    //    if (currentState == AIState.Patrol && aiData.currentPatrolTarget != null)
    //        return aiData.currentPatrolTarget;

    //    return null;
    //}

    //private void HandleAttack(bool seesPlayer)
    //{
    //    if (currentState != AIState.Attack)
    //        return;

    //    if (!seesPlayer || aiData.currentTarget == null)
    //        return;

    //    movementInput = Vector2.zero;

    //    if (Time.time >= lastAttackTime + attackDelay)
    //    {
    //        OnAttackPressed?.Invoke();
    //        lastAttackTime = Time.time;
    //    }
    //}

    //private void ChangeState(AIState newState)
    //{
    //    currentState = newState;

    //    switch (currentState)
    //    {
    //        case AIState.Patrol:
    //            exclamationMark.SetActive(false);
    //            break;

    //        case AIState.Chase:
    //            exclamationMark.SetActive(false);
    //            break;

    //        case AIState.Investigate:
    //            exclamationMark.SetActive(false);
    //            break;

    //        case AIState.Attack:
    //            exclamationMark.SetActive(false);
    //            break;

    //        case AIState.Wait:
    //            exclamationMark.SetActive(true);
    //            movementInput = Vector2.zero;
    //            break;
    //    }
    //}

    //private void ChangeState(AIState newState, float delay)
    //{
    //    currentState = newState;
    //    stateTimer = Time.time + delay;

    //    if (currentState == AIState.Wait)
    //    {
    //        movementInput = Vector2.zero;

    //        if (queuedStateAfterWait == AIState.Chase)
    //        {
    //            exclamationMark.SetActive(true);
    //            SoundEffectManager.PlayClip("Monster", "Monster_growl", 0.5f);
    //        }

    //        else
    //            exclamationMark.SetActive(false);
    //    }
    //    else
    //    {
    //        exclamationMark.SetActive(false);
    //    }
    //}


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

    [Header("Stuck Check")]
    [SerializeField] private float stuckCheckInterval = 0.5f;
    [SerializeField] private float stuckDistanceThreshold = 0.08f;
    [Tooltip("Po stuck eventos — kiek sekundziu duoti enemy laisvai pajudeti pries kita stuck check")]
    [SerializeField] private float stuckRecoveryGracePeriod = 1.5f;
    [Tooltip("Po N stuck eventu is eiles per ta pacia patrol pozicija — perokam i kita patrol task")]
    [SerializeField] private int stuckEventsBeforeSkipPatrol = 2;

    [SerializeField] private GameObject exclamationMark;

    private Vector2 lastStuckCheckPosition;
    private float stuckCheckTimer;
    private float stuckGraceUntil = 0f;
    private int patrolStuckCount = 0;

    public UnityEvent OnAttackPressed;
    public UnityEvent<Vector2> OnMovementInput;
    public UnityEvent<Vector2> OnPointerInput;

    private Vector2 movementInput;
    private float lastAttackTime;

    private AIState currentState = AIState.Patrol;
    private float stateTimer = 0f;
    private AIState queuedStateAfterWait = AIState.Patrol;

    private float ignorePlayerUntil = 0f;

    private void Start()
    {
        lastStuckCheckPosition = transform.position;
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
        bool rawSeesPlayer = aiData.targets != null && aiData.targets.Count > 0;
        bool seesPlayer = rawSeesPlayer && Time.time >= ignorePlayerUntil;

        if (rawSeesPlayer)
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

        CheckIfStuck(rawSeesPlayer);

        Debug.Log($"State: {currentState} | MoveInput: {movementInput} | PatrolIdx: {aiData.currentPatrolIndex} | StuckCount: {patrolStuckCount}");
    }

    private void CheckIfStuck(bool rawSeesPlayer)
    {
        if (currentState == AIState.Wait || currentState == AIState.Attack)
        {
            lastStuckCheckPosition = transform.position;
            stuckCheckTimer = 0f;
            return;
        }

        // Grace period — duodam laiko pajudeti po praeito recovery
        if (Time.time < stuckGraceUntil)
        {
            lastStuckCheckPosition = transform.position;
            stuckCheckTimer = 0f;
            return;
        }

        if (currentState != AIState.Patrol)
            patrolStuckCount = 0;

        stuckCheckTimer += Time.deltaTime;

        if (stuckCheckTimer < stuckCheckInterval)
            return;

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

                foreach (SteeringBehaviour b in patrolBehaviours)
                {
                    PatrolBehaviour patrol = b as PatrolBehaviour;
                    if (patrol != null) patrol.ForceRepath();
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
                return chaseBehaviours;

            case AIState.Investigate:
                return investigateBehaviours;

            case AIState.Attack:
            case AIState.Wait:
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

            case AIState.Attack:
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
            {
                StartCoroutine(PlayStingerThenGrowl());
                exclamationMark.SetActive(true);
            }
            else
            {
                exclamationMark.SetActive(false);
            }
        }
        else
        {
            exclamationMark.SetActive(false);
        }
    }

    private IEnumerator PlayStingerThenGrowl()
    {
        SoundEffectManager.PlayClip("Monster", "Stinger", 0.4f);

        yield return new WaitForSeconds(1f);

        SoundEffectManager.PlayClip("Monster", "Monster_growl", 0.5f);
    }

}
