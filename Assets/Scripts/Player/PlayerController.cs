using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private float moveSpeed = 1.0f;

    private PlayerControls playerControls;
    private Vector2 movement;
    private Rigidbody2D rb;
    private bool playingFootsteps = false;
    [SerializeField] private float footstepSpeed = 0.3f;
    [SerializeField] private float footstepVolume = 0.5f;

    private void Awake()
    {
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        playerControls.Enable();
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
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        rb.MovePosition(rb.position + movement * (moveSpeed * Time.fixedDeltaTime));
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
