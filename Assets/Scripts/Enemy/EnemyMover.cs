using UnityEngine;

public class EnemyMover : MonoBehaviour
{
    public enum FootstepSound
    {
        None,
        MonsterFootsteps,
        MouseFootsteps
    }

    
    private Rigidbody2D rb2d;

    [SerializeField]
    private float maxSpeed = 2, acceleration = 50, deacceleration = 100;
    [SerializeField]
    private float currentSpeed = 0;
    private Vector2 oldMovementInput;
    public Vector2 MovementInput { get; set; }

    bool playingFootsteps = false;
    [SerializeField] private float footstepSpeed = 5f;
    [SerializeField] private float footstepVolume = 0.05f;
    [SerializeField] private FootstepSound footstepSound;
    private Animator animator;

  
    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }


    private void FixedUpdate()
    {
        if (MovementInput.magnitude > 0 && currentSpeed >= 0)
        {
            oldMovementInput = MovementInput;
            currentSpeed += acceleration * maxSpeed * Time.deltaTime;

            animator.SetBool("isWalking", true);
            animator.SetFloat("InputX", MovementInput.x);
            animator.SetFloat("InputY", MovementInput.y);
        }
        else
        {
            currentSpeed -= deacceleration * maxSpeed * Time.deltaTime;
            animator.SetBool("isWalking", false);
            animator.SetFloat("LastInputX", oldMovementInput.x);
            animator.SetFloat("LastInputY", oldMovementInput.y);
        }
        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);
        rb2d.linearVelocity = oldMovementInput * currentSpeed;


        if (MovementInput.magnitude > 0 && !playingFootsteps)
        {
            StartFootsteps();
        }
        else if (MovementInput.magnitude == 0)
        {
            StopFootsteps();
        }
    }

    private void StopFootsteps()
    {
        playingFootsteps = false;
        CancelInvoke(nameof(PlayFootstep));
    }

    private void StartFootsteps()
    {
        playingFootsteps = true;
        InvokeRepeating(nameof(PlayFootstep), 0.05f, footstepSpeed);
    }

    private void PlayFootstep()
    {
        SoundEffectManager.Play(footstepSound.ToString(), footstepVolume);
    }
}
