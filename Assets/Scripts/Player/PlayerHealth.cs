using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    private float health = 0f;

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Slider healthBar;

    [Header("Death")]
    [SerializeField] private float respawnDelay = 5f;
    [SerializeField] private GameObject deathScreen;

    [Header("Death Loading")]
    [SerializeField] private GameObject respawnLoadingPanel;
    [SerializeField] private float audioFadeInTime = 1f;
    [SerializeField] private float audioFadeOutTime = 1f;
    [SerializeField] private float blackScreenDelay = 1f;

    [Header("Death Visual")]
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Sprite deathBloodSprite;

    [Header("Effects")]
    [SerializeField] private GameObject bloodPrefab;

    [Header("References")]
    [SerializeField] private CinemachineCamera virtualCamera;
    public GameObject inventoryPanel;
    public GameObject popupPanel;

    private bool isDead = false;

    private void Start()
    {
        InteractionDialogueManager.UnblockDialogue();

        AudioListener.volume = 0f;
        StartCoroutine(FadeAudioListenerVolume(0f, 1f, audioFadeInTime));

        if (playerSpriteRenderer == null)
        {
            playerSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (playerAnimator == null)
        {
            playerAnimator = GetComponentInChildren<Animator>();
        }

        health = maxHealth;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = maxHealth;
        }

        if (deathScreen != null)
        {
            deathScreen.SetActive(false);
        }

        if (respawnLoadingPanel != null)
        {
            respawnLoadingPanel.SetActive(false);
        }

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }

        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }
    }

    public void UpdateHealth(float mod, Vector2 hitPosition = default)
    {
        if (isDead)
        {
            return;
        }

        health += mod;

        if (mod < 0)
        {
            PlayerController playerController = GetComponent<PlayerController>();

            if (playerController != null)
            {
                playerController.NotifyDamaged();
            }

            float volume = Mathf.Clamp01(Mathf.Abs(mod) / 10f);
            SoundEffectManager.PlayClip("Health", "Hit", volume);

            if (bloodPrefab != null)
            {
                Vector2 spawnPos = hitPosition == default
                    ? (Vector2)transform.position
                    : hitPosition;

                GameObject blood = Instantiate(bloodPrefab, spawnPos, Quaternion.identity);

                SpriteRenderer bloodRenderer = blood.GetComponent<SpriteRenderer>();

                if (bloodRenderer != null)
                {
                    bloodRenderer.sortingLayerName = "Decor";
                }

                Destroy(blood, 0.5f);
            }
        }

        health = Mathf.Clamp(health, 0f, maxHealth);

        if (healthBar != null)
        {
            healthBar.value = health;
        }

        Debug.Log($"Health: {health}");

        if (health <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        PlayerController playerController = GetComponent<PlayerController>();

        if (playerController != null)
        {
            playerController.StopFootsteps();
            playerController.enabled = false;
        }

        if (playerAnimator != null)
        {
            playerAnimator.enabled = false;
        }

        if (playerSpriteRenderer != null && deathBloodSprite != null)
        {
            playerSpriteRenderer.enabled = true;
            playerSpriteRenderer.sprite = deathBloodSprite;

            playerSpriteRenderer.sortingLayerName = "Player";
            playerSpriteRenderer.sortingOrder = 1;
        }

        Collider2D playerCollider = GetComponent<Collider2D>();

        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }

        InteractionDialogueManager.BlockDialogue();

        SoundEffectManager.PlayClip("Health", "Death_Popup", 1f);
        SoundEffectManager.PlayClip("Health", "Player_death", 0.6f);

        if (virtualCamera != null)
        {
            virtualCamera.Follow = null;
            virtualCamera.LookAt = null;
        }

        if (deathScreen != null)
        {
            deathScreen.SetActive(true);
        }

        StartCoroutine(RestartSceneAfterDelay());
    }

    private IEnumerator RestartSceneAfterDelay()
    {
        yield return new WaitForSecondsRealtime(respawnDelay);

        yield return StartCoroutine(FadeAudioListenerVolume(
            AudioListener.volume,
            0f,
            audioFadeOutTime
        ));

        if (respawnLoadingPanel != null)
        {
            respawnLoadingPanel.SetActive(true);

        }

        yield return null;
        yield return new WaitForEndOfFrame();

        yield return new WaitForSecondsRealtime(blackScreenDelay);

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator FadeAudioListenerVolume(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            AudioListener.volume = to;
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            AudioListener.volume = Mathf.Lerp(from, to, timer / duration);
            yield return null;
        }

        AudioListener.volume = to;
    }
}