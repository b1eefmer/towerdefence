using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UpgradeUIHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private static UpgradeUIHandler instance;

    public bool mouse_over = false;

    private PlacedTower tower;
    private GameObject panel;
    private TMP_Text levelText;
    private TMP_Text upgradeText;
    private TMP_Text sellText;
    private Button upgradeButton;
    private Button sellButton;
    private Button closeButton;
    private TMP_Text closeButtonText;
    private float closeTimer;
    private const float CloseDelay = 10f;

    public static UpgradeUIHandler Create(Transform parent)
    {
        if (instance != null)
            return instance;

        GameObject canvasObject = new GameObject("Tower Action UI");
        if (BuildMananger.main != null)
            canvasObject.transform.SetParent(BuildMananger.main.transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 101;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject panelObject = CreateUIObject("Tower Action Panel", canvasObject.transform);
        panelObject.transform.SetParent(canvasObject.transform, false);
        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.11f, 0.16f, 0.90f); // Glassmorphism dark background

        panelObject.AddComponent<CanvasGroup>(); // Для Fade-анімації

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 24f);
        panelRect.sizeDelta = new Vector2(720f, 150f);

        UpgradeUIHandler handler = canvasObject.AddComponent<UpgradeUIHandler>();
        handler.panel = panelObject;
        handler.levelText = handler.CreateText(panelObject.transform, "Level Text", new Vector2(-220f, 82f), new Vector2(210f, 32f), 24f);
        handler.upgradeText = handler.CreateText(panelObject.transform, "Upgrade Text", new Vector2(0f, 82f), new Vector2(240f, 32f), 24f);
        handler.sellText = handler.CreateText(panelObject.transform, "Sell Text", new Vector2(230f, 82f), new Vector2(210f, 32f), 24f);
        
        // Оновлені кольори кнопок під новий дизайн
        handler.upgradeButton = handler.CreateButton(
            panelObject.transform,
            "Upgrade Button",
            "Upgrade",
            new Vector2(-210f, 22f),
            new Color(0f, 0.75f, 0.85f, 1f)); // Primary Cyan
            
        handler.sellButton = handler.CreateButton(
            panelObject.transform,
            "Sell Button",
            "Sell",
            new Vector2(0f, 22f),
            new Color(0.15f, 0.25f, 0.45f, 1f)); // Secondary Blue
            
        handler.closeButton = handler.CreateButton(
            panelObject.transform,
            "Close Button",
            "Close",
            new Vector2(210f, 22f),
            new Color(0.60f, 0.22f, 0.22f, 1f)); // Destructive Red
            
        handler.closeButtonText = handler.closeButton.GetComponentInChildren<TMP_Text>();

        handler.upgradeButton.onClick.AddListener(handler.Upgrade);
        handler.sellButton.onClick.AddListener(handler.Sell);
        handler.closeButton.onClick.AddListener(handler.Close);
        canvasObject.SetActive(false);

        instance = handler;
        return handler;
    }

    private void OnEnable()
    {
        if (panel != null)
        {
            CanvasGroup group = panel.GetComponent<CanvasGroup>();
            if (group != null) StartCoroutine(UIAnimator.FadeIn(group, 0.2f));

            RectTransform rect = panel.GetComponent<RectTransform>();
            if (rect != null) StartCoroutine(UIAnimator.ScaleIn(rect, 0.35f));
        }
    }

    public void Bind(PlacedTower placedTower)
    {
        tower = placedTower;
        closeTimer = CloseDelay;
        Refresh();
    }

    public void Refresh()
    {
        if (tower == null)
            return;

        levelText.text = $"Level: {tower.GetLevel()}";
        upgradeText.text = $"Upgrade: {tower.GetUpgradeCost()}g";
        sellText.text = $"Sell: {tower.GetSellValue()}g";
        upgradeButton.interactable = tower.CanUpgrade();
        RefreshCloseText();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouse_over = true;
        UIManager.main.SetHoveringState(true);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        ClearHoverState();
    }

    private void OnDisable()
    {
        ClearHoverState();
    }

    private void Update()
    {
        if (tower == null)
            return;

        closeTimer -= Time.unscaledDeltaTime;
        if (closeTimer <= 0f)
        {
            Close();
            return;
        }

        RefreshCloseText();
    }

    private TMP_Text CreateText(
        Transform parent,
        string objectName,
        Vector2 anchoredPosition,
        Vector2 size,
        float fontSize)
    {
        GameObject textObject = CreateUIObject(objectName, parent);
        textObject.transform.SetParent(parent, false);

        TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.raycastTarget = false;

        textObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.5f);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        return text;
    }

    private Button CreateButton(
        Transform parent,
        string objectName,
        string label,
        Vector2 anchoredPosition,
        Color color)
    {
        GameObject buttonObject = CreateUIObject(objectName, parent);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.02f, 0.05f, 0.1f, 0.55f); // Магічне скло

        Sprite roundedSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        if (roundedSprite != null)
        {
            image.sprite = roundedSprite;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 4f;
        }

        Outline outline = buttonObject.AddComponent<Outline>();
        outline.effectColor = new Color(color.r, color.g, color.b, 0.8f);
        outline.effectDistance = new Vector2(1f, -1f);

        Shadow btnShadow = buttonObject.AddComponent<Shadow>();
        btnShadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
        btnShadow.effectDistance = new Vector2(0f, -5f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        
        ButtonEnhancer enhancer = buttonObject.AddComponent<ButtonEnhancer>();
        enhancer.themeColor = color;

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0f);
        buttonRect.anchoredPosition = anchoredPosition;
        buttonRect.sizeDelta = new Vector2(180f, 46f);

        TMP_Text text = CreateText(buttonObject.transform, "Label", Vector2.zero, Vector2.zero, 24f);
        text.text = label;
        text.color = Color.white;

        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }

    private static GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    private void Upgrade()
    {
        if (tower == null)
            return;

        tower.Upgrade();
        Refresh();
    }

    private void Sell()
    {
        if (tower == null)
            return;

        PlacedTower towerToSell = tower;
        tower = null;
        ClearHoverState();
        gameObject.SetActive(false);
        towerToSell.Sell();
    }

    private void Close()
    {
        if (tower != null)
            tower.Deselect();

        tower = null;
        ClearHoverState();
        gameObject.SetActive(false);
    }

    private void RefreshCloseText()
    {
        if (closeButtonText == null)
            return;

        closeButtonText.text = $"Close ({Mathf.CeilToInt(closeTimer)})";
    }

    private void ClearHoverState()
    {
        mouse_over = false;

        if (UIManager.main != null)
            UIManager.main.SetHoveringState(false);
    }
}
