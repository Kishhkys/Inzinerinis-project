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
        AudioClip audioClip = soundEffectLibrary.GetRandomClip(soundName);
        if (audioClip != null)
        {
 
            audioSource.PlayOneShot(audioClip, volume * sfxVolume);
        }
    }


    public static float PlayClip(string groupName, string soundName, float volume = 1f, bool lockMovement = false, float shortenLength = 0f, Vector3? faceTargetPosition = null)
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

        Debug.LogWarning($"Sound clip not found: {groupName}/{soundName}");
        return 0f;
    }


    private void Start()
    {
        sfxSlider.onValueChanged.AddListener(delegate { OnValueChanged(); });
    }

 
    public static void SetVolume(float volume)
    {
        sfxVolume = volume;
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
}
