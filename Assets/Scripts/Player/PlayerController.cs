using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 1.5f;

    [Header("Audio")]
    public float footstepSpeed = 0.5f;
    [SerializeField] private float footstepVolume = 0.5f;

    [Header("Hide")]
    [SerializeField] private LayerMask wallLayerMask;
    [SerializeField] private float hideDelay = 3f;
    [SerializeField] private float wallCheckDistance = 0.1f;
    [SerializeField] private float hiddenAlpha = 0.45f;
    [SerializeField] private float movementThreshold = 0.05f;
    [SerializeField] private float detectionMemory = 0.2f;
    [SerializeField] private float damageHideLockDuration = 1f;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private SpriteRenderer[] spriteRenderers;
    private Vector2 moveInput;
    private Animator animator;
    private bool playingFootsteps = false;
    private float stillNearWallTimer = 0f;
    private bool isHidden = false;
    private float hideBlockedUntil = 0f;
    private float lastDetectedTime = float.NegativeInfinity;
    private Color[] originalSpriteColors;
    private readonly RaycastHit2D[] wallHits = new RaycastHit2D[4];

    public bool IsHidden => isHidden;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalSpriteColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalSpriteColors[i] = spriteRenderers[i].color;
        }
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

        UpdateHideState();

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

    private void UpdateHideState()
    {
        bool isMoving = rb.linearVelocity.sqrMagnitude > movementThreshold * movementThreshold;
        bool isNearWall = IsNearWall();
        bool recentlyDetected = Time.time - lastDetectedTime <= detectionMemory;
        bool hideBlocked = Time.time < hideBlockedUntil;

        if (isMoving || !isNearWall || recentlyDetected || hideBlocked)
        {
            stillNearWallTimer = 0f;
            SetHidden(false);
            return;
        }

        stillNearWallTimer += Time.deltaTime;

        if (stillNearWallTimer >= hideDelay)
        {
            SetHidden(true);
        }
    }

    public void NotifyDetected()
    {
        lastDetectedTime = Time.time;
    }

    public void NotifyDamaged()
    {
        lastDetectedTime = Time.time;
        hideBlockedUntil = Time.time + damageHideLockDuration;
        stillNearWallTimer = 0f;
        SetHidden(false);
    }

    private bool IsNearWall()
    {
        if (playerCollider == null || wallLayerMask == 0)
        {
            return false;
        }

        ContactFilter2D filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = wallLayerMask,
            useTriggers = false
        };

        int hitCount = 0;
        hitCount += playerCollider.Cast(Vector2.up, filter, wallHits, wallCheckDistance);
        hitCount += playerCollider.Cast(Vector2.down, filter, wallHits, wallCheckDistance);
        hitCount += playerCollider.Cast(Vector2.left, filter, wallHits, wallCheckDistance);
        hitCount += playerCollider.Cast(Vector2.right, filter, wallHits, wallCheckDistance);

        return hitCount > 0;
    }

    private void SetHidden(bool hidden)
    {
        if (isHidden == hidden)
        {
            return;
        }

        isHidden = hidden;
        ApplySpriteTransparency(hidden);
    }

    private void ApplySpriteTransparency(bool hidden)
    {
        if (spriteRenderers == null || originalSpriteColors == null)
        {
            return;
        }

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            Color color = originalSpriteColors[i];
            color.a = hidden ? originalSpriteColors[i].a * hiddenAlpha : originalSpriteColors[i].a;
            spriteRenderers[i].color = color;
        }
    }

    private void OnDisable()
    {
        stillNearWallTimer = 0f;
        SetHidden(false);
        StopFootsteps();
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

    public void Teleport(Vector3 newPosition)
    {
        rb.linearVelocity = Vector2.zero;
        transform.position = newPosition;
    }

}
