using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private bool playingFootsteps = false;
    public float footstepSpeed = 0.5f;
    [SerializeField] private float footstepVolume = 0.5f;
    [SerializeField] private float runSpeed = 1.5f;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (PauseController.isGamePaused)
        //{
        //    rb.linearVelocity = Vector2.zero;
        //    animator.SetBool("isWalking", false);
        //    StopFootsteps();
        //    return;
        //}
        
        
        if(Keyboard.current.shiftKey.isPressed)
        {
            rb.linearVelocity = moveInput * runSpeed;
        }
        else
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }
        animator.SetBool("isMoving", rb.linearVelocity.magnitude > 0);

        if (rb.linearVelocity.magnitude > 0 && !playingFootsteps)
        {
            StartFootsteps();
        }
        else if (rb.linearVelocity.magnitude == 0)
        {
            StopFootsteps();
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        //if (PauseController.isGamePaused) return;

        if (context.canceled)
        {
            animator.SetBool("isMoving", false);
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);

        }

        moveInput = context.ReadValue<Vector2>();
        animator.SetFloat("inputX", moveInput.x);
        animator.SetFloat("inputY", moveInput.y);
    }


    public void StopFootsteps()
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
        SoundEffectManager.PlayRandomClip("PlayerFootsteps", footstepVolume);
    }

}
