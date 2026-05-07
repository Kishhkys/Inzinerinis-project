using UnityEngine;
using TMPro;

public class ActionPromptBox : MonoBehaviour
{
    public static ActionPromptBox Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject promptBox;
    [SerializeField] private TMP_Text promptText;

    [Header("Messages")]
    [SerializeField] private string interactMessage = "Press E to interact";
    [SerializeField] private string hideMessage = "Hold CTRL to hide";
    [SerializeField] private string closeMessage = "Press TAB to close";

    private bool interactActive = false;
    private bool hideActive = false;
    private bool closeActive = false;

    private void Awake()
    {
        Instance = this;
        Refresh();
    }

    public void SetInteractPrompt(bool active)
    {
        interactActive = active;
        Refresh();
    }

    public void SetHidePrompt(bool active)
    {
        hideActive = active;
        Refresh();
    }

    public void SetClosePrompt(bool active)
    {
        closeActive = active;
        Refresh();
    }

    private void Refresh()
    {
        if (promptBox == null)
        {
            return;
        }

        if (closeActive)
        {
            promptBox.SetActive(true);

            if (promptText != null)
            {
                promptText.text = closeMessage;
            }

            return;
        }

        if (interactActive)
        {
            promptBox.SetActive(true);

            if (promptText != null)
            {
                promptText.text = interactMessage;
            }

            return;
        }

        if (hideActive)
        {
            promptBox.SetActive(true);

            if (promptText != null)
            {
                promptText.text = hideMessage;
            }

            return;
        }

        promptBox.SetActive(false);
    }
}