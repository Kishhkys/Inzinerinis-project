using UnityEngine;

public class Enemy : MonoBehaviour
{

    private EnemyMover agentMover;

    private Vector2 pointerInput, movementInput;

    public Vector2 PointerInput { get => pointerInput; set => pointerInput = value; }
    public Vector2 MovementInput { get => movementInput; set => movementInput = value; }

    private void Update()
    {

        agentMover.MovementInput = MovementInput;

    }

    public void PerformAttack()
    {

    }

    private void Awake()
    {
   
        agentMover = GetComponent<EnemyMover>();
    }

}
