using UnityEngine;

public class Potion : Item
{
    [SerializeField] private float healAmount = 25f;

    public override void UseItem()
    {
        PlayerHealth player = FindFirstObjectByType<PlayerHealth>();
        if (player != null)
        {
            SoundEffectManager.PlayClip("Potion", "Potion_drink", 1f, true, 0f, transform.position);
            player.UpdateHealth(healAmount);
            InteractionDialogueEvents.PotionDrunk();

            InventoryController inv = FindFirstObjectByType<InventoryController>();
            inv.RemoveSelectedItem();
        }
    }
}
