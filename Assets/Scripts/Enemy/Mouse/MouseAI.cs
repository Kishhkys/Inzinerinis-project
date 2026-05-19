using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MouseAI : MonoBehaviour
{
    private enum AIState { Patrol, Attack }

    [SerializeField] private List<SteeringBehaviour> patrolBehaviours;
    [SerializeField] private AIData aiData;
    [SerializeField] private ContextSolver movementDirectionSolver;
    [SerializeField] private float attackDistance = 0.5f;
    [SerializeField] private float attackDelay = 1f;

    [Header("Unstuck")]
    [SerializeField] private float stuckCheckInterval = 0.5f;
    [SerializeField] private float stuckDistanceThreshold = 0.05f;
    [SerializeField] private float unstuckDuration = 0.3f;

    public UnityEvent OnAttackPressed;
    public UnityEvent<Vector2> OnMovementInput;
    public UnityEvent<Vector2> OnPointerInput;

    private AIState currentState = AIState.Patrol;
    private float lastAttackTime;

    private Vector2 lastPosition;
    private float stuckCheckTimer;
    private bool isUnstucking;
    private Coroutine unstuckCoroutine;
    private int playerLayerMask;

    private void Start()
    {
        
        lastPosition = transform.position;
        playerLayerMask = LayerMask.GetMask("Player");
    }

    private void Update()
    {
        HandleTransitions();

        if (!isUnstucking)
        {
            switch (currentState)
            {
                case AIState.Patrol:
                    Vector2 moveDir = movementDirectionSolver.GetDirectionToMove(patrolBehaviours, aiData);
                    OnMovementInput?.Invoke(moveDir);

                    if (aiData.currentPatrolTarget != null)
                        OnPointerInput?.Invoke(aiData.currentPatrolTarget.position);
                    break;

                case AIState.Attack:
                    OnMovementInput?.Invoke(Vector2.zero);

                    if (Time.time >= lastAttackTime + attackDelay)
                    {
                        OnAttackPressed?.Invoke();
                        lastAttackTime = Time.time;
                    }
                    break;
            }
        }

        CheckIfStuck();
    }

    private void HandleTransitions()
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(
            transform.position,
            attackDistance,
            playerLayerMask
        );

        if (playerCollider != null)
            currentState = AIState.Attack;
        else
            currentState = AIState.Patrol;
    }

    private void CheckIfStuck()
    {
        if (currentState != AIState.Patrol || isUnstucking)
        {
            lastPosition = transform.position;
            stuckCheckTimer = 0f;
            return;
        }

        stuckCheckTimer += Time.deltaTime;

        if (stuckCheckTimer < stuckCheckInterval)
            return;

        float movedDistance = Vector2.Distance(transform.position, lastPosition);

        if (movedDistance < stuckDistanceThreshold)
        {
            if (unstuckCoroutine != null)
                StopCoroutine(unstuckCoroutine);

            unstuckCoroutine = StartCoroutine(UnstuckRoutine());
        }

        lastPosition = transform.position;
        stuckCheckTimer = 0f;
    }

    private IEnumerator UnstuckRoutine()
    {
        isUnstucking = true;

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float timer = unstuckDuration;

        while (timer > 0f)
        {
            OnMovementInput?.Invoke(randomDir);
            timer -= Time.deltaTime;
            yield return null;
        }

        isUnstucking = false;
        unstuckCoroutine = null;
    }
}
