using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public GameObject inventoryPanel;
    public GameObject settingsPanel;

    [SerializeField] private PlayerController playerController;
    [SerializeField] private Animator playerAnimator;

    private bool isPaused = false;
    private GameObject[] pauseMenuItems;
    private Slider settingsSfxSlider;
    private Slider settingsAmbienceSlider;
    private TMP_FontAsset menuFont;
    private Color menuTextColor = new(0.196f, 0.196f, 0.196f, 1f);

    void Start()
    {
        EnsureSettingsPage();
        ShowPauseMenuPage();
        pauseMenuPanel.SetActive(false);
        isPaused = false;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                if (settingsPanel != null && settingsPanel.activeSelf)
                {
                    ShowPauseMenuPage();
                    return;
                }

                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        EnsureSettingsPage();
        pauseMenuPanel.SetActive(true);
        ShowPauseMenuPage();
        inventoryPanel.SetActive(false);

        Time.timeScale = 0f;
        isPaused = true;

        if (playerController != null)
            playerController.enabled = false;

        if (playerAnimator != null)
            playerAnimator.enabled = false;
    }

    public void OpenSettings()
    {
        if (settingsPanel == null)
        {
            return;
        }

        SetPauseMenuItemsActive(false);
        SyncSettingsSliders();
        settingsPanel.SetActive(true);
        SoundEffectManager.PlayClip("Menu", "Menu_click", 0.7f);
    }

    public void BackToPauseMenu()
    {
        ShowPauseMenuPage();
        SoundEffectManager.PlayClip("Menu", "Menu_click", 0.7f);
    }

    public void Resume()
    {
        pauseMenuPanel.SetActive(false);
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

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

        Application.Quit();
    }

    private void EnsureSettingsPage()
    {
        if (pauseMenuPanel == null || settingsPanel != null)
        {
            return;
        }

        CacheMenuTextStyle();
        CreateSettingsButton();
        CachePauseMenuItems();
        CreateSettingsPanel();
    }

    private void CacheMenuTextStyle()
    {
        TMP_Text text = pauseMenuPanel.GetComponentInChildren<TMP_Text>(true);

        if (text == null)
        {
            return;
        }

        menuFont = text.font;
        menuTextColor = text.color;
    }

    private void CreateSettingsButton()
    {
        CreateButton("SettingsButton", "Settings", pauseMenuPanel.transform, new Vector2(400f, 90f), new Vector2(0f, 0f), OpenSettings);

        MoveButton("ResumeButton", new Vector2(0f, 120f));
        MoveButton("QuitButton", new Vector2(0f, -120f));
    }

    private void CachePauseMenuItems()
    {
        int childCount = pauseMenuPanel.transform.childCount;
        pauseMenuItems = new GameObject[childCount];

        for (int i = 0; i < childCount; i++)
        {
            pauseMenuItems[i] = pauseMenuPanel.transform.GetChild(i).gameObject;
        }
    }

    private void CreateSettingsPanel()
    {
        settingsPanel = new GameObject("SettingsPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        settingsPanel.transform.SetParent(pauseMenuPanel.transform, false);

        RectTransform panelRect = settingsPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = settingsPanel.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0f);

        CreateText("SettingsTitle", "Settings", settingsPanel.transform, new Vector2(480f, 80f), new Vector2(0f, 150f), 72f);
        CreateText("SfxLabel", "SFX", settingsPanel.transform, new Vector2(180f, 50f), new Vector2(-170f, 60f), 42f);
        settingsSfxSlider = CreateSlider("SfxSlider", settingsPanel.transform, new Vector2(320f, 36f), new Vector2(110f, 60f));
        settingsSfxSlider.onValueChanged.AddListener(SoundEffectManager.SetVolume);

        CreateText("AmbienceLabel", "Ambience", settingsPanel.transform, new Vector2(220f, 50f), new Vector2(-150f, -20f), 42f);
        settingsAmbienceSlider = CreateSlider("AmbienceSlider", settingsPanel.transform, new Vector2(320f, 36f), new Vector2(110f, -20f));
        settingsAmbienceSlider.onValueChanged.AddListener(AmbientSoundManager.SetVolume);

        CreateButton("BackButton", "Back", settingsPanel.transform, new Vector2(300f, 80f), new Vector2(0f, -140f), BackToPauseMenu);
        settingsPanel.SetActive(false);
    }

    private void ShowPauseMenuPage()
    {
        SetPauseMenuItemsActive(true);

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void SetPauseMenuItemsActive(bool active)
    {
        if (pauseMenuItems == null)
        {
            return;
        }

        foreach (GameObject item in pauseMenuItems)
        {
            if (item != null && item != settingsPanel)
            {
                item.SetActive(active);
            }
        }
    }

    private void SyncSettingsSliders()
    {
        if (settingsSfxSlider != null)
        {
            settingsSfxSlider.SetValueWithoutNotify(SoundEffectManager.GetVolume());
        }

        if (settingsAmbienceSlider != null)
        {
            settingsAmbienceSlider.SetValueWithoutNotify(AmbientSoundManager.GetVolume());
        }
    }

    private void MoveButton(string buttonName, Vector2 anchoredPosition)
    {
        Transform button = pauseMenuPanel.transform.Find(buttonName);
        RectTransform rect = button != null ? button.GetComponent<RectTransform>() : null;

        if (rect != null)
        {
            rect.anchoredPosition = anchoredPosition;
        }
    }

    private Button CreateButton(string objectName, string label, Transform parent, Vector2 size, Vector2 position, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        CreateText(label + "Text", label, buttonObject.transform, size, Vector2.zero, 64f);
        return button;
    }

    private TMP_Text CreateText(string objectName, string text, Transform parent, Vector2 size, Vector2 position, float fontSize)
    {
        GameObject textObject = new(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        TMP_Text textComponent = textObject.GetComponent<TMP_Text>();
        textComponent.text = text;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.fontSize = fontSize;
        textComponent.textWrappingMode = TextWrappingModes.NoWrap;
        textComponent.color = menuTextColor;

        if (menuFont != null)
        {
            textComponent.font = menuFont;
        }

        return textComponent;
    }

    private Slider CreateSlider(string objectName, Transform parent, Vector2 size, Vector2 position)
    {
        GameObject sliderObject = new(objectName, typeof(RectTransform), typeof(Slider));
        sliderObject.transform.SetParent(parent, false);

        RectTransform rect = sliderObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Image background = CreateSliderImage("Background", sliderObject.transform, Vector2.zero, Vector2.one, new Color(0.08f, 0.08f, 0.08f, 0.8f));
        Image fill = CreateSliderImage("Fill", sliderObject.transform, Vector2.zero, Vector2.one, new Color(0.72f, 0.08f, 0.08f, 1f));
        Image handle = CreateSliderImage("Handle", sliderObject.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0.95f, 0.93f, 0.86f, 1f));

        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(28f, 48f);

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.targetGraphic = handle;
        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.direction = Slider.Direction.LeftToRight;

        background.raycastTarget = false;
        fill.raycastTarget = false;
        handle.raycastTarget = true;

        return slider;
    }

    private Image CreateSliderImage(string objectName, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject imageObject = new(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        return image;
    }
}
