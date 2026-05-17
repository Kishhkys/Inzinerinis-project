using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    private ItemDictionary itemDictionary;

    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject slotPrefab;

    [SerializeField] private int slotCount = 9;

    [SerializeField] private GameObject[] itemPrefabs;

    private Key[] inventoryKeys;
    private int selectedSlotIndex = 0;

    void Awake()
    {
        itemDictionary = FindFirstObjectByType<ItemDictionary>();

        if (inventoryPanel == null || slotPrefab == null)
        {
            Debug.LogError("InventoryController is missing inventoryPanel or slotPrefab reference.", this);
            enabled = false;
            return;
        }

        inventoryKeys = new Key[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotObject = Instantiate(slotPrefab, inventoryPanel.transform);
            Slot slot = slotObject.GetComponent<Slot>();

            if (slot == null || slot.slotNum == null)
            {
                Debug.LogError("Inventory slot prefab is missing Slot component or slot number text.", slotObject);
                enabled = false;
                return;
            }

            slot.slotNum.text = (i + 1).ToString();

            inventoryKeys[i] = i < 9 ? (Key)((int)Key.Digit1 + i) : Key.Digit0;

            if (i == 0)
            {
                SelectSlot(i);
            }
        }
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null || inventoryKeys == null)
        {
            return;
        }

        for (int i = 0; i < slotCount; i++)
        {
            if (keyboard[inventoryKeys[i]].wasPressedThisFrame)
            {
                SelectSlot(i);
            }
        }

        if (keyboard.eKey.wasPressedThisFrame)
        {
            if (InteractionDetector.ShouldBlockInventoryUse)
            {
                return;
            }

            UseSelectedItem();
        }
    }

    void SelectSlot(int index)
    {
        selectedSlotIndex = index;
        Debug.Log($"slot selected {index}");
        UpdateSlotVisuals();
    }

    void UpdateSlotVisuals()
    {
        if (inventoryPanel == null)
        {
            return;
        }

        for (int i = 0; i < inventoryPanel.transform.childCount; i++)
        {
            Slot slot = inventoryPanel.transform.GetChild(i).GetComponent<Slot>();
            if (slot == null)
            {
                continue;
            }

            Image img = slot.GetComponent<Image>();
            if (img == null)
            {
                continue;
            }

            if (i == selectedSlotIndex)
            {
                img.sprite = slot.activeSprite;
            }
            else
            {
                img.sprite = slot.regularSprite;
            }
        }
    }

    public void UseSelectedItem()
    {
        UseItemInSlot(selectedSlotIndex);
    }

    void UseItemInSlot(int index)
    {
        if (inventoryPanel == null || index < 0 || index >= inventoryPanel.transform.childCount)
        {
            return;
        }

        Slot slot = inventoryPanel.transform.GetChild(index).GetComponent<Slot>();

        if (slot != null && slot.currentItem != null)
        {
            Item item = slot.currentItem.GetComponent<Item>();

            if (item != null)
            {
                item.UseItem();
            }
        }
    }

    public Item GetSelectedItem()
    {
        if (inventoryPanel == null)
        {
            return null;
        }

        if (selectedSlotIndex < 0 || selectedSlotIndex >= inventoryPanel.transform.childCount)
        {
            return null;
        }

        Slot slot = inventoryPanel.transform.GetChild(selectedSlotIndex).GetComponent<Slot>();

        if (slot != null && slot.currentItem != null)
        {
            return slot.currentItem.GetComponent<Item>();
        }

        return null;
    }

    public void RemoveSelectedItem()
    {
        if (inventoryPanel == null || selectedSlotIndex < 0 || selectedSlotIndex >= inventoryPanel.transform.childCount)
        {
            return;
        }

        Slot slot = inventoryPanel.transform.GetChild(selectedSlotIndex).GetComponent<Slot>();

        if (slot != null && slot.currentItem != null)
        {
            Destroy(slot.currentItem);
            slot.currentItem = null;
        }
    }

    public bool AddItem(GameObject itemPrefab)
    {
        if (inventoryPanel == null || itemPrefab == null)
        {
            return false;
        }

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot != null && slot.currentItem == null)
            {
                Item sourceItem = itemPrefab.GetComponent<Item>();

                GameObject newItem = Instantiate(itemPrefab, slot.transform);
                Item newItemComp = newItem.GetComponent<Item>();

                RectTransform rectTransform = newItem.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = Vector2.zero;
                }

                if (sourceItem != null && newItemComp != null)
                {
                    newItemComp.ID = sourceItem.ID;
                    newItemComp.Name = sourceItem.Name;
                }

                slot.currentItem = newItem;
                return true;
            }
        }

        InteractionDialogueEvents.InventoryFullChecked();
        return false;
    }
}
