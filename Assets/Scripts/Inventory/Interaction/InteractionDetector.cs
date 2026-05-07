using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    public static bool IsInteractBusy { get; private set; }
    public static bool IsCapturingInteractSounds { get; private set; }

    private static InteractionDetector activeDetector;

    private IInteractable interactableInRange = null;

    [Header("Interaction UI")]
    public GameObject interactionIcon;

    [Header("Fallback Cooldown")]
    [SerializeField] private float minimumInteractLock = 0.15f;

    private bool isOnCooldown = false;
    private Coroutine busyCoroutine;
    private float busyUntilTime = 0f;

    void Start()
    {
        SetInteractPrompt(false);
        IsInteractBusy = false;
        IsCapturingInteractSounds = false;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0f) return;
        if (!context.performed) return;
        if (isOnCooldown) return;
        if (interactableInRange == null) return;

        IInteractable currentInteractable = interactableInRange;

        BeginInteractSoundCapture(this);

        currentInteractable.Interact();

        EndInteractSoundCapture();

        // Jeigu interact nepaleido jokio PlayClip, vis tiek trumpai užrakinam,
        // kad hide tekstas nešokt? t? pat? frame.
        LockInteractBusy(minimumInteractLock);

        if (!currentInteractable.CanInteract())
        {
            interactableInRange = null;
            SetInteractPrompt(false);
        }
    }

    private static void BeginInteractSoundCapture(InteractionDetector detector)
    {
        activeDetector = detector;
        IsCapturingInteractSounds = true;
    }

    private static void EndInteractSoundCapture()
    {
        IsCapturingInteractSounds = false;
        activeDetector = null;
    }

    public static void RegisterInteractSoundDuration(float duration)
    {
        if (!IsCapturingInteractSounds) return;
        if (activeDetector == null) return;
        if (duration <= 0f) return;

        activeDetector.LockInteractBusy(duration);
    }

    public void LockInteractBusy(float duration)
    {
        float targetTime = Time.time + Mathf.Max(duration, minimumInteractLock);

        if (targetTime > busyUntilTime)
        {
            busyUntilTime = targetTime;
        }

        if (busyCoroutine == null)
        {
            busyCoroutine = StartCoroutine(InteractBusyRoutine());
        }
    }

    private IEnumerator InteractBusyRoutine()
    {
        isOnCooldown = true;
        IsInteractBusy = true;

        SetInteractPrompt(false);

        while (Time.time < busyUntilTime)
        {
            yield return null;
        }

        isOnCooldown = false;
        IsInteractBusy = false;
        busyCoroutine = null;
        busyUntilTime = 0f;

        if (interactableInRange != null && interactableInRange.CanInteract())
        {
            SetInteractPrompt(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
        {
            interactableInRange = interactable;

            if (!isOnCooldown)
            {
                SetInteractPrompt(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange)
        {
            interactableInRange = null;
            SetInteractPrompt(false);
        }
    }

    private void OnDisable()
    {
        interactableInRange = null;
        SetInteractPrompt(false);

        isOnCooldown = false;
        IsInteractBusy = false;
        IsCapturingInteractSounds = false;

        if (activeDetector == this)
        {
            activeDetector = null;
        }

        if (busyCoroutine != null)
        {
            StopCoroutine(busyCoroutine);
            busyCoroutine = null;
        }
    }

    private void SetInteractPrompt(bool active)
    {
        if (interactionIcon != null)
        {
            interactionIcon.SetActive(active);
        }

        if (ActionPromptBox.Instance != null)
        {
            ActionPromptBox.Instance.SetInteractPrompt(active);
        }
    }
}