using UnityEngine;

public class PlayerItemCollector : MonoBehaviour
{

    private InventoryController inventoryController;
    void Start()
    {
        inventoryController = FindFirstObjectByType<InventoryController>();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Item"))
        {
            Item item = collision.GetComponent<Item>();
            if (item != null && item.gameObject.activeSelf)
            {
                bool itemAdded = inventoryController.AddItem(collision.gameObject);
                if (itemAdded)
                {
                    SoundEffectManager.PlayRandomClip("ItemCollect", 0.7f);
                    item.PickUp();
                    InteractionDialogueEvents.ItemPickedUp(item);
                    collision.gameObject.SetActive(false);
                    Destroy(collision.gameObject);
                }
            }
        }
    }
}

