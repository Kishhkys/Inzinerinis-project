using System.Collections;
using UnityEngine;

public class Boxes : MonoBehaviour, IInteractable
{
    public bool IsOpened { get; private set; }
    public string BoxID { get; private set; }

    [Header("Item Spawn")]
    public GameObject itemPrefab;
    public Vector3 itemSpawnOffset = Vector3.down;
    public float itemSpawnDelay = 0.5f;

    private void Start()
    {
        BoxID ??= GlobalHelper.GenerateUniqueId(gameObject);

    }

    public bool CanInteract()
    {
        return !IsOpened;
    }

    public void Interact()
    {
        if (!CanInteract()) return;
        OpenBox();
    }

    private void OpenBox()
    {
        SoundEffectManager.PlayClip("Elevator", "Cardboard_box", 2f, true, 1f, transform.position);
        IsOpened = true;
        StartCoroutine(SpawnItemAfterDelay());
    }

    private IEnumerator SpawnItemAfterDelay()
    {
        yield return new WaitForSeconds(itemSpawnDelay);

        if (itemPrefab != null)
        {
            Instantiate(itemPrefab, transform.position + itemSpawnOffset, Quaternion.identity);
        }
    }
}
