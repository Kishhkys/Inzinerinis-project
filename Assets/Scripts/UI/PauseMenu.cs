using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public GameObject inventoryPanel;

    [SerializeField] private PlayerController playerController;
    [SerializeField] private Animator playerAnimator;

    private bool isPaused = false;

    void Start()
    {
        pauseMenuPanel.SetActive(false);
        isPaused = false;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
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

        if (playerController != null)
            playerController.enabled = false;

        if (playerAnimator != null)
            playerAnimator.enabled = false;
    }

    public void Resume()
    {
        pauseMenuPanel.SetActive(false);
        inventoryPanel.SetActive(true);

        Time.timeScale = 1f;
        isPaused = false;

        if (playerController != null)
            playerController.enabled = true;

        if (playerAnimator != null)
            playerAnimator.enabled = true;

        SoundEffectManager.PlayClip("Menu", "Menu_click", 0.7f);
    }

    public void QuitGame()
    {
        SoundEffectManager.PlayClip("Menu", "Menu_click", 0.7f);

        Time.timeScale = 1f;

        Debug.Log("Quit called");
        Application.Quit();
    }
}