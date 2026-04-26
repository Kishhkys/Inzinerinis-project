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

    [Header("Optional Level Complete")]
    public bool loadNextSceneOnEnter = true;
    public string nextSceneName;
    public float levelCompleteDelay = 4f;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool isOpening = false;
    [SerializeField] private GameObject levelCompleteScreen;

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

        if (spriteRenderer != null && closedSprite != null)
        {
            spriteRenderer.sprite = closedSprite;
        }

        if (elevatorLightRenderer != null && brokenLightSprite != null)
        {
            elevatorLightRenderer.sprite = brokenLightSprite;
        }
    }

    public bool CanInteract()
    {
        return !isOpened && !isOpening;
    }

    public void Interact()
    {
        Debug.Log("INTERACTED WITH ELEVATOR: " + gameObject.name);
        if (!CanInteract()) return;
        SoundEffectManager.PlayClip("Elevator", "Elevator_button_press", 0.5f);
        if (!isRepaired)
        {
            Debug.Log("Elevator not working");

            return;
        }

        StartCoroutine(OpenElevator());
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
        isOpened = true;

        if (spriteRenderer != null && openSprite != null)
        {
            spriteRenderer.sprite = openSprite;
        }

        if (animator != null)
        {
            animator.SetBool("setOpened", true);
        }

        SoundEffectManager.PlayClip("Elevator", "Elevator_open", 0.5f);

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