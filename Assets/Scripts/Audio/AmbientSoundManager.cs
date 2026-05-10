using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AmbientSoundManager : MonoBehaviour
{
    private static AmbientSoundManager Instance;

    private static AudioSource audioSource;
    private static SoundEffectLibrary soundEffectLibrary;

    [SerializeField] private Slider sfxSlider;

    [Header("Volume Icon")]
    [SerializeField] private Image volumeIcon;
    [SerializeField] private Sprite volumeOnSprite;
    [SerializeField] private Sprite volumeOffSprite;

    private static float sliderValue = 1f;
    private static float currentMaxVolume = 0.3f;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
        soundEffectLibrary = GetComponent<SoundEffectLibrary>();
    }

    private void Start()
    {
        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.SetValueWithoutNotify(sliderValue);
            sfxSlider.onValueChanged.AddListener(delegate { OnValueChanged(); });
        }

        UpdateVolumeIcon();
    }

    public static void PlayMusic(string soundName, float volume = 0.3f)
    {
        if (Instance == null || audioSource == null || soundEffectLibrary == null)
        {
            Debug.LogWarning("AmbientSoundManager is missing AudioSource or SoundEffectLibrary.");
            return;
        }

        AudioClip audioClip = soundEffectLibrary.GetRandomClip(soundName);

        if (audioClip == null)
        {
            Debug.LogWarning("Music clip not found: " + soundName);
            return;
        }

        currentMaxVolume = volume;

        if (Instance.fadeCoroutine != null)
        {
            Instance.StopCoroutine(Instance.fadeCoroutine);
        }

        audioSource.clip = audioClip;
        audioSource.volume = sliderValue * currentMaxVolume;
        audioSource.loop = true;
        audioSource.Play();

        Instance.UpdateVolumeIcon();
    }

    public static void FadeInMusic(string soundName, float volume = 0.3f, float fadeTime = 1f)
    {
        if (Instance == null || audioSource == null || soundEffectLibrary == null)
        {
            Debug.LogWarning("AmbientSoundManager is missing AudioSource or SoundEffectLibrary.");
            return;
        }

        AudioClip audioClip = soundEffectLibrary.GetRandomClip(soundName);

        if (audioClip == null)
        {
            Debug.LogWarning("Music clip not found: " + soundName);
            return;
        }

        currentMaxVolume = volume;

        if (Instance.fadeCoroutine != null)
        {
            Instance.StopCoroutine(Instance.fadeCoroutine);
        }

        Instance.fadeCoroutine = Instance.StartCoroutine(
            Instance.FadeInCoroutine(audioClip, fadeTime)
        );
    }

    private IEnumerator FadeInCoroutine(AudioClip audioClip, float fadeTime)
    {
        audioSource.clip = audioClip;
        audioSource.volume = 0f;
        audioSource.loop = true;
        audioSource.Play();

        float targetVolume = sliderValue * currentMaxVolume;
        float timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, targetVolume, timer / fadeTime);
            yield return null;
        }

        audioSource.volume = targetVolume;
        UpdateVolumeIcon();
    }

    public static void StopMusic()
    {
        if (audioSource == null) return;

        audioSource.Stop();
        audioSource.loop = false;
        audioSource.clip = null;
    }

    public static void FadeOutMusic(float fadeTime = 1f)
    {
        if (Instance == null || audioSource == null) return;

        if (Instance.fadeCoroutine != null)
        {
            Instance.StopCoroutine(Instance.fadeCoroutine);
        }

        Instance.fadeCoroutine = Instance.StartCoroutine(Instance.FadeOutCoroutine(fadeTime));
    }

    private IEnumerator FadeOutCoroutine(float fadeTime)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeTime);
            yield return null;
        }

        audioSource.volume = 0f;
        StopMusic();
    }

    public static void SetVolume(float volume)
    {
        sliderValue = Mathf.Clamp01(volume);

        if (audioSource != null)
        {
            audioSource.volume = sliderValue * currentMaxVolume;
        }

        if (Instance != null)
        {
            Instance.UpdateVolumeIcon();
            Instance.UpdateSliderWithoutEvent();
        }
    }

    public static float GetVolume()
    {
        return sliderValue;
    }

    public void OnValueChanged()
    {
        if (sfxSlider != null)
        {
            SetVolume(sfxSlider.value);
        }
    }

    private void UpdateVolumeIcon()
    {
        if (volumeIcon == null) return;

        if (sliderValue <= 0f)
        {
            volumeIcon.sprite = volumeOffSprite;
        }
        else
        {
            volumeIcon.sprite = volumeOnSprite;
        }
    }

    private void UpdateSliderWithoutEvent()
    {
        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(sliderValue);
        }
    }

    public static AudioClip GetRandomClip(string soundName)
    {
        if (soundEffectLibrary == null)
        {
            Debug.LogWarning("SoundEffectLibrary is missing.");
            return null;
        }

        return soundEffectLibrary.GetRandomClip(soundName);
    }
}
