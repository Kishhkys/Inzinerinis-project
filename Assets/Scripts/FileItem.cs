using UnityEngine;

public class FileItem : Item
{
    [TextArea]
    public string fileContent;

    public override void UseItem()
    {
        Debug.Log("Opening patient file...");
        Debug.Log(fileContent);
    }

    public override void PickUp(string group = "File", string sound = "File_pick_up")
    {
        base.PickUp(group, sound);
        Debug.Log("Patient file collected");
    }
}