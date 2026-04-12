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
        //if (collision.CompareTag("Item"))
        //{
        //    Item item = collision.GetComponent<Item>();
        //    if (item != null)
        //    {

        //        bool itemAdded = inventoryController.AddItem(collision.gameObject);

        //        if (itemAdded)
        //        {
        //            Debug.Log("Trig");
        //            SoundEffectManager.Play("ItemCollect", 1f);
        //            item.PickUp();
        //            Destroy(collision.gameObject);
        //        }
        //    }
        //}

        //if (collision.CompareTag("Item"))
        //{
        //    Item item = collision.GetComponent<Item>();
        //    if (item != null)
        //    {
        //        Debug.Log($"Trying to add: {item.Name}, ID: {item.ID}");
        //        bool itemAdded = inventoryController.AddItem(collision.gameObject);
        //        Debug.Log($"Item added: {itemAdded}");
        //        if (itemAdded)
        //        {
        //            SoundEffectManager.Play("ItemCollect", 1f);
        //            item.PickUp();
        //            Destroy(collision.gameObject);
        //        }
        //    }
        //}

        if (collision.CompareTag("Item"))
        {
            Item item = collision.GetComponent<Item>();
            if (item != null && item.gameObject.activeSelf)
            {
                bool itemAdded = inventoryController.AddItem(collision.gameObject);
                if (itemAdded)
                {
                    SoundEffectManager.Play("ItemCollect", 1f);
                    item.PickUp();
                    collision.gameObject.SetActive(false);
                    Destroy(collision.gameObject);
                }
            }
        }
    }

        void Update()
        {

        }
    }

