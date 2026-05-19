using UnityEngine;

public class InteractiveWall : MonoBehaviour, IInteractable
{
    public GameObject puzzleUI;

    public void Interact()
    {
        if (puzzleUI != null)
        {
            puzzleUI.SetActive(true);
            Time.timeScale = 0f;

            if (ActionPromptBox.Instance != null)
            {
                ActionPromptBox.Instance.SetInteractPrompt(false);
                ActionPromptBox.Instance.SetHidePrompt(false);
                ActionPromptBox.Instance.SetClosePrompt(true);
            }
        }
    }

    public bool CanInteract()
    {
        return true;
    }
}
