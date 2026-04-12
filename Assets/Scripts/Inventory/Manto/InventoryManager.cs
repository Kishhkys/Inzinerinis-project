using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager: MonoBehaviour
{
    public InventorySlot[] itemSlots;
    [SerializeField] private GameObject player;

    private InventorySlot selectedSlot;

    private void Awake()
    {
        Loot.OnItemLooted += AddItem;
    }

    private void OnDestroy()
    {
        Loot.OnItemLooted -= AddItem;
    }

    private void Start()
    {
        foreach (var slot in itemSlots)
        {
            slot.SetManager(this);
            slot.UpdateUI();
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            UseSelectedItem();
        }
    }

    public void AddItem(ItemSO itemSO)
    {
        foreach (var slot in itemSlots)
        {
            if (slot.itemSO == null)
            {
                slot.itemSO = itemSO;
                slot.UpdateUI();
                return;
            }
        }
    }

    public void SelectSlot(InventorySlot slot)
    {
        selectedSlot = slot;

        foreach (var itemSlot in itemSlots)
        {
            itemSlot.SetSelected(itemSlot == selectedSlot);
        }
    }

    private void UseSelectedItem()
    {
        if (selectedSlot == null || selectedSlot.itemSO == null)
            return;

        selectedSlot.UseItem(player);
    }
}
