using System.Collections.Generic;
using UnityEngine;

public class ElevatorPanel : MonoBehaviour, IInteractable
{
    [Header("Elevator")]
    public Elevator elevator;

    [Header("Required Items")]
    public List<int> requiredItemIDs = new List<int>();

    [Header("Settings")]
    public bool removeItemsAfterRepair = true;

    private List<int> insertedItemIDs = new List<int>();
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
            return;
        }

        int selectedID = selectedItem.ID;

        if (!requiredItemIDs.Contains(selectedID))
        {
            Debug.Log("Wrong item");
            return;
        }

        if (insertedItemIDs.Contains(selectedID))
        {
            Debug.Log("Item already used");
            return;
        }

        insertedItemIDs.Add(selectedID);

        Debug.Log("Used item: " + selectedItem.name);

        if (removeItemsAfterRepair)
        {
            inventory.RemoveSelectedItem();
        }

        if (HasAllRequiredItems())
        {
            RepairElevator();
        }
        else
        {
            Debug.Log("Missing parts: " + GetMissingItemCount());
        }
    }

    private bool HasAllRequiredItems()
    {
        foreach (int id in requiredItemIDs)
        {
            if (!insertedItemIDs.Contains(id))
            {
                return false;
            }
        }

        return true;
    }

    private int GetMissingItemCount()
    {
        int missing = 0;

        foreach (int id in requiredItemIDs)
        {
            if (!insertedItemIDs.Contains(id))
            {
                missing++;
            }
        }

        return missing;
    }

    private void RepairElevator()
    {
        isRepaired = true;

        Debug.Log("Elevator repaired");

        if (elevator != null)
        {
            elevator.SetOpen(true);
        }
    }
}