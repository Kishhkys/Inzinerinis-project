using UnityEngine;

public class KeycardItem : Item
{
    public override void UseItem()
    {
        SoundEffectManager.PlayClip("Keycard", "Keycard_use", 1f, true);
    }

    public void UseKeycard(Vector3 faceTargetPosition)
    {
        SoundEffectManager.PlayClip(
            "Keycard",
            "Keycard_use",
            1f,
            true,
            0f,
            faceTargetPosition
        );
    }

    public override void PickUp(string group = null, string sound = null)
    {
        base.PickUp("Keycard", "Keycard_pick_up");
        Debug.Log("Picked up keycard.");
    }
}