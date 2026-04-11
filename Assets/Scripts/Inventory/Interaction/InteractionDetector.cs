using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null;
    public GameObject interactionIcon;

    void Start()
    {
        interactionIcon.SetActive(false);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        //if (context.performed)
        //{
        //    interactableInRange?.Interact();
        //    if (!interactableInRange.CanInteract())
        //    {
        //        interactionIcon?.SetActive(false);
        //    }
        //}

        if (!context.performed) return;
        if (interactableInRange == null) return;

        IInteractable currentInteractable = interactableInRange;
        currentInteractable.Interact();

        if (!currentInteractable.CanInteract())
        {
            interactableInRange = null;
            interactionIcon?.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
        {
            interactableInRange = interactable;
            interactionIcon.SetActive(true);
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
