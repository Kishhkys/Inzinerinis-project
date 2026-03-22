using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("milda1");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit called");
    }
}