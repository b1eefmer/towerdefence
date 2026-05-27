using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VolumeSettingsPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private const float CloseDelay = 10f;

    private GameObject canvasObject;
    private GameObject modalRoot;
    private Slider musicSlider;
    private Slider sfxSlider;
    private TMP_Text musicValueText;
    private TMP_Text sfxValueText;
    private TMP_Text closeButtonText;
    private float closeTimer;

    public void Initialize(bool visible)
    {
        if (modalRoot == null)
            CreateView();

        SetVisible(visible);
        RefreshFromSettings();
        VolumeSettings.ApplyAllSceneVolumes();
    }

    public void SetVisible(bool visible)
    {
        if (canvasObject != null)
            canvasObject.SetActive(visible);

        if (!visible)
            Close();
    }

    public void Open()
    {
        if (modalRoot == null)
            CreateView();

        if (canvasObject != null)
            canvasObject.SetActive(true);

        modalRoot.SetActive(true);
        closeTimer = CloseDelay;
        RefreshFromSettings();
        RefreshCloseText();
    }

    public void Close()
    {
        if (modalRoot != null)
            modalRoot.SetActive(false);

        ClearHoverState();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (UIManager.main != null)
            UIManager.main.SetHoveringState(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (UIManager.main != null)
            UIManager.main.SetHoveringState(false);
    }

    private void OnDisable()
    {
        ClearHoverState();
    }

    private void Update()
    {
        if (modalRoot == null || !modalRoot.activeSelf)
            return;

        closeTimer -= Time.unscaledDeltaTime;
        if (closeTimer <= 0f)
        {
            Close();
            return;
        }

        RefreshCloseText();
    }

    private void CreateView()
    {
        canvasObject = new GameObject("Volume Settings UI");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 120;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        modalRoot = CreateUIObject("Volume Modal", canvasObject.transform);
        Image backdrop = modalRoot.AddComponent<Image>();
        backdrop.color = new Color(0f, 0f, 0f, 0.42f);
        RectTransform backdropRect = modalRoot.GetComponent<RectTransform>();
        backdropRect.anchorMin = Vector2.zero;
        backdropRect.anchorMax = Vector2.one;
        backdropRect.offsetMin = Vector2.zero;
        backdropRect.offsetMax = Vector2.zero;

        GameObject panel = CreateUIObject("Volume Panel", modalRoot.transform);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.07f, 0.08f, 0.11f, 0.96f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(520f, 300f);

        CreateTitle(panel.transform);
        CreateVolumeRow(panel.transform, "Music", -78f, out musicSlider, out musicValueText);
        CreateVolumeRow(panel.transform, "SFX", -142f, out sfxSlider, out sfxValueText);

        Button closeButton = CreateButton(
            panel.transform,
            "Close Button",
            "Close",
            new Vector2(0f, -232f),
            new Vector2(240f, 50f),
            new Color(0.60f, 0.22f, 0.22f, 1f));
        closeButton.onClick.AddListener(Close);
        closeButtonText = closeButton.GetComponentInChildren<TMP_Text>();

        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        modalRoot.SetActive(false);
    }

    private void CreateTitle(Transform parent)
    {
        TMP_Text title = CreateText(parent, "Title", "Audio Settings", new Vector2(0f, -30f), new Vector2(430f, 42f), 30f);
        title.fontStyle = FontStyles.Bold;
    }

    private void CreateVolumeRow(
        Transform parent,
        string label,
        float y,
        out Slider slider,
        out TMP_Text valueText)
    {
        CreateText(parent, label + " Label", label, new Vector2(-180f, y), new Vector2(110f, 32f), 22f);
        valueText = CreateText(parent, label + " Value", "100%", new Vector2(186f, y), new Vector2(90f, 32f), 20f);

        GameObject sliderObject = CreateUIObject(label + " Slider", parent);
        RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 1f);
        sliderRect.anchorMax = new Vector2(0.5f, 1f);
        sliderRect.pivot = new Vector2(0.5f, 0.5f);
        sliderRect.anchoredPosition = new Vector2(0f, y);
        sliderRect.sizeDelta = new Vector2(240f, 26f);

        slider = sliderObject.AddComponent<Slider>();
        slider.minValue = 0.0001f;
        slider.maxValue = 1f;
        slider.value = 1f;

        Image background = CreateImage("Background", sliderObject.transform, new Color(0.02f, 0.025f, 0.035f, 1f));
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0.25f);
        backgroundRect.anchorMax = new Vector2(1f, 0.75f);
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        Image fill = CreateImage("Fill", sliderObject.transform, new Color(0.20f, 0.58f, 0.28f, 1f));
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0.25f);
        fillRect.anchorMax = new Vector2(1f, 0.75f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image handle = CreateImage("Handle", sliderObject.transform, new Color(0.9f, 0.95f, 1f, 1f));
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(24f, 24f);

        slider.targetGraphic = handle;
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
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

    private Image CreateImage(string objectName, Transform parent, Color color)
    {
        GameObject imageObject = CreateUIObject(objectName, parent);
        Image image = imageObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private Button CreateButton(
        Transform parent,
        string objectName,
        string label,
        Vector2 anchoredPosition,
        Vector2 size,
        Color color)
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
        rect.sizeDelta = size;

        TMP_Text text = CreateText(buttonObject.transform, "Label", label, Vector2.zero, Vector2.zero, 22f);
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

    private void RefreshFromSettings()
    {
        musicSlider.SetValueWithoutNotify(VolumeSettings.MusicVolume);
        sfxSlider.SetValueWithoutNotify(VolumeSettings.SfxVolume);
        RefreshValueTexts();
    }

    private void OnMusicChanged(float value)
    {
        VolumeSettings.SetMusicVolume(value);
        RefreshValueTexts();
    }

    private void OnSfxChanged(float value)
    {
        VolumeSettings.SetSfxVolume(value);
        RefreshValueTexts();
    }

    private void RefreshValueTexts()
    {
        musicValueText.text = Mathf.RoundToInt(VolumeSettings.MusicVolume * 100f) + "%";
        sfxValueText.text = Mathf.RoundToInt(VolumeSettings.SfxVolume * 100f) + "%";
    }

    private void RefreshCloseText()
    {
        if (closeButtonText != null)
            closeButtonText.text = $"Close ({Mathf.CeilToInt(closeTimer)})";
    }

    private void ClearHoverState()
    {
        if (UIManager.main != null)
            UIManager.main.SetHoveringState(false);
    }
}
