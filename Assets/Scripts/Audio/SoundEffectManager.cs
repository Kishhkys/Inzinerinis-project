using UnityEngine;
using UnityEngine.UI;

public class SoundEffectManager : MonoBehaviour
{
    private static SoundEffectManager instance;

    private static AudioSource audioSource;
    private static SoundEffectLibrary soundEffectLibrary;

    [SerializeField] private Slider sfxSlider;

    [Header("Volume Icon")]
    [SerializeField] private Image volumeIcon;
    [SerializeField] private Sprite volumeOnSprite;
    [SerializeField] private Sprite volumeOffSprite;

    private static float sfxVolume = 1f;

    private void Awake()
    {
        instance = this;

        audioSource = GetComponent<AudioSource>();
        soundEffectLibrary = GetComponent<SoundEffectLibrary>();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
            audioSource = null;
            soundEffectLibrary = null;
        }
    }

    private void Start()
    {
        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.SetValueWithoutNotify(sfxVolume);
            sfxSlider.onValueChanged.AddListener(delegate { OnValueChanged(); });
        }

        UpdateVolumeIcon();
    }

    public static void PlayRandomClip(string soundName, float volume = 1f)
    {
        if (soundEffectLibrary == null || audioSource == null)
        {
            Debug.LogWarning("SoundEffectManager is missing AudioSource or SoundEffectLibrary.");
            return;
        }

        AudioClip audioClip = soundEffectLibrary.GetRandomClip(soundName);

        if (audioClip != null)
        {
            audioSource.PlayOneShot(audioClip, volume * sfxVolume);
        }
        else
        {
            Debug.LogWarning("Random sound clip not found: " + soundName);
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

    public static float PlayClip(
        string groupName,
        string soundName,
        float volume = 1f,
        bool lockMovement = false,
        float shortenLength = 0f,
        Vector3? faceTargetPosition = null
    )
    {
        if (soundEffectLibrary == null || audioSource == null)
        {
            Debug.LogWarning("SoundEffectManager is missing AudioSource or SoundEffectLibrary.");
            return 0f;
        }

        AudioClip audioClip = soundEffectLibrary.GetClip(groupName, soundName);

        if (audioClip != null)
        {
            audioSource.PlayOneShot(audioClip, volume * sfxVolume);

            float duration = Mathf.Max(0f, audioClip.length - shortenLength);

            if (lockMovement && duration > 0f)
            {
                PlayerController playerMovement = Object.FindFirstObjectByType<PlayerController>();

                if (playerMovement != null)
                {
                    if (faceTargetPosition.HasValue)
                    {
                        playerMovement.LockMovementFor(duration, faceTargetPosition.Value);
                    }
                    else
                    {
                        playerMovement.LockMovementFor(duration);
                    }
                }
            }

            return duration;
        }

        Debug.LogWarning("Sound clip not found: " + groupName + "/" + soundName);
        return 0f;
    }

    public static void SetVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);

        if (instance != null)
        {
            instance.UpdateVolumeIcon();
            instance.UpdateSliderWithoutEvent();
        }
    }

    public static float GetVolume()
    {
        return sfxVolume;
    }

    public void OnValueChanged()
    {
        if (sfxSlider != null)
        {
            SetVolume(sfxSlider.value);
        }
    }

    private void UpdateSliderWithoutEvent()
    {
        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(sfxVolume);
        }
    }

    private void UpdateVolumeIcon()
    {
        if (volumeIcon == null) return;

        if (sfxVolume <= 0f)
        {
            volumeIcon.sprite = volumeOffSprite;
        }
        else
        {
            volumeIcon.sprite = volumeOnSprite;
        }
    }
}