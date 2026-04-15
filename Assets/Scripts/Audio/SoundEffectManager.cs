using UnityEngine;
using UnityEngine.UI;

public class SoundEffectManager : MonoBehaviour
{
    private static SoundEffectManager instance;

    private static AudioSource audioSource;
    private static SoundEffectLibrary soundEffectLibrary;
    [SerializeField] private Slider sfxSlider;

    private static float sfxVolume = 1f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            audioSource = GetComponent<AudioSource>();
            soundEffectLibrary = GetComponent<SoundEffectLibrary>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void PlayRandomClip(string soundName, float volume = 1f)
    {
        if (soundEffectLibrary == null || audioSource == null)
        {
            return;
        }

        AudioClip audioClip = soundEffectLibrary.GetRandomClip(soundName);
        if (audioClip != null)
        {
 
            audioSource.PlayOneShot(audioClip, volume * sfxVolume);
        }
    }

    public static void PlayClip(string groupName, string soundName, float volume = 1f)
    {
        if (soundEffectLibrary == null || audioSource == null)
        {
            return;
        }

        AudioClip audioClip = soundEffectLibrary.GetClip(groupName, soundName);
        if (audioClip != null)
        {
            audioSource.PlayOneShot(audioClip, volume * sfxVolume);
        }
    }

    private void Start()
    {
        sfxSlider.onValueChanged.AddListener(delegate { OnValueChanged(); });
    }

 
    public static void SetVolume(float volume)
    {
        sfxVolume = volume;
    }

    public void OnValueChanged()
    {
        sfxVolume = sfxSlider.value;
    }
}
