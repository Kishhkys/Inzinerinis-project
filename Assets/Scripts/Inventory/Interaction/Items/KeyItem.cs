using UnityEngine;

public class KeyItem : Item
{
    public string targetID;


    public override void UseItem()
    {

    }

    public void UseKey(Vector3 faceTargetPosition)
    {
        SoundEffectManager.PlayClip("Key", "Keys_unlock", 1f, true, 0f, faceTargetPosition);
    }

    public override void PickUp(string group = null, string sound = null)
    {
        base.PickUp("Key", "Keys_pick_up");
        Debug.Log("Picked up key for target: " + targetID);
    }

}
