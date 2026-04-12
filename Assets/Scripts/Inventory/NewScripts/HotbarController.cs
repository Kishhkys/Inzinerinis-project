using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HotbarController : MonoBehaviour
{
    //public GameObject hotbarPanel;
    //public GameObject slotPrefab;
    //[SerializeField] private int slotCount = 6;

    //private ItemDictionary itemDictionary;

    //private Key[] hotbarKeys;

    //private void Awake()
    //{
    //    itemDictionary = FindFirstObjectByType<ItemDictionary>();

    //    hotbarKeys = new Key[slotCount];
    //    for (int i = 0; i < slotCount; i++)
    //    {
    //        hotbarKeys[i] = i < 9 ? (Key)((int)Key.Digit1 + i) : Key.Digit0;
    //    }
    //}

    //void Update()
    //{
    //    for (int i = 0; i < slotCount; i++)
    //    {
    //        if (Keyboard.current[hotbarKeys[i]].wasPressedThisFrame)
    //        {
    //            UseItemInSlot(i);
    //        }
    //    }
    //}

    //void UseItemInSlot(int index)
    //{
    //    Slot slot = hotbarPanel.transform.GetChild(index).GetComponent<Slot>();
    //    if (slot.currentItem != null)
    //    {
    //        Item item = slot.currentItem.GetComponent<Item>();
    //        item.UseItem();
    //    }
    //}

    //public List<InventorySaveData> GetHotbarItems()
    //{
    //    List<InventorySaveData> invData = new List<InventorySaveData>();
    //    foreach (Transform slotTransform in hotbarPanel.transform)
    //    {
    //        Slot slot = slotTransform.GetComponent<Slot>();
    //        if (slot.currentItem != null)
    //        {
    //            Item item = slot.currentItem.GetComponent<Item>();
    //            invData.Add(new InventorySaveData { itemID = item.ID, slotIndex = slotTransform.GetSiblingIndex() });
    //        }
    //    }

    //    return invData;

    //}


    //public void SetHotbarItems(List<InventorySaveData> inventorySaveData)
    //{
    //    foreach (Transform child in hotbarPanel.transform)
    //    {
    //        Destroy(child.gameObject);
    //    }

    //    for (int i = 0; i < slotCount; i++)
    //    {
    //        Instantiate(slotPrefab, hotbarPanel.transform);
    //    }

    //    foreach (InventorySaveData data in inventorySaveData)
    //    {
    //        if (data.slotIndex < slotCount)
    //        {
    //            Slot slot = hotbarPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();
    //            GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);
    //            if (itemPrefab != null)
    //            {
    //                GameObject item = Instantiate(itemPrefab, slot.transform);
    //                item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    //                slot.currentItem = item;
    //            }
    //        }
    //    }
    //}

    public GameObject hotbarPanel;
    public GameObject slotPrefab;
    [SerializeField] private int slotCount = 6;

    private ItemDictionary itemDictionary;
    private Key[] hotbarKeys;

    // Pasirinktas slotas
    private int selectedSlotIndex = 0;

    private void Awake()
    {
        itemDictionary = FindFirstObjectByType<ItemDictionary>();

        hotbarKeys = new Key[slotCount];
        for (int i = 0; i < slotCount; i++)
        {
            hotbarKeys[i] = i < 9 ? (Key)((int)Key.Digit1 + i) : Key.Digit0;
        }
        UpdateSlotVisuals();
    }

    void Update()
    {

        for (int i = 0; i < slotCount; i++)
        {
            if (Keyboard.current[hotbarKeys[i]].wasPressedThisFrame)
            {
                SelectSlot(i);
            }
        }
    }

    void SelectSlot(int index)
    {
        selectedSlotIndex = index;
        UpdateSlotVisuals();
    }

    void UpdateSlotVisuals()
    {
        for (int i = 0; i < hotbarPanel.transform.childCount; i++)
        {
            Slot slot = hotbarPanel.transform.GetChild(i).GetComponent<Slot>();

        }
    }

    public void UseSelectedItem()
    {
        UseItemInSlot(selectedSlotIndex);
    }

    void UseItemInSlot(int index)
    {
        Slot slot = hotbarPanel.transform.GetChild(index).GetComponent<Slot>();
        if (slot.currentItem != null)
        {
            Item item = slot.currentItem.GetComponent<Item>();
            item.UseItem();
        }
    }


    public void RemoveSelectedItem()
    {
        Slot slot = hotbarPanel.transform.GetChild(selectedSlotIndex).GetComponent<Slot>();
        if (slot.currentItem != null)
        {
            Destroy(slot.currentItem);
            slot.currentItem = null;
        }
    }
}
