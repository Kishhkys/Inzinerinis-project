using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3.0f;

    private PlayerControls playerControls;
    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator animator;
    private bool playingFootsteps = false;
    [SerializeField] private float footstepSpeed = 0.3f;
    [SerializeField] private float footstepVolume = 0.5f;

    private void Awake()
    {
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }
    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Update()
    {
        PlayerInput();
        if (movement.magnitude > 0 && !playingFootsteps)
        {
            StartFootsteps();
        }
        else if (movement.magnitude == 0)
        {
            StopFootsteps();
        }
    }

    private void PlayerInput()
    {
        movement = playerControls.Movement.Move.ReadValue<Vector2>();

        bool isMoving = movement.sqrMagnitude > 0;
        animator.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
            {
                animator.SetFloat("horizontal", movement.x);
                animator.SetFloat("vertical", 0f);
            }
            else
            {
                animator.SetFloat("horizontal", 0f);
                animator.SetFloat("vertical", movement.y);
            }
        }

    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * movement);
    }

    private void StopFootsteps()
    {
        playingFootsteps = false;
        CancelInvoke(nameof(PlayFootstep));
    }

    private void StartFootsteps()
    {
        playingFootsteps = true;
        InvokeRepeating(nameof(PlayFootstep), 0f, footstepSpeed);
    }

    private void PlayFootstep()
    {
        SoundEffectManager.Play("PlayerFootsteps", footstepVolume);
    }
}
