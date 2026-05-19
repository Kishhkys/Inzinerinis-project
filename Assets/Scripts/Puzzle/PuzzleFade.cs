using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class PuzzleFade : MonoBehaviour
{
    private Image img;

    void OnEnable()
    {
        TryGetComponent(out img);
        Color c = img.color;
        img.color = new(c.r, c.g, c.b, 1f);
    }
}
