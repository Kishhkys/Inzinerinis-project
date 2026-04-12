using UnityEngine;
using UnityEngine.InputSystem;  // ← add this at the top

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public GameObject inventoryPanel;
    private bool isPaused = false;

    void Start()
    {
        pauseMenuPanel.SetActive(false);
        isPaused = false;
        Time.timeScale = 1f;
    }


    void Update()
{
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
        inventoryPanel.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Resume()
    {
        pauseMenuPanel.SetActive(false);
        inventoryPanel.SetActive(true);
        Time.timeScale = 1f;
        isPaused = false;
        SoundEffectManager.PlayClip("Menu", "Menu_click", 0.5f);  
    }

    public void QuitGame()
    {
        SoundEffectManager.PlayClip("Menu", "Menu_click", 0.5f);
        Time.timeScale = 1f;
        Application.Quit();
        Debug.Log("Quit called");
    }
}