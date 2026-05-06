using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    private bool isTransitioning = false;

    private void Awake()
    {
        AmbientSoundManager.PlayMusic("Menu");
    }

    public void PlayGame()
    {
        if (isTransitioning) return;

        StartCoroutine(PlayGameRoutine());
    }

    private IEnumerator PlayGameRoutine()
    {
        isTransitioning = true;

        SoundEffectManager.PlayClip("Menu", "Menu_click", 0.4f);

        AmbientSoundManager.FadeOutMusic(1f);

        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene("Scene1");
    }

    public void QuitGame()
    {
        if (isTransitioning) return;

        StartCoroutine(QuitGameRoutine());
    }

    private IEnumerator QuitGameRoutine()
    {
        isTransitioning = true;

        SoundEffectManager.PlayClip("Menu", "Menu_click", 0.4f);

        AmbientSoundManager.FadeOutMusic(1f);

        yield return new WaitForSeconds(1f);

        Debug.Log("Quit called");
        Application.Quit();
    }
}