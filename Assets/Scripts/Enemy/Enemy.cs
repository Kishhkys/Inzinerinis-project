using Unity.Cinemachine;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    private EnemyMover agentMover;

    private Vector2 pointerInput, movementInput;

    public Vector2 PointerInput { get => pointerInput; set => pointerInput = value; }
    public Vector2 MovementInput { get => movementInput; set => movementInput = value; }

    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float hitPointScatterRadius = 0.3f;
    private CinemachineImpulseSource impulseSource;


    private Vector2 startPosition;

    private void Awake()
    {
        agentMover = GetComponent<EnemyMover>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void Start()
    {
        startPosition = transform.position;
        
    }

    public void ResetToStart()
    {
        transform.position = startPosition;
    }

    private void Update()
    {

        agentMover.MovementInput = MovementInput;

    }

    public void PerformAttack()
    {

        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, attackRange, LayerMask.GetMask("Player"));
        if (playerCollider != null)
        {
            PlayerHealth playerHealth = playerCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Vector2 hitPoint = (Vector2)playerCollider.transform.position + Random.insideUnitCircle * hitPointScatterRadius;
                CameraShakeManager.instance.CameraShake(impulseSource);
                playerHealth.UpdateHealth(-attackDamage, hitPoint);
                InteractionDialogueEvents.PlayerDamagedBy(gameObject);
            }
        }
    }

}
