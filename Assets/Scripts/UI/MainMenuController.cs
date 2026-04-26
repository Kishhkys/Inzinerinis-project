using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("sprint4Milda");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit called");
    }
}