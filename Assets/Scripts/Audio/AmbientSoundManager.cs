using UnityEngine;
using UnityEngine.UI;

public class AmbientSoundManager : MonoBehaviour
{
    private static AmbientSoundManager Instance;

    private static AudioSource audioSource;
    private static SoundEffectLibrary soundEffectLibrary;
    [SerializeField] private Slider sfxSlider;

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
        AudioClip audioClip = soundEffectLibrary.GetRandomClip(soundName);
        if (audioClip != null)
        {
            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public static void StopMusic()
    {
        audioSource.Stop();
        audioSource.loop = false;
        audioSource.clip = null;
    }


    private void Start()
    {

        sfxSlider.onValueChanged.AddListener(delegate { OnValueChanged(); });
        PlayMusic("Ambience", 0.11f);
    }

    public static void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }

    public void OnValueChanged()
    {
        SetVolume(sfxSlider.value);
    }
}
