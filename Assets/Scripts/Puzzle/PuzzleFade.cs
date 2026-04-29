using UnityEngine;
using UnityEngine.UI;

public class PuzzleFade : MonoBehaviour
{
    private Image img;

    void OnEnable()
    {
        img = GetComponent<Image>();
        Color c = img.color;
        img.color = new Color(c.r, c.g, c.b, 1f);
        Debug.Log("FADE SCRIPT TRIGGERED");
    }
}