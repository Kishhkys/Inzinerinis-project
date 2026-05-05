using UnityEngine;

public class TapeItem : Item
{
    public string targetID;


    public override void UseItem()
    {

    }

    public float UseTape(Vector3 faceTargetPosition)
    {
        return SoundEffectManager.PlayClip("Elevator", "Tape", 1f, true, 0f, faceTargetPosition);
    }

}
