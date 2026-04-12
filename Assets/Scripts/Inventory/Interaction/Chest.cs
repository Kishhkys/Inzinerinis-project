using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Chest : MonoBehaviour, IInteractable
{
    public bool isOpened { get; private set; }
    public string ChestID { get; private set; }
    public GameObject itemPrefab;
    public Sprite defaultSprite;
    public Sprite lockedSprite;
    public Sprite openedSprite;
    public string requiredKeyID = null;

    private bool isLocked = false;
    private bool isUnlocking = false;

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
                StartCoroutine(UnlockAndOpenChest());
            }
            else
            {
                SoundEffectManager.PlayClip("Chest", "Chest_locked", 0.5f);
            }

            return;
        }

        OpenChest();
    }

    private IEnumerator UnlockAndOpenChest()
    {
        isLocked = false;
        GetComponent<SpriteRenderer>().sprite = defaultSprite;
        yield return new WaitForSeconds(3f);
        OpenChest();
    }

    private void OpenChest()
    {
        SetOpened(true);
        isUnlocking = false;
        SoundEffectManager.PlayClip("Chest", "Chest_open", 1f);

        if (itemPrefab)
        {
            Instantiate(itemPrefab, transform.position + Vector3.down, Quaternion.identity);
        }
    }

    public void SetOpened(bool opened)
    {
        isOpened = opened;

        if (isOpened)
        {
            GetComponent<SpriteRenderer>().sprite = openedSprite;
        }
    }

    void Start()
    {
        ChestID ??= GlobalHelper.GenerateUniqueId(gameObject);

        if (!string.IsNullOrEmpty(requiredKeyID))
        {
            isLocked = true;
            GetComponent<SpriteRenderer>().sprite = lockedSprite;
        }
    }

}
