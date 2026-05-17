using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    public static bool HasInteractableInRange { get; private set; }

    public static bool IsInteractBusy { get; private set; }
    public static bool IsCapturingInteractSounds { get; private set; }

    public static bool ShouldBlockInventoryUse
    {
        get
        {
            return HasInteractableInRange
                || IsInteractBusy
                || Time.frameCount == lastInteractFrame;
        }
    }

    private static InteractionDetector activeDetector;
    private static int lastInteractFrame = -1;

    private IInteractable interactableInRange = null;
    private readonly List<IInteractable> interactablesInRange = new();

    [Header("Interaction UI")]
    public GameObject interactionIcon;

    [Header("Fallback Lock")]
    [SerializeField] private float minimumInteractLock = 0.15f;

    private bool isOnCooldown = false;
    private Coroutine busyCoroutine;
    private float busyUntilTime = 0f;

    void Start()
    {
        SetInteractPrompt(false);

        HasInteractableInRange = false;
        IsInteractBusy = false;
        IsCapturingInteractSounds = false;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0f) return;
        if (!context.performed) return;
        if (isOnCooldown) return;
        if (interactableInRange == null) return;

        lastInteractFrame = Time.frameCount;

        IInteractable currentInteractable = interactableInRange;

        BeginInteractSoundCapture(this);

        try
        {
            currentInteractable.Interact();
        }
        finally
        {
            EndInteractSoundCapture();
        }

        LockInteractBusy(minimumInteractLock);

        if (!currentInteractable.CanInteract())
        {
            interactablesInRange.Remove(currentInteractable);
            RefreshInteractableInRange();
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

    private void LockInteractBusy(float duration)
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
            HasInteractableInRange = true;
            SetInteractPrompt(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
        {
            if (!interactablesInRange.Contains(interactable))
            {
                interactablesInRange.Add(interactable);
            }

            RefreshInteractableInRange();

            if (!isOnCooldown)
            {
                SetInteractPrompt(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable))
        {
            interactablesInRange.Remove(interactable);
            RefreshInteractableInRange();
        }
    }

    private void OnDisable()
    {
        interactablesInRange.Clear();
        interactableInRange = null;
        HasInteractableInRange = false;

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

        busyUntilTime = 0f;
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

    private void RefreshInteractableInRange()
    {
        for (int i = interactablesInRange.Count - 1; i >= 0; i--)
        {
            IInteractable interactable = interactablesInRange[i];

            if (interactable == null || !interactable.CanInteract())
            {
                interactablesInRange.RemoveAt(i);
            }
        }

        interactableInRange = interactablesInRange.Count > 0 ? interactablesInRange[interactablesInRange.Count - 1] : null;
        HasInteractableInRange = interactableInRange != null;

        SetInteractPrompt(HasInteractableInRange && !isOnCooldown);
    }
}
