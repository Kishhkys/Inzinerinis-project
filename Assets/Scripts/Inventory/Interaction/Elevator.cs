using UnityEngine;
using UnityEngine.SceneManagement;

public class Elevator : MonoBehaviour, IInteractable
{
    [Header("Elevator State")]
    public bool isOpen = false;

    [Header("Sprites")]
    public Sprite closedSprite;
    public Sprite openSprite;

    [Header("Elevator Light")]
    public SpriteRenderer elevatorLightRenderer;
    public Sprite brokenLightSprite;
    public Sprite repairedLightSprite;

    [Header("Optional Level Complete")]
    public bool loadNextSceneOnEnter = true;
    public string nextSceneName;

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        SetOpen(false);
    }

    public void SetOpen(bool open)
    {
        isOpen = open;

        if (spriteRenderer != null)
        {
            if (isOpen && openSprite != null)
            {
                spriteRenderer.sprite = openSprite;
            }
            else if (!isOpen && closedSprite != null)
            {
                spriteRenderer.sprite = closedSprite;
            }
        }

        UpdateElevatorLight();
    }

    private void UpdateElevatorLight()
    {
        if (elevatorLightRenderer == null) return;

        if (isOpen)
        {
            if (repairedLightSprite != null)
            {
                elevatorLightRenderer.sprite = repairedLightSprite;
            }
        }
        else
        {
            if (brokenLightSprite != null)
            {
                elevatorLightRenderer.sprite = brokenLightSprite;
            }
        }
    }

    public bool CanInteract()
    {
        return isOpen;
    }

    public void Interact()
    {
        if (!isOpen)
        {
            Debug.Log("Elevator not working");
            return;
        }

        if (animator != null)
        {
            animator.SetBool("setOpened", true);
        }

        Debug.Log("Elevator used");

        if (loadNextSceneOnEnter)
        {
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.Log("Level Complete!");
            }
        }
    }
}