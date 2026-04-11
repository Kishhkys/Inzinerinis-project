using UnityEngine;
using UnityEngine.InputSystem;

public class Chest : MonoBehaviour, IInteractable
{
    public bool isOpened {  get; private set; }
    public string ChestID { get; private set; }
    public GameObject itemPrefab;
    public Sprite openedSprite;
    public string requiredKeyID;

    public bool CanInteract()
    {
        return !isOpened;
    }

    public void Interact()
    {
        //if (!CanInteract()) return;
        //SoundEffectManager.Play("Chest", 1f);
        //OpenChest();
        if (!CanInteract()) return;

        InventoryController inventory = FindFirstObjectByType<InventoryController>();
        Item selectedItem = inventory.GetSelectedItem();

        if (selectedItem != null && selectedItem is KeyItem key && key.targetID == requiredKeyID)
        {
           SoundEffectManager.Play("Chest", 1f);
            inventory.RemoveSelectedItem();
            OpenChest(); 
            return;
        }

    }

    private void OpenChest() 
    {
        SetOpened(true);
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

    }

}
