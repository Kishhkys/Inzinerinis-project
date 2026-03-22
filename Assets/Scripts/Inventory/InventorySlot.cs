using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot: MonoBehaviour, IPointerClickHandler
{
    public ItemSO itemSO;
    public Image itemImage;
    public Image selectionHighlight;

    private InventoryManager manager;

    public void SetManager(InventoryManager inventoryManager)
    {
        manager = inventoryManager;
    }

    public void UpdateUI()
    {
        if (itemSO != null)
        {
            itemImage.sprite = itemSO.icon;
            itemImage.gameObject.SetActive(true);
        }
        else
        {
            itemImage.gameObject.SetActive(false);
        }
    }

    public void UseItem(GameObject user)
    {
        if (itemSO == null)
            return;

        itemSO.Use(user);
        itemSO = null;
        UpdateUI();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (manager != null)
        {
            manager.SelectSlot(this);
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectionHighlight != null)
        {
            selectionHighlight.gameObject.SetActive(selected);
        }
    }
}
