using UnityEngine;

public class TapeItem : Item
{
    public string targetID;


    public override void UseItem()
    {

    }

    public float UseTape(Vector3 faceTargetPosition)
    {
        return SoundEffectManager.PlayClip("Elevator", "Tape", 0.5f, true, 0f, faceTargetPosition);
    }

}
