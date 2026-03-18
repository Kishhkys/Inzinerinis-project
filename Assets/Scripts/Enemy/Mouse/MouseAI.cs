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

    public UnityEvent OnAttackPressed;
    public UnityEvent<Vector2> OnMovementInput;
    public UnityEvent<Vector2> OnPointerInput;

    private AIState currentState = AIState.Patrol;
    private float lastAttackTime;

    private void Update()
    {
        HandleTransitions();

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

    private void HandleTransitions()
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, attackDistance, LayerMask.GetMask("Player"));

        if (playerCollider != null)
            currentState = AIState.Attack;
        else
            currentState = AIState.Patrol;
    }
}