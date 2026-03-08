using UnityEngine;

public class InventoryManager: MonoBehaviour
{
    public InventorySlot[] itemSlots;

    private void Start()
    {
        foreach(var slot in itemSlots)
        {
            slot.UpdateUI();
        }
    }
    private void OnEnable()
    {
        Loot.OnItemLooted += AddItem;
    }

    private void OnDisable()
    {
        Loot.OnItemLooted -= AddItem;
    }

    public void AddItem(ItemSO itemSO)
    {
        foreach (var slot in itemSlots)
        {
            if(slot.itemSO == null)
            {
                slot.itemSO = itemSO;
                slot.UpdateUI();
                return;
            }
        }
    }
}
