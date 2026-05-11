using UnityEngine;

public class InteractiveBed : MonoBehaviour, IInteractable
{
    public GameObject hiddenKeycard;
    public bool hasBeenSearched = false;

    public bool CanInteract()
    {
        return !hasBeenSearched;
    }

    public void Interact()
    {
        if (!CanInteract()) return;

        hasBeenSearched = true;

        if (hiddenKeycard != null)
        {
            hiddenKeycard.SetActive(true);
        }

        Debug.Log("You found something under the bed.");
    }

    private void Start()
    {
        if (hiddenKeycard != null)
        {
            hiddenKeycard.SetActive(false);
        }
    }
}