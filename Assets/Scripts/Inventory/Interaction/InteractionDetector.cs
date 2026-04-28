using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    //private IInteractable interactableInRange = null;
    //public GameObject interactionIcon;

    //void Start()
    //{
    //    interactionIcon.SetActive(false);
    //}

    //public void OnInteract(InputAction.CallbackContext context)
    //{
    //    //if (context.performed)
    //    //{
    //    //    interactableInRange?.Interact();
    //    //    if (!interactableInRange.CanInteract())
    //    //    {
    //    //        interactionIcon?.SetActive(false);
    //    //    }
    //    //}

    //    if (!context.performed) return;
    //    if (interactableInRange == null) return;

    //    IInteractable currentInteractable = interactableInRange;
    //    currentInteractable.Interact();

    //    if (!currentInteractable.CanInteract())
    //    {
    //        interactableInRange = null;
    //        interactionIcon?.SetActive(false);
    //    }
    //}

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if(collision.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
    //    {
    //        interactableInRange = interactable;
    //        interactionIcon.SetActive(true);
    //    }
    //}

    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange)
    //    {
    //        interactableInRange = null;
    //        interactionIcon.SetActive(false);
    //    }
    //}

    private IInteractable interactableInRange = null;
    public GameObject interactionIcon;

    [Header("Interaction Cooldown")]
    public float interactCooldown = 1f;
    private bool isOnCooldown = false;

    void Start()
    {
        interactionIcon.SetActive(false);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0f) return;
        if (!context.performed) return;
        if (isOnCooldown) return;
        if (interactableInRange == null) return;

        IInteractable currentInteractable = interactableInRange;
        currentInteractable.Interact();

        StartCoroutine(InteractionCooldown());

        if (!currentInteractable.CanInteract())
        {
            interactableInRange = null;
            interactionIcon?.SetActive(false);
        }
    }

    private IEnumerator InteractionCooldown()
    {
        isOnCooldown = true;
        interactionIcon?.SetActive(false);

        yield return new WaitForSeconds(interactCooldown);

        isOnCooldown = false;

        if (interactableInRange != null && interactableInRange.CanInteract())
        {
            interactionIcon?.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
        {
            interactableInRange = interactable;

            if (!isOnCooldown)
            {
                interactionIcon.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange)
        {
            interactableInRange = null;
            interactionIcon.SetActive(false);
        }
    }
}
