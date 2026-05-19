using System.Collections;
using UnityEngine;

public class Drain : MonoBehaviour, IInteractable
{
    public bool isUsed { get; private set; }

    [Header("Required Item")]
    public int requiredItemID;

    [Header("Key Spawn")]
    public GameObject keyPrefab;
    public Vector3 keySpawnOffset = Vector3.down;
    public float keySpawnDelay = 2f;

    [Header("Settings")]
    public bool removePliersAfterUse = false;

    private bool isUsing = false;

    public bool CanInteract()
    {
        return !isUsed && !isUsing;
    }

    public void Interact()
    {
        if (!CanInteract()) return;

        InventoryController inventory = FindFirstObjectByType<InventoryController>();
        if (inventory == null) return;

        Item selectedItem = inventory.GetSelectedItem();

        if (selectedItem == null)
        {
            InteractionDialogueEvents.DrainChecked(false);
            SoundEffectManager.PlayClip("Elevator", "Elevator_creak", 0.5f, true, 0f, transform.position);
            return;
        }

        if (selectedItem.ID != requiredItemID)
        {
            InteractionDialogueEvents.DrainChecked(false);
            SoundEffectManager.PlayClip("Elevator", "Elevator_creak", 0.5f, true, 0f, transform.position);
            return;
        }

        InteractionDialogueEvents.DrainChecked(true);
        StartCoroutine(UseDrain(inventory, selectedItem));
    }

    private IEnumerator UseDrain(InventoryController inventory, Item selectedItem)
    {
        isUsing = true;
        isUsed = true;

        SoundEffectManager.PlayClip("Drain", "Drain", 0.1f, true, 0f, transform.position);
        selectedItem.UseItem();

        if (removePliersAfterUse)
        {
            inventory.RemoveSelectedItem();
        }

        yield return new WaitForSeconds(keySpawnDelay);

        if (keyPrefab != null)
        {
            Instantiate(keyPrefab, transform.position + keySpawnOffset, Quaternion.identity);
        }

        isUsing = false;
    }
}
