using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionDialogueManager : MonoBehaviour
{
    public static InteractionDialogueManager Instance { get; private set; }

    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private Vector2 panelSize = new Vector2(300f, 56f);
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.35f, 0f);
    [SerializeField] private float worldCanvasScale = 0.01f;
    [SerializeField] private float zombieSightRange = 8f;

    private Canvas dialogueCanvas;
    private RectTransform canvasRect;
    private RectTransform panelRect;
    private CanvasGroup canvasGroup;
    private TMP_Text dialogueText;

    private Coroutine hideRoutine;
    private bool checkedZombieSight;
    private bool dialogueBlocked;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateOnSceneLoad()
    {
        if (Instance == null)
        {
            GameObject managerObject = new GameObject("Interaction Dialogue Manager");
            managerObject.AddComponent<InteractionDialogueManager>();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureUI();
    }

    public static void Show(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (Instance == null)
        {
            GameObject managerObject = new GameObject("Interaction Dialogue Manager");
            managerObject.AddComponent<InteractionDialogueManager>();
        }

        Instance.ShowMessage(message);
    }

    public static void ForceHide()
    {
        if (Instance == null)
        {
            return;
        }

        Instance.HideNow();
    }

    public static void BlockDialogue()
    {
        if (Instance == null)
        {
            return;
        }

        Instance.dialogueBlocked = true;
        Instance.HideNow();
    }

    public static void UnblockDialogue()
    {
        if (Instance == null)
        {
            return;
        }

        Instance.dialogueBlocked = false;
    }

    private void Update()
    {
        if (dialogueBlocked)
        {
            return;
        }

        UpdatePanelPosition();
        CheckFirstZombieSight();
    }

    private void ShowMessage(string message)
    {
        if (dialogueBlocked)
        {
            return;
        }

        EnsureUI();

        if (dialogueText == null || canvasGroup == null)
        {
            return;
        }

        dialogueText.text = message;
        dialogueText.ForceMeshUpdate();

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        UpdatePanelPosition();

        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
        }

        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);

        float fadeDuration = 0.35f;

        for (float elapsed = 0f; elapsed < fadeDuration; elapsed += Time.deltaTime)
        {
            if (canvasGroup == null)
            {
                yield break;
            }

            canvasGroup.alpha = 1f - elapsed / fadeDuration;
            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        hideRoutine = null;
    }

    private void HideNow()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (dialogueText != null)
        {
            dialogueText.text = "";
            dialogueText.ForceMeshUpdate();
        }
    }

    private void EnsureUI()
    {
        if (dialogueText != null && canvasGroup != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("Interaction Dialogue Canvas");
        canvasObject.transform.SetParent(transform, false);

        dialogueCanvas = canvasObject.AddComponent<Canvas>();
        dialogueCanvas.renderMode = RenderMode.WorldSpace;
        dialogueCanvas.overrideSorting = true;
        dialogueCanvas.sortingLayerName = "UI";
        dialogueCanvas.sortingOrder = 999;

        canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = panelSize;
        canvasObject.transform.localScale = Vector3.one * worldCanvasScale;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject textObject = new GameObject("Dialogue Text");
        textObject.transform.SetParent(canvasObject.transform, false);

        canvasGroup = textObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        panelRect = textObject.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panelRect.sizeDelta = panelSize;

        dialogueText = textObject.AddComponent<TextMeshProUGUI>();
        dialogueText.alignment = TextAlignmentOptions.Center;
        dialogueText.textWrappingMode = TextWrappingModes.Normal;
        dialogueText.fontSize = 22f;
        dialogueText.color = new Color(0.96f, 0.94f, 0.86f, 1f);
        dialogueText.outlineColor = new Color(0f, 0f, 0f, 0.9f);
        dialogueText.outlineWidth = 0.18f;
        dialogueText.text = "";
    }

    private void UpdatePanelPosition()
    {
        if (canvasRect == null || canvasGroup == null || canvasGroup.alpha <= 0f)
        {
            return;
        }

        PlayerController player = FindFirstObjectByType<PlayerController>();

        if (player == null)
        {
            return;
        }

        canvasRect.position = player.transform.position + worldOffset;
        canvasRect.rotation = Quaternion.identity;

        Camera mainCamera = Camera.main;

        if (dialogueCanvas != null && mainCamera != null)
        {
            dialogueCanvas.worldCamera = mainCamera;
        }
    }

    private void CheckFirstZombieSight()
    {
        if (checkedZombieSight)
        {
            return;
        }

        PlayerController player = FindFirstObjectByType<PlayerController>();
        Camera mainCamera = Camera.main;

        if (player == null || mainCamera == null)
        {
            return;
        }

        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);

        foreach (EnemyAI enemy in enemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
            {
                continue;
            }

            float distance = Vector2.Distance(player.transform.position, enemy.transform.position);

            if (distance > zombieSightRange)
            {
                continue;
            }

            Vector3 viewportPosition = mainCamera.WorldToViewportPoint(enemy.transform.position);

            bool isVisible =
                viewportPosition.z > 0f &&
                viewportPosition.x >= 0f &&
                viewportPosition.x <= 1f &&
                viewportPosition.y >= 0f &&
                viewportPosition.y <= 1f;

            if (isVisible)
            {
                checkedZombieSight = true;
                InteractionDialogueEvents.ZombieSeen(enemy.gameObject);
                return;
            }
        }
    }
}
