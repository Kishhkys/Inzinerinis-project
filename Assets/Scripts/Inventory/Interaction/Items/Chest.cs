using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Chest : MonoBehaviour, IInteractable
{
    private static readonly WaitForSeconds UnlockDelay = new(3f);

    public bool IsOpened { get; private set; }
    public string ChestID { get; private set; }
    public GameObject itemPrefab;
    public Sprite defaultSprite;
    public Sprite lockedSprite;
    public Sprite openedSprite;
    public string requiredKeyID = null;

    private bool isLocked = false;
    private bool isUnlocking = false;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        TryGetComponent(out spriteRenderer);
    }

    public bool CanInteract()
    {
        return !IsOpened && !isUnlocking;
    }

    public void Interact()
    {
        if (!CanInteract()) return;

        InventoryController inventory = FindFirstObjectByType<InventoryController>();
        if (inventory == null) return;

        Item selectedItem = inventory.GetSelectedItem();

        if (isLocked)
        {
            KeyItem key = selectedItem as KeyItem;
            bool hasMatchingKey = key != null && key.targetID == requiredKeyID;
            InteractionDialogueEvents.LockedContainerChecked(hasMatchingKey);

            if (hasMatchingKey)
            {
                isUnlocking = true;
                key.UseKey(transform.position);
                inventory.RemoveSelectedItem();
                StartCoroutine(UnlockAndOpenChest());
            }
            else
            {
                SoundEffectManager.PlayClip("Chest", "Chest_locked", 0.5f, true, 0f, transform.position);
            }

            return;
        }

        OpenChest();
    }

    private IEnumerator UnlockAndOpenChest()
    {
        isLocked = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = defaultSprite;
        }

        yield return UnlockDelay;
        OpenChest();
    }

    private void OpenChest()
    {
        SetOpened(true);
        isUnlocking = false;
        SoundEffectManager.PlayClip("Chest", "Chest_open", 1f, true, 0f, transform.position);

        if (itemPrefab)
        {
            Instantiate(itemPrefab, transform.position + Vector3.down, Quaternion.identity);
        }
    }

    public void SetOpened(bool opened)
    {
        IsOpened = opened;

        if (IsOpened && spriteRenderer != null)
        {
            spriteRenderer.sprite = openedSprite;
        }
    }

    void Start()
    {
        ChestID ??= GlobalHelper.GenerateUniqueId(gameObject);

        if (!string.IsNullOrEmpty(requiredKeyID))
        {
            isLocked = true;

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = lockedSprite;
            }
        }
    }

}
