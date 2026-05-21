using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour, ITeleportable
{
    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");
    private static readonly int InputXHash = Animator.StringToHash("inputX");
    private static readonly int InputYHash = Animator.StringToHash("inputY");
    private static readonly int LastInputXHash = Animator.StringToHash("LastInputX");
    private static readonly int LastInputYHash = Animator.StringToHash("LastInputY");

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 1.5f;

    [Header("Audio")]
    [SerializeField] private float footstepSpeed = 0.5f;
    [SerializeField] private float runFootstepSpeed = 0.3f;
    [SerializeField] private float footstepVolume = 0.5f;

    [Header("Hide")]
    [SerializeField] private LayerMask wallLayerMask;
    [SerializeField] private float hideDelay = 3f;
    [SerializeField] private float wallCheckDistance = 0.1f;
    [SerializeField] private float hiddenAlpha = 0.45f;
    [SerializeField] private float movementThreshold = 0.05f;
    [SerializeField] private float detectionMemory = 0.2f;
    [SerializeField] private float damageHideLockDuration = 1f;

    [Header("Flashlight")]
    [SerializeField] private bool flashlightStartsOn = false;
    [SerializeField] private float flashlightIntensity = 1.8f;
    [SerializeField] private float flashlightRange = 4.5f;
    [SerializeField] private float flashlightOuterAngle = 55f;
    [SerializeField] private float flashlightInnerAngle = 35f;
    [SerializeField] private Color flashlightColor = new(1f, 0.92f, 0.72f, 1f);
    [SerializeField] private float flashlightForwardOffset = 0.28f;
    [SerializeField] private float flashlightRightHandOffset = 0.18f;
    [SerializeField] private Vector2 flashlightBaseOffset = new(0f, -0.05f);
    [SerializeField] private Sprite flashlightOffHandSprite;
    [SerializeField] private Sprite flashlightOnHandSprite;
    [SerializeField] private float flashlightSpriteDirectionOffset = 135f;
    [SerializeField] private int flashlightSpriteSortingOrder = 3;
    [SerializeField] private Vector2 flashlightRightFacingSpritePosition = new(0.08f, -0.2f);

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private SpriteRenderer[] spriteRenderers;
    private Vector2 moveInput;
    private Animator animator;

    private bool playingFootsteps = false;
    private bool wasRunning = false;

    private float stillNearWallTimer = 0f;
    private bool isHidden = false;
    private float hideBlockedUntil = 0f;
    private float lastDetectedTime = float.NegativeInfinity;

    private Color[] originalSpriteColors;
    private readonly RaycastHit2D[] wallHits = new RaycastHit2D[4];
    private readonly ContactFilter2D wallContactFilter = new()
    {
        useLayerMask = true,
        useTriggers = false
    };

    private Light2D flashlight;
    private Transform flashlightTransform;
    private Transform flashlightSpriteTransform;
    private SpriteRenderer flashlightSpriteRenderer;
    private Sprite loadedFlashlightOffSprite;
    private Sprite loadedFlashlightOnSprite;
    private bool flashlightOn;
    private Vector2 lastFacingDirection = Vector2.down;

    [SerializeField] private float teleportBlockedUntil = 2f;

    private const float MinSqrMagnitude = 0.01f;
    private const float FlashlightAngleOffset = 90f;

    private bool movementBlocked = false;
    private bool isLocked = false;

    public bool IsHidden => isHidden;

    void Start()
    {
        TryGetComponent(out rb);
        TryGetComponent(out playerCollider);
        TryGetComponent(out animator);

        CreateFlashlight();

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalSpriteColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalSpriteColors[i] = spriteRenderers[i].color;
        }

        if (ActionPromptBox.Instance != null)
        {
            ActionPromptBox.Instance.SetHidePrompt(false);
        }

        SetFlashlight(flashlightStartsOn);
    }

    void Update()
    {
        if (rb == null)
        {
            return;
        }

        if (movementBlocked)
        {
            rb.velocity = Vector2.zero;
            UpdateHidePrompt();
            return;
        }

        Keyboard keyboard = Keyboard.current;
        bool isRunning = IsRunPressed(keyboard);

        HandleFlashlightInput(keyboard);
        ApplyMovement(isRunning);
        UpdateHideState();
        UpdateHidePrompt();
        UpdateFlashlightTransform();
        UpdateAnimatorMovement();
        UpdateFootsteps(isRunning);

        wasRunning = isRunning;
    }

    private bool IsRunPressed(Keyboard keyboard)
    {
        return keyboard != null && keyboard.shiftKey.isPressed;
    }

    private void HandleFlashlightInput(Keyboard keyboard)
    {
        if (keyboard != null && keyboard.fKey.wasPressedThisFrame)
        {
            ToggleFlashlight();
        }
    }

    private void ApplyMovement(bool isRunning)
    {
        float speed = isRunning ? runSpeed : moveSpeed;
        rb.velocity = moveInput * speed;
    }

    private void UpdateAnimatorMovement()
    {
        if (animator != null)
        {
            animator.SetBool(IsMovingHash, rb.velocity.magnitude > 0);
        }
    }

    private void UpdateFootsteps(bool isRunning)
    {
        if (rb.velocity.magnitude > 0)
        {
            if (!playingFootsteps || isRunning != wasRunning)
            {
                StartFootsteps(isRunning);
            }
        }
        else
        {
            StopFootsteps();
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (isLocked || movementBlocked)
        {
            moveInput = Vector2.zero;

            if (animator != null)
            {
                animator.SetBool(IsMovingHash, false);
                animator.SetFloat(InputXHash, 0f);
                animator.SetFloat(InputYHash, 0f);
            }

            return;
        }

        if (context.canceled)
        {
            if (animator != null)
            {
                animator.SetBool(IsMovingHash, false);
                animator.SetFloat(LastInputXHash, moveInput.x);
                animator.SetFloat(LastInputYHash, moveInput.y);
            }
        }

        moveInput = context.ReadValue<Vector2>();

        if (animator != null)
        {
            animator.SetFloat(InputXHash, moveInput.x);
            animator.SetFloat(InputYHash, moveInput.y);
        }

        if (moveInput.sqrMagnitude > MinSqrMagnitude)
        {
            lastFacingDirection = moveInput.normalized;
            UpdateFlashlightTransform();
        }
    }

    private void UpdateHidePrompt()
    {
        if (ActionPromptBox.Instance == null || rb == null)
        {
            return;
        }

        bool isMoving = rb.velocity.sqrMagnitude > movementThreshold * movementThreshold;
        bool touchingWall = IsTouchingWall();
        bool recentlyDetected = Time.time - lastDetectedTime <= detectionMemory;
        bool hideBlocked = Time.time < hideBlockedUntil;

        bool shouldShowHide =
            !isMoving &&
            touchingWall &&
            !recentlyDetected &&
            !hideBlocked &&
            !isHidden &&
            !InteractionDetector.IsInteractBusy;

        ActionPromptBox.Instance.SetHidePrompt(shouldShowHide);
    }


    private void UpdateHideState()
    {
        if (rb == null)
        {
            return;
        }

        bool isMoving = rb.velocity.sqrMagnitude > movementThreshold * movementThreshold;
        bool canHideWithCtrl = IsNearWall();
        bool recentlyDetected = Time.time - lastDetectedTime <= detectionMemory;
        bool hideBlocked = Time.time < hideBlockedUntil;

        if (isMoving || !canHideWithCtrl || recentlyDetected || hideBlocked)
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

    private bool IsTouchingWall()
    {
        if (playerCollider == null || wallLayerMask == 0)
        {
            return false;
        }

        ContactFilter2D filter = wallContactFilter;
        filter.layerMask = wallLayerMask;

        int hitCount = 0;
        hitCount += playerCollider.Cast(Vector2.up, filter, wallHits, wallCheckDistance);
        hitCount += playerCollider.Cast(Vector2.down, filter, wallHits, wallCheckDistance);
        hitCount += playerCollider.Cast(Vector2.left, filter, wallHits, wallCheckDistance);
        hitCount += playerCollider.Cast(Vector2.right, filter, wallHits, wallCheckDistance);

        return hitCount > 0;
    }

    private bool IsNearWall()
    {
        Keyboard keyboard = Keyboard.current;

        return IsTouchingWall()
            && keyboard != null
            && keyboard.ctrlKey.isPressed;
    }

    private void SetHidden(bool hidden)
    {
        if (isHidden == hidden)
        {
            return;
        }

        isHidden = hidden;
        ApplySpriteTransparency(hidden);

        if (hidden && ActionPromptBox.Instance != null)
        {
            ActionPromptBox.Instance.SetHidePrompt(false);
        }
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

    private void CreateFlashlight()
    {
        GameObject flashlightObject = new("Player Flashlight");
        flashlightObject.transform.SetParent(transform, false);

        flashlightTransform = flashlightObject.transform;
        flashlight = flashlightObject.AddComponent<Light2D>();

        flashlight.lightType = Light2D.LightType.Point;
        flashlight.intensity = flashlightIntensity;
        flashlight.color = flashlightColor;
        flashlight.pointLightOuterRadius = flashlightRange;
        flashlight.pointLightInnerRadius = 0f;
        flashlight.pointLightOuterAngle = flashlightOuterAngle;
        flashlight.pointLightInnerAngle = flashlightInnerAngle;
        flashlight.falloffIntensity = 0.65f;
        flashlight.shadowsEnabled = true;
        flashlight.shadowIntensity = 0.55f;
        flashlight.shadowSoftness = 0.35f;

        CreateFlashlightSprite();
        UpdateFlashlightTransform();
    }

    private void CreateFlashlightSprite()
    {
        loadedFlashlightOffSprite = flashlightOffHandSprite != null
            ? flashlightOffHandSprite
            : Resources.Load<Sprite>("PlayerFlashlightOff");

        loadedFlashlightOnSprite = flashlightOnHandSprite != null
            ? flashlightOnHandSprite
            : Resources.Load<Sprite>("PlayerFlashlightOn");

        // If both sprites are missing, skip creating the sprite object but keep the Light2D functional.
        if (loadedFlashlightOffSprite == null && loadedFlashlightOnSprite == null)
        {
            if (Debug.isDebugBuild)
            {
                Debug.LogWarning("Player flashlight sprites not found in inspector or Resources.");
            }
            return;
        }

        // If only one sprite is present, reuse it for both on/off states to avoid missing visuals.
        if (loadedFlashlightOffSprite == null)
        {
            loadedFlashlightOffSprite = loadedFlashlightOnSprite;
        }

        if (loadedFlashlightOnSprite == null)
        {
            loadedFlashlightOnSprite = loadedFlashlightOffSprite;
        }

        GameObject spriteObject = new("Player Flashlight Sprite");
        spriteObject.transform.SetParent(transform, false);

        flashlightSpriteTransform = spriteObject.transform;
        flashlightSpriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
        flashlightSpriteRenderer.sprite = loadedFlashlightOffSprite;
        flashlightSpriteRenderer.sortingLayerName = "Player";
        flashlightSpriteRenderer.sortingOrder = flashlightSpriteSortingOrder;
    }

    private void ToggleFlashlight()
    {
        SetFlashlight(!flashlightOn);
    }

    private void SetFlashlight(bool enabled)
    {
        flashlightOn = enabled;

        if (flashlight != null)
        {
            flashlight.enabled = flashlightOn;
        }

        if (flashlightSpriteRenderer != null)
        {
            flashlightSpriteRenderer.sprite = flashlightOn ? loadedFlashlightOnSprite : loadedFlashlightOffSprite;
            flashlightSpriteRenderer.enabled = ShouldShowFlashlightSprite();
        }
    }

    private void UpdateFlashlightTransform()
    {
        if (flashlightTransform == null)
        {
            return;
        }

        Vector2 direction = lastFacingDirection.sqrMagnitude > MinSqrMagnitude
            ? lastFacingDirection.normalized
            : Vector2.down;

        Vector2 rightHandDirection = new(direction.y, -direction.x);

        Vector2 offset =
            flashlightBaseOffset +
            direction * flashlightForwardOffset +
            rightHandDirection * flashlightRightHandOffset;

        float directionAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        flashlightTransform.SetLocalPositionAndRotation(
            offset,
            Quaternion.Euler(0f, 0f, directionAngle - FlashlightAngleOffset)
        );

        if (flashlightSpriteTransform != null)
        {
            Vector2 spriteOffset = GetFlashlightSpritePosition(direction, offset);
            flashlightSpriteTransform.SetLocalPositionAndRotation(
                spriteOffset,
                Quaternion.Euler(0f, 0f, directionAngle + flashlightSpriteDirectionOffset)
            );

            if (flashlightSpriteRenderer != null)
            {
                flashlightSpriteRenderer.enabled = ShouldShowFlashlightSprite();
            }
        }
    }

    private Vector2 GetFlashlightSpritePosition(Vector2 direction, Vector2 fallbackPosition)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y) && direction.x > 0f)
        {
            return flashlightRightFacingSpritePosition;
        }

        return fallbackPosition;
    }

    private bool ShouldShowFlashlightSprite()
    {
        Vector2 direction = lastFacingDirection.sqrMagnitude > MinSqrMagnitude
            ? lastFacingDirection.normalized
            : Vector2.down;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            return direction.x > 0f;
        }

        return direction.y <= 0f;
    }

    private void OnDisable()
    {
        stillNearWallTimer = 0f;
        SetHidden(false);
        StopFootsteps();

        if (ActionPromptBox.Instance != null)
        {
            ActionPromptBox.Instance.SetHidePrompt(false);
        }
    }

    public void StopFootsteps()
    {
        playingFootsteps = false;
        CancelInvoke(nameof(PlayFootstep));
    }

    private void StartFootsteps(bool isRunning)
    {
        CancelInvoke(nameof(PlayFootstep));

        playingFootsteps = true;

        float interval = isRunning ? runFootstepSpeed : footstepSpeed;
        InvokeRepeating(nameof(PlayFootstep), 0f, interval);
    }

    private void PlayFootstep()
    {
        SoundEffectManager.PlayRandomClip("PlayerFootsteps", footstepVolume);
    }

    public void Teleport(Vector3 newPosition)
    {
        Teleport(newPosition, 0.2f);
    }

    public bool CanTeleport()
    {
        return Time.time >= teleportBlockedUntil;
    }

    public void Teleport(Vector3 newPosition, float blockDuration = 0.2f)
    {
        StartCoroutine(TeleportRoutine(newPosition, blockDuration));
    }

    private IEnumerator TeleportRoutine(Vector3 newPosition, float blockDuration)
    {
        if (rb == null)
        {
            transform.position = newPosition;
            teleportBlockedUntil = Time.time + blockDuration;
            yield break;
        }

        movementBlocked = true;

        rb.velocity = Vector2.zero;
        moveInput = Vector2.zero;

        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }

        SoundEffectManager.PlayClip("Door", "Teleport", 1f);

        rb.position = newPosition;
        transform.position = newPosition;

        yield return new WaitForFixedUpdate();

        rb.velocity = Vector2.zero;

        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }

        teleportBlockedUntil = Time.time + blockDuration;

        yield return new WaitForSeconds(blockDuration);

        movementBlocked = false;
    }

    public void LockMovementFor(float duration)
    {
        StopCoroutine(nameof(MovementLockRoutine));
        StartCoroutine(MovementLockRoutine(duration));
    }

    public void LockMovementFor(float duration, Vector3 faceTargetPosition)
    {
        FaceTowards(faceTargetPosition);

        StopCoroutine(nameof(MovementLockRoutine));
        StartCoroutine(MovementLockRoutine(duration));
    }

    public void FaceTowards(Vector3 targetPosition)
    {
        Vector2 direction = targetPosition - transform.position;

        if (direction.sqrMagnitude < MinSqrMagnitude)
        {
            return;
        }

        direction.Normalize();

        lastFacingDirection = direction;
        UpdateFlashlightTransform();

        if (animator != null)
        {
            animator.SetFloat(InputXHash, direction.x);
            animator.SetFloat(InputYHash, direction.y);
            animator.SetFloat(LastInputXHash, direction.x);
            animator.SetFloat(LastInputYHash, direction.y);
        }
    }

    private IEnumerator MovementLockRoutine(float duration)
    {
        isLocked = true;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        moveInput = Vector2.zero;

        if (animator != null)
        {
            animator.SetBool(IsMovingHash, false);
        }

        StopFootsteps();

        yield return new WaitForSeconds(duration);

        isLocked = false;
    }
}
