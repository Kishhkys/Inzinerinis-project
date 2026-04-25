using System.Collections;
using UnityEngine;

public class Drawers : MonoBehaviour, IInteractable
{
    public bool isOpened { get; private set; }
    public string CabinetID { get; private set; }

    [Header("Item Spawn")]
    public GameObject itemPrefab;
    public Vector3 itemSpawnOffset = Vector3.down;
    public float itemSpawnDelay = 1f;

    [Header("Lock")]
    public string requiredKeyID = null;

    private bool isLocked = false;
    private bool isUnlocking = false;

    private void Start()
    {
        CabinetID ??= GlobalHelper.GenerateUniqueId(gameObject);

        if (!string.IsNullOrEmpty(requiredKeyID))
        {
            isLocked = true;
        }
    }

    public bool CanInteract()
    {
        return !isOpened && !isUnlocking;
    }

    public void Interact()
    {
        if (!CanInteract()) return;

        InventoryController inventory = FindFirstObjectByType<InventoryController>();
        if (inventory == null) return;

        Item selectedItem = inventory.GetSelectedItem();

        if (isLocked)
        {
            if (selectedItem is KeyItem key && key.targetID == requiredKeyID)
            {
                isUnlocking = true;
                key.UseKey();
                inventory.RemoveSelectedItem();

                isLocked = false;
                OpenDrawers();
            }
            else
            {
                SoundEffectManager.PlayClip("Chest", "Chest_locked", 0.5f);
            }

            return;
        }

        OpenDrawers();
    }

    private void OpenDrawers()
    {
        isOpened = true;
        isUnlocking = false;

        SoundEffectManager.PlayClip("Drawers", "Drawers_open", 1f);

        StartCoroutine(SpawnItemAfterDelay());
    }

    private IEnumerator SpawnItemAfterDelay()
    {
        yield return new WaitForSeconds(itemSpawnDelay);

        if (itemPrefab != null)
        {
            Instantiate(itemPrefab, transform.position + itemSpawnOffset, Quaternion.identity);
        }
    }
}