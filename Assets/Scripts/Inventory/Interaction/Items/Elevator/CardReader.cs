using System.Collections;
using UnityEngine;

public class CardReader : MonoBehaviour, IInteractable
{
    [Header("Elevator")]
    [SerializeField] private Elevator elevator;

    [Header("Required Item")]
    [SerializeField] private int requiredKeycardID = 7;

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
        if (!CanInteract())
        {
            return;
        }

        if (elevator == null)
        {
            Debug.LogWarning("CardReader: Elevator is not assigned.");
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


        StartCoroutine(UseKeycard(inventory, selectedItem));
    }

    private IEnumerator UseKeycard(InventoryController inventory, Item selectedItem)
    {
        isUsing = true;

        float duration;

        if (selectedItem is KeycardItem keycard)
        {
            duration = keycard.UseKeycard(transform.position);
        }
        else
        {
            duration = SoundEffectManager.PlayClip(
                "Keycard",
                "Keycard_use",
                1f,
                true,
                0f,
                transform.position
            );
        }

        yield return new WaitForSeconds(duration);

        isAccepted = true;
        elevator.SetKeycardAccepted(true);

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