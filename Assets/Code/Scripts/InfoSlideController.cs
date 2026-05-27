using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InfoSlideController : MonoBehaviour
{
    private static readonly string[] DefaultSlides =
    {
        "New threat detected.\nSome enemies can use different paths.",
        "Flying enemies ignore ground defenses.\nUse Anti Air towers to stop them.",
        "Plan tower direction before each wave.\nGood placement matters more than spam."
    };

    private string targetScene;
    private string[] slideTexts;
    private int slideIndex;
    private TMP_Text slideText;
    private TMP_Text counterText;
    private Button nextButton;

    public static void Show(string targetSceneName)
    {
        GameObject controllerObject = new GameObject("Info Slides");
        InfoSlideController controller = controllerObject.AddComponent<InfoSlideController>();
        controller.Initialize(targetSceneName, DefaultSlides);
    }

    public void Initialize(string targetSceneName, string[] texts)
    {
        targetScene = targetSceneName;
        slideTexts = texts == null || texts.Length == 0 ? DefaultSlides : texts;
        slideIndex = 0;
        CreateView();
        Refresh();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            LoadTargetScene();
    }

    private void CreateView()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        GameObject backdrop = CreateUIObject("Backdrop", transform);
        Image backdropImage = backdrop.AddComponent<Image>();
        backdropImage.color = new Color(0f, 0f, 0f, 0.78f);
        RectTransform backdropRect = backdrop.GetComponent<RectTransform>();
        backdropRect.anchorMin = Vector2.zero;
        backdropRect.anchorMax = Vector2.one;
        backdropRect.offsetMin = Vector2.zero;
        backdropRect.offsetMax = Vector2.zero;

        GameObject card = CreateUIObject("Slide Card", backdrop.transform);
        Image cardImage = card.AddComponent<Image>();
        cardImage.color = new Color(0.07f, 0.08f, 0.11f, 0.96f);
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.anchoredPosition = Vector2.zero;
        cardRect.sizeDelta = new Vector2(760f, 470f);

        TMP_Text title = CreateText(card.transform, "Title", "Briefing", new Vector2(0f, -62f), new Vector2(680f, 64f), 42f);
        title.fontStyle = FontStyles.Bold;

        slideText = CreateText(card.transform, "Slide Text", string.Empty, new Vector2(0f, -200f), new Vector2(620f, 180f), 30f);
        slideText.lineSpacing = 14f;

        counterText = CreateText(card.transform, "Counter", string.Empty, new Vector2(0f, -320f), new Vector2(250f, 36f), 22f);

        nextButton = CreateButton(card.transform, "Next Button", "Next", new Vector2(0f, -390f), new Color(0.20f, 0.58f, 0.28f, 1f));
        nextButton.onClick.AddListener(NextSlide);
    }

    private void NextSlide()
    {
        if (slideIndex >= slideTexts.Length - 1)
        {
            LoadTargetScene();
            return;
        }

        slideIndex++;
        Refresh();
    }

    private void Refresh()
    {
        slideText.text = slideTexts[slideIndex];
        counterText.text = $"{slideIndex + 1} / {slideTexts.Length}";

        TMP_Text nextText = nextButton.GetComponentInChildren<TMP_Text>();
        if (nextText != null)
            nextText.text = slideIndex >= slideTexts.Length - 1 ? "Continue" : "Next";
    }

    private void LoadTargetScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(targetScene);
    }

    private TMP_Text CreateText(
        Transform parent,
        string objectName,
        string textValue,
        Vector2 anchoredPosition,
        Vector2 size,
        float fontSize)
    {
        GameObject textObject = CreateUIObject(objectName, parent);
        TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = textValue;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.fontSize = fontSize;
        text.raycastTarget = false;

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        return text;
    }

    private Button CreateButton(Transform parent, string objectName, string label, Vector2 anchoredPosition, Color color)
    {
        GameObject buttonObject = CreateUIObject(objectName, parent);
        Image image = buttonObject.AddComponent<Image>();
        image.color = color;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(240f, 54f);

        TMP_Text text = CreateText(buttonObject.transform, "Label", label, Vector2.zero, Vector2.zero, 23f);
        text.fontStyle = FontStyles.Bold;
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }

    private GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }
}
