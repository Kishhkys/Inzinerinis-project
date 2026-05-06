using UnityEngine;

public class PanelBlink : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float blinkSpeed = 3f;
    [SerializeField] private float minAlpha = 0.25f;
    [SerializeField] private float maxAlpha = 1f;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void OnEnable()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = maxAlpha;
        }
    }

    private void Update()
    {
        if (canvasGroup == null) return;

        float blink = Mathf.PingPong(Time.unscaledTime * blinkSpeed, 1f);
        canvasGroup.alpha = Mathf.Lerp(minAlpha, maxAlpha, blink);
    }

    private void OnDisable()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = maxAlpha;
        }
    }
}