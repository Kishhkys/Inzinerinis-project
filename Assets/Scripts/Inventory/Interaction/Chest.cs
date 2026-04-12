using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Chest : MonoBehaviour, IInteractable
{
    public bool isOpened {  get; private set; }
    public string ChestID { get; private set; }
    public GameObject itemPrefab;
    public Sprite defaultSprite;
    public Sprite lockedSprite;
    public Sprite openedSprite;
    public string requiredKeyID = null;
    private bool isLocked = false;

    public bool CanInteract()
    {
        if (isLocked && Keyboard.current.eKey.wasPressedThisFrame)
        {
            SoundEffectManager.PlayClip("Chest", "Chest_locked", 0.5f);
        }
        return !isOpened;
    }

    public void Interact()
    {
        if (!CanInteract()) return;

        InventoryController inventory = FindFirstObjectByType<InventoryController>();
        Item selectedItem = inventory.GetSelectedItem();
        if(!string.IsNullOrEmpty(requiredKeyID))
        {
            if (selectedItem != null && selectedItem is KeyItem key && key.targetID == requiredKeyID)
            {
                key.UseItem();
                inventory.RemoveSelectedItem();
                StartCoroutine(UnlockAndOpenChest());
                return;
            }
        }

        else
        {
            OpenChest();

        }

    }

    private IEnumerator UnlockAndOpenChest()
    {
        GetComponent<SpriteRenderer>().sprite = defaultSprite;
        yield return new WaitForSeconds(2f);
        OpenChest();

    }

    private void OpenChest() 
    {
        SetOpened(true);
        SoundEffectManager.PlayRandomClip("Chest", 1f);
        if (itemPrefab)
        {
            GameObject droppedItem = Instantiate(itemPrefab, transform.position + Vector3.down , Quaternion.identity);

        }
    
    }

    public void SetOpened(bool opened) 
    {
        isOpened = opened;
        if(isOpened)
        {
            GetComponent<SpriteRenderer>().sprite = openedSprite;
            Debug.Log("Sprite changed");
        }
    
    }

    void Start()
    {
        ChestID ??= GlobalHelper.GenerateUniqueId(gameObject);
        if(!string.IsNullOrEmpty(requiredKeyID))
        {
            isLocked = true;
            GetComponent<SpriteRenderer>().sprite = lockedSprite;
        }

    }

}
