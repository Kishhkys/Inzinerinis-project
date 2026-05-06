using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public bool isOpened { get; private set; }
    public string ChestID { get; private set; }

    public Sprite defaultSprite;
    public Sprite lockedSprite;
    public Sprite openedSprite;
    public string requiredKeyID = null;

    [SerializeField] private Transform teleportPoint;
    [SerializeField] private float teleportDelay = 1f;

    private bool isLocked = false;
    private bool isUnlocking = false;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        ChestID ??= GlobalHelper.GenerateUniqueId(gameObject);

        if (!string.IsNullOrEmpty(requiredKeyID))
        {
            isLocked = true;

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = lockedSprite;
            }
        }
    }

    public bool CanInteract()
    {
        return !isOpened && !isUnlocking;
    }

    public void Interact()
    {
        if (!CanInteract())
        {
            return;
        }

        InventoryController inventory = FindFirstObjectByType<InventoryController>();
        if (inventory == null)
        {
            return;
        }

        Item selectedItem = inventory.GetSelectedItem();

        if (isLocked)
        {
            KeyItem key = selectedItem as KeyItem;
            bool hasMatchingKey = key != null && key.targetID == requiredKeyID;

            InteractionDialogueEvents.LockedDoorChecked(hasMatchingKey);

            if (hasMatchingKey)
            {
                isUnlocking = true;
                key.UseKey(transform.position);
                inventory.RemoveSelectedItem();
                StartCoroutine(UnlockAndOpenDoor());
            }
            else
            {
                float duration = SoundEffectManager.PlayClip(
                    "Door",
                    "Door_locked",
                    0.3f,
                    true,
                    0f,
                    transform.position
                );

                StartCoroutine(BlockInteractionFor(duration));
            }

            return;
        }

        StartCoroutine(OpenAndTeleport());
    }

    private IEnumerator UnlockAndOpenDoor()
    {
        isLocked = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = defaultSprite;
        }

        yield return new WaitForSeconds(3f);

        yield return StartCoroutine(OpenAndTeleport());
    }

    private IEnumerator OpenAndTeleport()
    {
        isUnlocking = true;

        SetOpened(true);

        if (animator != null)
        {
            animator.SetBool("setOpened", true);
        }

        float duration = SoundEffectManager.PlayClip(
            "Door",
            "Door_open",
            0.3f,
            true,
            0f,
            transform.position
        );

        yield return new WaitForSeconds(duration);
        yield return new WaitForSeconds(teleportDelay);

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null && teleportPoint != null)
        {
            player.Teleport(teleportPoint.position, 0.2f);
        }

        isUnlocking = false;
    }

    private IEnumerator BlockInteractionFor(float duration)
    {
        isUnlocking = true;

        yield return new WaitForSeconds(duration);

        isUnlocking = false;
    }

    public void SetOpened(bool opened)
    {
        isOpened = opened;

        if (isOpened && spriteRenderer != null)
        {
            spriteRenderer.sprite = openedSprite;
        }
    }

    public bool IsUnlocked()
    {
        return !isLocked;
    }
}