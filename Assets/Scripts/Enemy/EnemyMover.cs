using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class EnemyMover : MonoBehaviour, ITeleportable
{
    public enum FootstepSound
    {
        None,
        MonsterFootsteps,
        MouseSounds
    }

    private Rigidbody2D rb2d;
    private Animator animator;
    private AudioSource enemyAudioSource;
    private static readonly int IsWalkingHash = Animator.StringToHash("isWalking");
    private static readonly int InputXHash = Animator.StringToHash("InputX");
    private static readonly int InputYHash = Animator.StringToHash("InputY");
    private static readonly int LastInputXHash = Animator.StringToHash("LastInputX");
    private static readonly int LastInputYHash = Animator.StringToHash("LastInputY");

    [Header("Movement")]
    [SerializeField] private float maxSpeed = 2f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deacceleration = 100f;
    [SerializeField] private float currentSpeed = 0f;

    private Vector2 oldMovementInput;
    public Vector2 MovementInput { get; set; }

    [Header("Footsteps")]
    [SerializeField] private FootstepSound footstepSound = FootstepSound.None;
    [SerializeField] private float footstepSpeed = 0.5f;
    [SerializeField] private float footstepVolume = 0.5f;

    [Header("2D Distance Audio")]
    [SerializeField] private Transform listenerTarget;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 8f;

    private bool playingFootsteps = false;
    private float teleportBlockedUntil = 0f;

    private void Awake()
    {
        TryGetComponent(out rb2d);
        TryGetComponent(out animator);

        if (!TryGetComponent(out enemyAudioSource))
        {
            enemyAudioSource = gameObject.AddComponent<AudioSource>();
        }

        enemyAudioSource.playOnAwake = false;
        enemyAudioSource.loop = false;

        enemyAudioSource.spatialBlend = 0f;
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

            if (animator != null)
            {
                animator.SetBool(IsWalkingHash, true);
                animator.SetFloat(InputXHash, MovementInput.x);
                animator.SetFloat(InputYHash, MovementInput.y);
            }
        }
        else
        {
            currentSpeed -= deacceleration * maxSpeed * Time.deltaTime;

            if (animator != null)
            {
                animator.SetBool(IsWalkingHash, false);
                animator.SetFloat(LastInputXHash, oldMovementInput.x);
                animator.SetFloat(LastInputYHash, oldMovementInput.y);
            }
        }

        currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);
        rb2d.Velocity = oldMovementInput * currentSpeed;

        if (MovementInput.magnitude > 0 && !playingFootsteps)
        {
            StartFootsteps();
        }
        else if (MovementInput.magnitude == 0)
        {
            StopFootsteps();
        }

        UpdateLoopVolume();
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
        if (footstepSound == FootstepSound.None)
        {
            return;
        }

        playingFootsteps = true;

        if (footstepSound == FootstepSound.MouseSounds)
        {
            StartMouseSoundsLoop();
        }
        else
        {
            InvokeRepeating(nameof(PlayFootstep), 0.05f, footstepSpeed);
        }
    }

    private void StopFootsteps()
    {
        playingFootsteps = false;

        CancelInvoke(nameof(PlayFootstep));

        if (enemyAudioSource != null)
        {
            enemyAudioSource.Stop();
            enemyAudioSource.loop = false;
            enemyAudioSource.clip = null;
        }
    }

    private void StartMouseSoundsLoop()
    {
        if (enemyAudioSource == null)
        {
            Debug.LogWarning("Enemy AudioSource missing on: " + gameObject.name);
            return;
        }

        AudioClip clip = SoundEffectManager.GetRandomClip("MouseSounds");

        if (clip == null)
        {
            return;
        }

        enemyAudioSource.clip = clip;
        enemyAudioSource.loop = true;
        enemyAudioSource.volume = GetFinalFootstepVolume();
        enemyAudioSource.Play();
    }

    private void PlayFootstep()
    {
        if (footstepSound == FootstepSound.None)
        {
            return;
        }

        if (enemyAudioSource == null)
        {
            Debug.LogWarning("Enemy AudioSource missing on: " + gameObject.name);
            return;
        }

        AudioClip clip = SoundEffectManager.GetRandomClip(footstepSound.ToString());

        if (clip == null)
        {
            Debug.LogWarning("Footstep clip not found: " + footstepSound);
            return;
        }

        float finalVolume = GetFinalFootstepVolume();

        if (finalVolume <= 0f)
        {
            return;
        }

        enemyAudioSource.PlayOneShot(clip, finalVolume);
    }

    private void UpdateLoopVolume()
    {
        if (enemyAudioSource == null)
        {
            return;
        }

        if (enemyAudioSource.loop && enemyAudioSource.isPlaying)
        {
            enemyAudioSource.volume = GetFinalFootstepVolume();
        }
    }

    private float GetFinalFootstepVolume()
    {
        float distanceMultiplier = GetDistanceVolumeMultiplier();
        return footstepVolume * distanceMultiplier * SoundEffectManager.GetVolume();
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

    private void OnDisable()
    {
        StopFootsteps();
    }
}
