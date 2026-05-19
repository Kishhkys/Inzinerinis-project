using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorPanel : MonoBehaviour, IInteractable
{
    [Header("Elevator")]
    public Elevator elevator;

    [Header("Required Items In Order")]
    public List<int> requiredItemIDs = new();

    [Header("Settings")]
    public bool removeItemsAfterRepair = true;

    private int currentItemIndex = 0;
    private bool isRepaired = false;
    private bool isUsingItem = false;

    public bool CanInteract()
    {
        return !isRepaired && !isUsingItem;
    }

    public void Interact()
    {
        if (!CanInteract())
        {
            return;
        }

        InventoryController inventory = FindFirstObjectByType<InventoryController>();

        if (inventory == null)
        {
            return;
        }

        Item selectedItem = inventory.GetSelectedItem();

        if (selectedItem == null)
        {
            InteractionDialogueEvents.ElevatorPanelChecked(false, false, false);
            PlayWrongSound();
            return;
        }

        if (requiredItemIDs.Count == 0)
        {
            Debug.LogWarning("Required Item IDs list is empty on ElevatorPanel.");
            return;
        }

        if (currentItemIndex >= requiredItemIDs.Count)
        {
            RepairElevator();
            return;
        }

        int neededID = requiredItemIDs[currentItemIndex];
        bool willCompleteRepair = currentItemIndex >= requiredItemIDs.Count - 1;

        if (selectedItem.ID != neededID)
        {
            InteractionDialogueEvents.ElevatorPanelChecked(true, false, false);
            PlayWrongSound();
            return;
        }

        InteractionDialogueEvents.ElevatorPanelChecked(true, true, willCompleteRepair);

        StartCoroutine(UseCorrectItemRoutine(inventory, selectedItem));
    }

    private IEnumerator UseCorrectItemRoutine(InventoryController inventory, Item selectedItem)
    {
        isUsingItem = true;

        float duration = PlayCorrectItemSound(selectedItem);

        if (removeItemsAfterRepair)
        {
            inventory.RemoveSelectedItem();
        }

        yield return new WaitForSeconds(duration);

        currentItemIndex++;

        if (currentItemIndex >= requiredItemIDs.Count)
        {
            RepairElevator();
        }
        else
        {
            isUsingItem = false;
        }
    }

    private float PlayCorrectItemSound(Item selectedItem)
    {
        if (selectedItem is KeyItem key)
        {
            key.UseKey(transform.position);
            return 1f;
        }

        if (selectedItem is TapeItem tape)
        {
            return tape.UseTape(transform.position);
        }

        selectedItem.UseItem();
        return 1f;
    }

    private void PlayWrongSound()
    {
        float duration = SoundEffectManager.PlayClip(
            "Elevator",
            "Elevator_creak",
            0.5f,
            true,
            0f,
            transform.position
        );

        StartCoroutine(BlockInteractionFor(duration));
    }

    private IEnumerator BlockInteractionFor(float duration)
    {
        isUsingItem = true;

        yield return new WaitForSeconds(duration);

        isUsingItem = false;
    }

    private void RepairElevator()
    {
        isRepaired = true;
        isUsingItem = false;

        if (elevator != null)
        {
            elevator.SetOpen(true);
        }
        else
        {
            Debug.LogWarning("Elevator is not assigned in ElevatorPanel inspector.");
        }
    }
}
