using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    private ItemDictionary itemDictionary;

    public GameObject inventoryPanel;
    public GameObject slotPrefab;

    [SerializeField] private int slotCount = 9;

    public GameObject[] itemPrefabs;

    private Key[] inventoryKeys;
    private int selectedSlotIndex = 0;

    void Awake()
    {
        itemDictionary = FindFirstObjectByType<ItemDictionary>();

        inventoryKeys = new Key[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            Slot slot = Instantiate(slotPrefab, inventoryPanel.transform).GetComponent<Slot>();
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
        for (int i = 0; i < slotCount; i++)
        {
            if (Keyboard.current[inventoryKeys[i]].wasPressedThisFrame)
            {
                SelectSlot(i);
            }
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
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
        for (int i = 0; i < inventoryPanel.transform.childCount; i++)
        {
            Slot slot = inventoryPanel.transform.GetChild(i).GetComponent<Slot>();
            Image img = slot.GetComponent<Image>();

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
        Slot slot = inventoryPanel.transform.GetChild(index).GetComponent<Slot>();

        if (slot.currentItem != null)
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
        if (selectedSlotIndex < 0 || selectedSlotIndex >= inventoryPanel.transform.childCount)
        {
            return null;
        }

        Slot slot = inventoryPanel.transform.GetChild(selectedSlotIndex).GetComponent<Slot>();

        if (slot.currentItem != null)
        {
            return slot.currentItem.GetComponent<Item>();
        }

        return null;
    }

    public void RemoveSelectedItem()
    {
        Slot slot = inventoryPanel.transform.GetChild(selectedSlotIndex).GetComponent<Slot>();

        if (slot.currentItem != null)
        {
            Destroy(slot.currentItem);
            slot.currentItem = null;
        }
    }

    public bool AddItem(GameObject itemPrefab)
    {
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot != null && slot.currentItem == null)
            {
                Item sourceItem = itemPrefab.GetComponent<Item>();

                GameObject newItem = Instantiate(itemPrefab, slot.transform);
                Item newItemComp = newItem.GetComponent<Item>();

                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

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
