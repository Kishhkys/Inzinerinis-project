using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Elevator : MonoBehaviour, IInteractable
{
    [Header("Elevator State")]
    public bool isRepaired = false;
    public bool isOpened { get; private set; }

    [Header("Sprites")]
    public Sprite closedSprite;
    public Sprite openSprite;

    [Header("Elevator Light")]
    public SpriteRenderer elevatorLightRenderer;
    public Sprite brokenLightSprite;
    public Sprite repairedLightSprite;
    public float lightChangeDelay = 2f;

    [Header("Level Complete")]
    public string nextSceneName;
    public float levelCompleteDelay = 4f;

    [SerializeField] private GameObject levelCompleteScreen;

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private bool isOpening = false;
    private bool levelCompleteStarted = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        isRepaired = false;
        isOpened = false;
        isOpening = false;
        levelCompleteStarted = false;

        if (spriteRenderer != null && closedSprite != null)
        {
            spriteRenderer.sprite = closedSprite;
        }

        if (elevatorLightRenderer != null && brokenLightSprite != null)
        {
            elevatorLightRenderer.sprite = brokenLightSprite;
        }

        if (levelCompleteScreen != null)
        {
            levelCompleteScreen.SetActive(false);
        }
    }

    public bool CanInteract()
    {
        return isRepaired && !isOpening && !levelCompleteStarted;
    }

    public void Interact()
    {
        if (!CanInteract()) return;

        SoundEffectManager.PlayClip("Elevator", "Elevator_button_press", 0.5f);

        if (!isOpened)
        {
            StartCoroutine(OpenElevator());
            return;
        }

        StartCoroutine(LevelCompleteRoutine());
    }

    public void SetOpen(bool repaired)
    {
        isRepaired = repaired;

        if (isRepaired)
        {
            StartCoroutine(ChangeLightAfterDelay());
        }
        else
        {
            if (elevatorLightRenderer != null && brokenLightSprite != null)
            {
                elevatorLightRenderer.sprite = brokenLightSprite;
            }
        }
    }

    private IEnumerator ChangeLightAfterDelay()
    {
        yield return new WaitForSeconds(lightChangeDelay);

        if (elevatorLightRenderer != null && repairedLightSprite != null)
        {
            SoundEffectManager.PlayClip("Elevator", "Elevator_light_switch", 0.5f);
            elevatorLightRenderer.sprite = repairedLightSprite;
        }
    }

    private IEnumerator OpenElevator()
    {
        isOpening = true;

        if (animator != null)
        {
            animator.SetBool("setOpened", true);
        }

        SoundEffectManager.PlayClip("Elevator", "Elevator_open", 0.5f);

        yield return new WaitForSeconds(1f);

        isOpened = true;
        isOpening = false;

        if (spriteRenderer != null && openSprite != null)
        {
            spriteRenderer.sprite = openSprite;
        }

        Debug.Log("Elevator opened. Interact again to finish level.");
    }

    private IEnumerator LevelCompleteRoutine()
    {
        levelCompleteStarted = true;

        if (levelCompleteScreen != null)
        {
            levelCompleteScreen.SetActive(true);
        }

        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(levelCompleteDelay);

        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.Log("Next scene name is empty!");
        }
    }
}