using UnityEngine;
using UnityEngine.InputSystem;  // ← add this at the top

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    private bool isPaused = false;

void Update()
{
    Debug.Log("Update running test");
    if (Keyboard.current != null &&Keyboard.current.escapeKey.wasPressedThisFrame)
    {
        Debug.Log("Escape pressed");
        if (isPaused)
            Resume();
        else
            Pause();
    }
}

    public void Pause()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Resume()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
        Debug.Log("Quit called");
    }
}