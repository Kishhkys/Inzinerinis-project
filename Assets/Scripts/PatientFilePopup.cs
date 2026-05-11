using UnityEngine;
using TMPro;

public class PatientFilePopup : MonoBehaviour
{
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TMP_Text contentText;

    private void Start()
    {
        popupPanel.SetActive(false);
    }

    public void ShowFile(string content)
    {
        popupPanel.SetActive(true);
        contentText.text = content;

        Time.timeScale = 0f;
    }

    public void HideFile()
    {
        popupPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (popupPanel.activeSelf && Input.GetKeyDown(KeyCode.E))
        {
            HideFile();
        }
    }
}