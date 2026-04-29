using UnityEngine;

public class TapeItem : Item
{
    public string targetID;


    public override void UseItem()
    {

    }

    public void UseTape()
    {
        SoundEffectManager.PlayClip("Elevator", "Tape");
    }

}
