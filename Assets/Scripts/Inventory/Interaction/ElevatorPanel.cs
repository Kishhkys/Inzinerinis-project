using System.Collections.Generic;
using UnityEngine;

public class ElevatorPanel : MonoBehaviour, IInteractable
{
    [Header("Elevator")]
    public Elevator elevator;

    [Header("Required Items In Order")]
    public List<int> requiredItemIDs = new List<int>();

    [Header("Settings")]
    public bool removeItemsAfterRepair = true;

    private int currentItemIndex = 0;
    private bool isRepaired = false;

    public bool CanInteract()
    {
        return !isRepaired;
    }

    public void Interact()
    {
        if (isRepaired) return;

        InventoryController inventory = FindFirstObjectByType<InventoryController>();

        if (inventory == null)
        {
            return;
        }

        Item selectedItem = inventory.GetSelectedItem();

        if (selectedItem == null)
        {
            Debug.Log("Choose correct item");
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

        if (selectedItem.ID != neededID)
        {
            Debug.Log("Wrong item. Need item ID: " + neededID);
            PlayWrongSound();
            return;
        }

        Debug.Log("Used item: " + selectedItem.name);

        PlayCorrectItemSound(selectedItem);

        if (removeItemsAfterRepair)
        {
            inventory.RemoveSelectedItem();
        }

        currentItemIndex++;

        if (currentItemIndex >= requiredItemIDs.Count)
        {
            RepairElevator();
        }
        else
        {
            Debug.Log("Next required item ID: " + requiredItemIDs[currentItemIndex]);
        }
    }

    private void PlayCorrectItemSound(Item selectedItem)
    {
        if (selectedItem is KeyItem key)
        {
            key.UseKey();
        }
        else if (selectedItem is TapeItem tape)
        {
            tape.UseTape();
        }
        else
        {
            selectedItem.UseItem();
        }
    }

    private void PlayWrongSound()
    {
        SoundEffectManager.PlayClip("Elevator", "Elevator_creak", 0.5f);
    }

    private void RepairElevator()
    {
        isRepaired = true;

        Debug.Log("Elevator repaired");

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