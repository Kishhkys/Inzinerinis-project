using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    private static readonly WaitForSecondsRealtime PlayTransitionDelay = new(0.3f);
    private static readonly WaitForSecondsRealtime QuitTransitionDelay = new(1f);

    [Header("Loading")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private float loadingDelay = 1f;

    private bool isTransitioning = false;

    private void Awake()
    {
        AmbientSoundManager.PlayMusic("Menu", 0.5f);

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }

    public void PlayGame()
    {
        if (isTransitioning) return;
        StartCoroutine(PlayGameRoutine());
    }

    private IEnumerator PlayGameRoutine()
    {
        isTransitioning = true;

        SoundEffectManager.PlayClip("Menu", "Menu_click", 0.7f);

        AmbientSoundManager.FadeOutMusic(1f);

        yield return PlayTransitionDelay;

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }

        yield return null;
        yield return new WaitForEndOfFrame();

        yield return new WaitForSecondsRealtime(loadingDelay);

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

        SoundEffectManager.PlayClip("Menu", "Menu_click", 0.7f);

        AmbientSoundManager.FadeOutMusic(1f);

        yield return QuitTransitionDelay;

        Application.Quit();
    }
}
