using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PauseMenuPlayModeTests
{
    private readonly System.Collections.Generic.List<Object> createdObjects = new();

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Time.timeScale = 1f;

        foreach (Object createdObject in createdObjects)
        {
            if (createdObject != null)
            {
                Object.Destroy(createdObject);
            }
        }

        createdObjects.Clear();
        yield return null;
    }

    [UnityTest]
    public IEnumerator Start_HidesPausePanelAndResetsTimeScale()
    {
        Time.timeScale = 0f;

        PauseMenu pauseMenu = CreatePauseMenu();
        pauseMenu.pauseMenuPanel.SetActive(true);

        yield return null;

        Assert.That(pauseMenu.pauseMenuPanel.activeSelf, Is.False);
        Assert.That(Time.timeScale, Is.EqualTo(1f));
    }

    [UnityTest]
    public IEnumerator PauseAndResume_TogglePanelVisibilityAndTimeScale()
    {
        PauseMenu pauseMenu = CreatePauseMenu();

        yield return null;

        pauseMenu.Pause();
        Assert.That(pauseMenu.pauseMenuPanel.activeSelf, Is.True);
        Assert.That(Time.timeScale, Is.EqualTo(0f));

        pauseMenu.Resume();
        Assert.That(pauseMenu.pauseMenuPanel.activeSelf, Is.False);
        Assert.That(Time.timeScale, Is.EqualTo(1f));
    }

    private PauseMenu CreatePauseMenu()
    {
        GameObject menuObject = new("PauseMenu");
        createdObjects.Add(menuObject);

        GameObject panelObject = new("PausePanel");
        createdObjects.Add(panelObject);

        PauseMenu pauseMenu = menuObject.AddComponent<PauseMenu>();
        pauseMenu.pauseMenuPanel = panelObject;
        return pauseMenu;
    }
}
