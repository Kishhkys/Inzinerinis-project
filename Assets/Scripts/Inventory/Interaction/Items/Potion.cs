using UnityEngine;

public class Potion : Item
{
    [SerializeField] private float healAmount = 25f;

    public override void UseItem()
    {
        PlayerHealth player = FindFirstObjectByType<PlayerHealth>();
        if (player != null)
        {
            SoundEffectManager.PlayClip("Potion", "Potion_drink");
            player.UpdateHealth(healAmount);
            InteractionDialogueEvents.PotionDrunk();
            Debug.Log($"Healed {healAmount}");

            InventoryController inv = FindFirstObjectByType<InventoryController>();
            inv.RemoveSelectedItem();
        }
    }
}
