using UnityEngine;

public class FileItem : Item
{
    [TextArea(5, 15)]
    public string fileContent;

    private PatientFilePopup patientFilePopup;

    private void Start()
    {
        patientFilePopup = FindFirstObjectByType<PatientFilePopup>();
    }

    public override void UseItem()
    {
        if (patientFilePopup != null)
        {
            patientFilePopup.ShowFile(fileContent);
        }
        else
        {
            Debug.LogWarning("PatientFilePopup not found in scene.");
        }
    }

    public override void PickUp(string group = "File", string sound = "File_pick_up")
    {
        base.PickUp(group, sound);

        UseItem();
    }
}
