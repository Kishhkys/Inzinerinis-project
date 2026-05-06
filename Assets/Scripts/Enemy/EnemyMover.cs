using UnityEngine;

public class EnemyMover : MonoBehaviour, ITeleportable
{
    public enum FootstepSound
    {
        None,
        MonsterFootsteps,
        MouseFootsteps
    }

    private Rigidbody2D rb2d;
    private Animator animator;
    private AudioSource enemyAudioSource;

    [SerializeField] private float maxSpeed = 2f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deacceleration = 100f;
    [SerializeField] private float currentSpeed = 0f;

    private Vector2 oldMovementInput;
    public Vector2 MovementInput { get; set; }

    private bool playingFootsteps = false;

    [Header("Footsteps")]
    [SerializeField] private float footstepSpeed = 0.5f;
    [SerializeField] private float footstepVolume = 0.5f;
    [SerializeField] private FootstepSound footstepSound;

    [Header("2D Distance Audio")]
    [SerializeField] private Transform listenerTarget; // ?ia ?d?k Player
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 8f;

    private float teleportBlockedUntil = 0f;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        enemyAudioSource = GetComponent<AudioSource>();

        if (enemyAudioSource == null)
        {
            enemyAudioSource = gameObject.AddComponent<AudioSource>();
        }

        enemyAudioSource.playOnAwake = false;
        enemyAudioSource.spatialBlend = 0f; // 2D garsas, volume valdysim patys
    }

    private void Start()
    {
        if (listenerTarget == null)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();

            if (player != null)
            {
                listenerTarget = player.transform;
            }
        }
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

        currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);
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

    public bool CanTeleport()
    {
        return Time.time >= teleportBlockedUntil;
    }

    public void Teleport(Vector3 newPosition, float blockDuration = 0.2f)
    {
        MovementInput = Vector2.zero;
        oldMovementInput = Vector2.zero;
        currentSpeed = 0f;
        rb2d.linearVelocity = Vector2.zero;

        transform.position = newPosition;
        teleportBlockedUntil = Time.time + blockDuration;
    }

    private void StartFootsteps()
    {
        playingFootsteps = true;
        InvokeRepeating(nameof(PlayFootstep), 0.05f, footstepSpeed);
    }

    private void StopFootsteps()
    {
        playingFootsteps = false;
        CancelInvoke(nameof(PlayFootstep));
    }

    private void PlayFootstep()
    {
        if (footstepSound == FootstepSound.None) return;

        AudioClip clip = SoundEffectManager.GetRandomClip(footstepSound.ToString());

        if (clip == null)
        {
            Debug.LogWarning("Footstep clip not found: " + footstepSound);
            return;
        }

        float distanceMultiplier = GetDistanceVolumeMultiplier();

        if (distanceMultiplier <= 0f)
        {
            return;
        }

        float finalVolume = footstepVolume * distanceMultiplier * SoundEffectManager.GetVolume();

        enemyAudioSource.PlayOneShot(clip, finalVolume);
    }

    private float GetDistanceVolumeMultiplier()
    {
        if (listenerTarget == null)
        {
            return 1f;
        }

        float distance = Vector2.Distance(transform.position, listenerTarget.position);

        if (distance <= minDistance)
        {
            return 1f;
        }

        if (distance >= maxDistance)
        {
            return 0f;
        }

        return 1f - ((distance - minDistance) / (maxDistance - minDistance));
    }
}