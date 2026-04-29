using UnityEngine;
using UnityEngine.InputSystem;

public class ClosePuzzle : MonoBehaviour
{
    public GameObject puzzleUI;

    void Update()
    {
        if (puzzleUI != null &&
            puzzleUI.activeSelf &&
            Keyboard.current.tabKey.wasPressedThisFrame)
        {
            Close();
        }
    }

    public void Close()
    {
        puzzleUI.SetActive(false);
        Time.timeScale = 1f;
    }
}