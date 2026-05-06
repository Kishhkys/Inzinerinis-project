using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AmbientSoundManager : MonoBehaviour
{
    private static AmbientSoundManager Instance;

    private static AudioSource audioSource;
    private static SoundEffectLibrary soundEffectLibrary;

    [SerializeField] private Slider sfxSlider;

    private static float musicVolume = 0.11f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();
            soundEffectLibrary = GetComponent<SoundEffectLibrary>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void PlayMusic(string soundName, float volume = 0.3f)
    {
        if (soundEffectLibrary == null || audioSource == null)
        {
            Debug.LogWarning("AmbientSoundManager is missing AudioSource or SoundEffectLibrary.");
            return;
        }

        AudioClip audioClip = soundEffectLibrary.GetRandomClip(soundName);

        if (audioClip != null)
        {
            musicVolume = volume;
            audioSource.clip = audioClip;
            audioSource.volume = musicVolume;
            audioSource.loop = true;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("Music clip not found: " + soundName);
        }
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

        Instance.StartCoroutine(Instance.FadeOutCoroutine(fadeTime));
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

    private void Start()
    {
        if (sfxSlider != null)
        {
            sfxSlider.value = musicVolume;
            sfxSlider.onValueChanged.AddListener(delegate { OnValueChanged(); });
        }
    }

    public static void SetVolume(float volume)
    {
        musicVolume = volume;

        if (audioSource != null)
        {
            audioSource.volume = musicVolume;
        }
    }

    public static float GetVolume()
    {
        return musicVolume;
    }

    public void OnValueChanged()
    {
        if (sfxSlider != null)
        {
            SetVolume(sfxSlider.value);
        }
    }
}