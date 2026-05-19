using UnityEngine;

public class PliersItem : Item
{
    public override void UseItem()
    {
        SoundEffectManager.PlayClip("Tools", "Pliers_use", 1f, true, 0f, transform.position);
    }

    public override void PickUp(string group = null, string sound = null)
    {
        base.PickUp("Tools", "Pliers_pick_up");
    }
}
