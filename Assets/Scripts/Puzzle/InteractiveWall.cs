using UnityEngine;

public class InteractiveWall : MonoBehaviour, IInteractable
{
    public GameObject puzzleUI;

    public void Interact()
    {
        Debug.Log("WALL INTERACTED");

        if (puzzleUI != null)
        {
            puzzleUI.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public bool CanInteract()
    {
        return true;
    }
}