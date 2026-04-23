using UnityEngine;

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Door : MonoBehaviour, IInteractable
{
    public bool isOpened { get; private set; }
    public string ChestID { get; private set; }
    public Sprite defaultSprite;
    public Sprite lockedSprite;
    public Sprite openedSprite;
    public string requiredKeyID = null;

    private bool isLocked = false;
    private bool isUnlocking = false;
    private Animator animator;
    [SerializeField] private Transform teleportPoint;
    PlayerController player;

    void Awake()
    {
        player = FindFirstObjectByType<PlayerController>();
        animator = GetComponent<Animator>();
    }

    public bool CanInteract()
    {
            return !isOpened && !isUnlocking;
        }

    public void Interact()
    {
        if (!CanInteract()) return;

        InventoryController inventory = FindFirstObjectByType<InventoryController>();
        if (inventory == null) return;

        Item selectedItem = inventory.GetSelectedItem();

        if (isLocked)
        {
            if (selectedItem is KeyItem key && key.targetID == requiredKeyID)
            {
                isUnlocking = true;
                key.UseKey();
                inventory.RemoveSelectedItem();
                StartCoroutine(UnlockAndOpenDoor());
            }
            else
            {
                SoundEffectManager.PlayClip("Door", "Door_locked", 0.5f);
            }

            return;
        }

        OpenDoor();
    }

    private IEnumerator UnlockAndOpenDoor()
    {
        isLocked = false;
        GetComponent<SpriteRenderer>().sprite = defaultSprite;
        yield return new WaitForSeconds(3f);
        OpenDoor();
    }

    private void OpenDoor()
    {
        SetOpened(true);
        isUnlocking = false;
        animator.SetBool("setOpened", true);
        SoundEffectManager.PlayClip("Door", "Door_open", 0.5f);
        player.Teleport(teleportPoint.position);
        
    }

    public void SetOpened(bool opened)
    {
        isOpened = opened;

        if (isOpened)
        {
            GetComponent<SpriteRenderer>().sprite = openedSprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isOpened) return;

        if (collision.CompareTag("Player"))
        {

            if (player != null && teleportPoint != null)
            {
                player.Teleport(teleportPoint.position);
            }
        }
    }


    void Start()
    {
        ChestID ??= GlobalHelper.GenerateUniqueId(gameObject);

        if (!string.IsNullOrEmpty(requiredKeyID))
        {
            isLocked = true;
            GetComponent<SpriteRenderer>().sprite = lockedSprite;
        }
    }

}
