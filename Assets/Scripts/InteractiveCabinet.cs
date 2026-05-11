using UnityEngine;

public class InteractiveCabinet : MonoBehaviour, IInteractable
{
    public GameObject patientFilePrefab;
    public Transform spawnPoint;

    private bool used = false;

    public void Interact()
    {
        if (used) return;

        Instantiate(patientFilePrefab, spawnPoint.position, Quaternion.identity);
        used = true;
    }

    public bool CanInteract() => !used;
}