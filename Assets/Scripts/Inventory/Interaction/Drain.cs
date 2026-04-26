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
            Debug.Log("Need something to pull the key out.");
            SoundEffectManager.PlayClip("Elevator", "Elevator_creak", 0.5f);
            return;
        }

        if (selectedItem.ID != requiredItemID)
        {
            Debug.Log("Wrong item. Need pliers.");
            SoundEffectManager.PlayClip("Elevator", "Elevator_creak", 0.5f);
            return;
        }

        StartCoroutine(UseDrain(inventory, selectedItem));
    }

    private IEnumerator UseDrain(InventoryController inventory, Item selectedItem)
    {
        isUsing = true;
        isUsed = true;

        Debug.Log("Used pliers on drain.");

        SoundEffectManager.PlayRandomClip("Drain", 0.3f);
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