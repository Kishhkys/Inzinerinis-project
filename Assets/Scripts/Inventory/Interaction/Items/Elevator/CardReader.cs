using System.Collections;
using UnityEngine;

public class CardReader : MonoBehaviour, IInteractable
{
    [Header("Elevator")]
    [SerializeField] private Elevator elevator;

    [Header("Required Item")]
    [SerializeField] private int requiredKeycardID;

    [Header("Settings")]
    [SerializeField] private bool removeKeycardAfterUse = false;

    private bool isAccepted = false;
    private bool isUsing = false;

    public bool CanInteract()
    {
        return !isAccepted && !isUsing;
    }

    public void Interact()
    {
        Debug.Log("CardReader interacted.");

        if (!CanInteract())
        {
            Debug.Log("CardReader cannot interact right now.");
            return;
        }

        if (elevator == null)
        {
            Debug.LogWarning("CardReader: Elevator is not assigned in Inspector.");
            return;
        }

        if (!elevator.IsRepaired())
        {
            Debug.Log("The card reader has no power. Restore the elevator panel first.");

            float duration = SoundEffectManager.PlayClip(
                "Elevator",
                "Elevator_creak",
                0.5f,
                true,
                0f,
                transform.position
            );

            StartCoroutine(BlockInteractionFor(duration));
            return;
        }

        InventoryController inventory = FindFirstObjectByType<InventoryController>();

        if (inventory == null)
        {
            Debug.LogWarning("CardReader: InventoryController not found.");
            return;
        }

        Item selectedItem = inventory.GetSelectedItem();

        if (selectedItem == null)
        {
            Debug.Log("CardReader: No selected item. A keycard is required.");

            float duration = SoundEffectManager.PlayClip(
                "Door",
                "Door_locked",
                0.5f,
                true,
                0f,
                transform.position
            );

            StartCoroutine(BlockInteractionFor(duration));
            return;
        }

        Debug.Log("CardReader selected item: " + selectedItem.name + " ID: " + selectedItem.ID);
        Debug.Log("CardReader required keycard ID: " + requiredKeycardID);

        if (selectedItem.ID != requiredKeycardID)
        {
            Debug.Log("CardReader: Wrong item. Need keycard.");

            float duration = SoundEffectManager.PlayClip(
                "Door",
                "Door_locked",
                0.5f,
                true,
                0f,
                transform.position
            );

            StartCoroutine(BlockInteractionFor(duration));
            return;
        }

        StartCoroutine(UseKeycard(inventory));
    }

    private IEnumerator UseKeycard(InventoryController inventory)
    {
        isUsing = true;

        float duration = SoundEffectManager.PlayClip(
            "Keycard",
            "Keycard_use",
            1f,
            true,
            0f,
            transform.position
        );

        yield return new WaitForSeconds(duration);

        isAccepted = true;

        if (elevator != null)
        {
            elevator.SetKeycardAccepted(true);
        }

        if (removeKeycardAfterUse)
        {
            inventory.RemoveSelectedItem();
        }

        Debug.Log("CardReader: Access granted.");

        isUsing = false;
    }

    private IEnumerator BlockInteractionFor(float duration)
    {
        isUsing = true;

        yield return new WaitForSeconds(duration);

        isUsing = false;
    }
}