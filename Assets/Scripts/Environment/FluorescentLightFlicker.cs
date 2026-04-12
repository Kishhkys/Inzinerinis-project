using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FluorescentLightFlicker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light2D targetLight;
    [SerializeField] private SpriteRenderer fixtureSprite;
    [SerializeField] private AudioSource audioSource;

    [Header("Light")]
    [SerializeField] private float baseIntensity = 0.9f;
    [SerializeField] private float dimIntensity = 0.2f;
    [SerializeField] private float baseSpriteAlpha = 1f;
    [SerializeField] private float dimSpriteAlpha = 0.45f;

    [Header("Timing")]
    [SerializeField] private Vector2 stableDurationRange = new Vector2(3f, 8f);
    [SerializeField] private Vector2 flickerDurationRange = new Vector2(0.15f, 0.8f);
    [SerializeField] private Vector2 flickerStepDurationRange = new Vector2(0.03f, 0.08f);
    [SerializeField] private Vector2 offChanceRange = new Vector2(0.2f, 0.45f);

    [Header("Audio")]
    [SerializeField] private AudioClip ambientBuzzClip;
    [SerializeField] private float ambientBuzzVolume = 0.15f;
    [SerializeField] private AudioClip[] flickerClips;
    [SerializeField] private float flickerClipVolume = 0.35f;

    private float stateTimer;
    private float flickerTimer;
    private bool isFlickering;
    private Color fixtureColor;

    private void Awake()
    {
        if (targetLight == null)
        {
            targetLight = GetComponent<Light2D>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (fixtureSprite != null)
        {
            fixtureColor = fixtureSprite.color;
        }
    }

    private void Start()
    {
        ApplyVisualState(baseIntensity, baseSpriteAlpha);
        StartStableState();
        StartAmbientBuzz();
    }

    private void Update()
    {
        stateTimer -= Time.deltaTime;

        if (!isFlickering)
        {
            if (stateTimer <= 0f)
            {
                StartFlickerState();
            }

            return;
        }

        flickerTimer -= Time.deltaTime;

        if (flickerTimer <= 0f)
        {
            bool lightsOff = Random.value < Random.Range(offChanceRange.x, offChanceRange.y);
            ApplyVisualState(lightsOff ? dimIntensity : baseIntensity, lightsOff ? dimSpriteAlpha : baseSpriteAlpha);
            PlayRandomFlickerClip();
            flickerTimer = Random.Range(flickerStepDurationRange.x, flickerStepDurationRange.y);
        }

        if (stateTimer <= 0f)
        {
            ApplyVisualState(baseIntensity, baseSpriteAlpha);
            StartStableState();
        }
    }

    private void StartStableState()
    {
        isFlickering = false;
        stateTimer = Random.Range(stableDurationRange.x, stableDurationRange.y);
    }

    private void StartFlickerState()
    {
        isFlickering = true;
        stateTimer = Random.Range(flickerDurationRange.x, flickerDurationRange.y);
        flickerTimer = 0f;
    }

    private void ApplyVisualState(float lightIntensity, float spriteAlpha)
    {
        if (targetLight != null)
        {
            targetLight.intensity = lightIntensity;
        }

        if (fixtureSprite != null)
        {
            Color color = fixtureColor;
            color.a = spriteAlpha;
            fixtureSprite.color = color;
        }
    }

    private void StartAmbientBuzz()
    {
        if (audioSource == null || ambientBuzzClip == null)
        {
            return;
        }

        audioSource.loop = true;
        audioSource.clip = ambientBuzzClip;
        audioSource.volume = ambientBuzzVolume;
        audioSource.Play();
    }

    private void PlayRandomFlickerClip()
    {
        if (audioSource == null || flickerClips == null || flickerClips.Length == 0)
        {
            return;
        }

        AudioClip clip = flickerClips[Random.Range(0, flickerClips.Length)];
        if (clip != null)
        {
            audioSource.PlayOneShot(clip, flickerClipVolume);
        }
    }
}
