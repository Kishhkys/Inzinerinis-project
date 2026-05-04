using UnityEngine;

public class TapeItem : Item
{
    public string targetID;


    public override void UseItem()
    {

    }

    public void UseTape(Vector3 faceTargetPosition)
    {
        SoundEffectManager.PlayClip("Elevator", "Tape", 1f, true, 0f, faceTargetPosition);
    }

}
