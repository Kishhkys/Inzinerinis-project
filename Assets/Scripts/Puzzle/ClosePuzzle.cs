using UnityEngine;
using UnityEngine.InputSystem;

public class ClosePuzzle : MonoBehaviour
{
    public GameObject puzzleUI;

    void Update()
    {
        if (puzzleUI != null &&
            puzzleUI.activeSelf &&
            Keyboard.current != null &&
            Keyboard.current.tabKey.wasPressedThisFrame)
        {
            Close();
        }
    }

    public void Close()
    {
        if (puzzleUI != null)
        {
            puzzleUI.SetActive(false);
        }

        Time.timeScale = 1f;

        if (ActionPromptBox.Instance != null)
        {
            ActionPromptBox.Instance.SetClosePrompt(false);
        }
    }
}