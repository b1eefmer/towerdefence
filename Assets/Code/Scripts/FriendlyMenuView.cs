using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FriendlyMenuView : MonoBehaviour
{
    private GameObject canvasObject;
    private GameObject root;
    private GameObject sidebar;
    private Button saveButton;
    private Button loadButton;

    public void BuildMainMenu(MainMenuController controller, VolumeSettingsPanel volumePanel)
    {
        CreateCanvas(80);
        
        // Створюємо повноцінний бекграунд, завантажуючи картинку рівня
        root = CreateUIObject("Root", canvasObject.transform);
        Image bgImage = root.AddComponent<Image>();
        Sprite bgSprite = LoadSpriteFromResources("background");
        if (bgSprite != null)
        {
            bgImage.sprite = bgSprite;
            bgImage.color = new Color(0.8f, 0.8f, 0.8f, 1f); // Легке затемнення фону
        }
        else
        {
            bgImage.color = new Color(0.05f, 0.07f, 0.11f, 1f); // Запасний темний фон
        }

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        root.AddComponent<CanvasGroup>();

        // Створюємо декорацію (Кристал) справа
        Sprite crystalSprite = LoadSpriteFromResources("krysztal");
        if (crystalSprite != null)
        {
            GameObject crystalObj = CreateUIObject("Hero Crystal", root.transform);
            Image crystalImage = crystalObj.AddComponent<Image>();
            crystalImage.sprite = crystalSprite;
            
            // Масштабуємо, щоб виглядав велично
            RectTransform cRect = crystalObj.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.7f, 0.5f);
            cRect.anchorMax = new Vector2(0.7f, 0.5f);
            cRect.pivot = new Vector2(0.5f, 0.5f);
            cRect.sizeDelta = new Vector2(600f, 600f);
            cRect.anchoredPosition = new Vector2(0f, 0f);
            
            // Додаємо анімацію польоту
            crystalObj.AddComponent<FloatingEffect>();
        }

        // Створюємо бокову панель меню зліва (Glassmorphism)
        sidebar = CreateUIObject("Sidebar Menu", root.transform);
        Image sideImage = sidebar.AddComponent<Image>();
        sideImage.color = new Color(0.05f, 0.07f, 0.11f, 0.85f); // Напівпрозоре скло
        sidebar.AddComponent<GlassPanel>();

        RectTransform sideRect = sidebar.GetComponent<RectTransform>();
        sideRect.anchorMin = new Vector2(0f, 0f);
        sideRect.anchorMax = new Vector2(0f, 1f); // Розтягуємо на всю висоту
        sideRect.pivot = new Vector2(0f, 0.5f);
        sideRect.sizeDelta = new Vector2(600f, 0f); // Ширина 600 пікселів
        sideRect.anchoredPosition = Vector2.zero;

        // Заголовки
        CreateText(sidebar.transform, "Title", "CRYSTAL\nDEFENSE", new Vector2(300f, -250f), new Vector2(480f, 160f), 64f, FontStyles.Bold);
        CreateText(sidebar.transform, "Subtitle", "Protect the sacred artifact", new Vector2(300f, -360f), new Vector2(480f, 40f), 24f, FontStyles.Normal);

        // Кнопки (вирівняні по лівому краю)
        float startY = -500f;
        float spacing = 85f;

        CreateButton(sidebar.transform, "Start Button", "START CRUSADE", new Vector2(300f, startY), new Color(0f, 0.75f, 0.85f, 1f), controller.StartGame);
        loadButton = CreateButton(sidebar.transform, "Load Button", "LOAD GAME", new Vector2(300f, startY - spacing), new Color(0.15f, 0.25f, 0.45f, 1f), controller.LoadGame);
        CreateButton(sidebar.transform, "Audio Button", "SETTINGS", new Vector2(300f, startY - spacing * 2), new Color(0.15f, 0.25f, 0.45f, 1f), volumePanel.Open);
        CreateButton(sidebar.transform, "Exit Button", "EXIT", new Vector2(300f, startY - spacing * 3), new Color(0.60f, 0.22f, 0.22f, 1f), controller.ExitGame);
        
        RefreshSaveButtons(false, SaveSystem.HasSave());
        SetVisible(true);
    }

    public void BuildPauseMenu(PauseMenuController controller, VolumeSettingsPanel volumePanel)
    {
        CreateCanvas(90);
        
        root = CreateUIObject("Root", canvasObject.transform);
        Image image = root.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.75f);
        root.AddComponent<CanvasGroup>();
        
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        // Панель паузи по центру
        sidebar = CreateUIObject("Pause Menu Card", root.transform); // Використовуємо sidebar змінну для анімації
        Image cardImage = sidebar.AddComponent<Image>();
        cardImage.color = new Color(0.1f, 0.11f, 0.16f, 0.90f);
        
        RectTransform rect = sidebar.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(560f, 650f);

        var title = CreateText(sidebar.transform, "Title", "PAUSED", new Vector2(0f, -70f), new Vector2(500f, 64f), 48f, FontStyles.Bold);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -70f);

        var subtitle = CreateText(sidebar.transform, "Subtitle", "Take a breath, commander", new Vector2(0f, -124f), new Vector2(500f, 34f), 22f, FontStyles.Normal);
        RectTransform subRect = subtitle.GetComponent<RectTransform>();
        subRect.anchorMin = new Vector2(0.5f, 1f);
        subRect.anchorMax = new Vector2(0.5f, 1f);
        subRect.anchoredPosition = new Vector2(0f, -124f);

        CreateButton(sidebar.transform, "Continue Button", "Continue", new Vector2(0f, -190f), new Color(0f, 0.75f, 0.85f, 1f), controller.ResumeGame);
        saveButton = CreateButton(sidebar.transform, "Save Button", "Save Game", new Vector2(0f, -260f), new Color(0.15f, 0.25f, 0.45f, 1f), controller.SaveGame);
        loadButton = CreateButton(sidebar.transform, "Load Button", "Load Game", new Vector2(0f, -330f), new Color(0.15f, 0.25f, 0.45f, 1f), controller.LoadGame);
        CreateButton(sidebar.transform, "Audio Button", "Audio Settings", new Vector2(0f, -400f), new Color(0.15f, 0.25f, 0.45f, 1f), volumePanel.Open);
        CreateButton(sidebar.transform, "Restart Button", "Restart Level", new Vector2(0f, -470f), new Color(0.44f, 0.36f, 0.20f, 1f), controller.RestartLevel);
        CreateButton(sidebar.transform, "Main Menu Button", "Main Menu", new Vector2(0f, -540f), new Color(0.60f, 0.22f, 0.22f, 1f), controller.ReturnToMainMenu);

        canvasObject.SetActive(false);
    }

    public void SetVisible(bool visible)
    {
        if (canvasObject != null)
        {
            if (visible)
            {
                canvasObject.SetActive(true);
                CanvasGroup group = root.GetComponent<CanvasGroup>();
                if (group != null) StartCoroutine(UIAnimator.FadeIn(group, 0.2f));
                
                if (sidebar != null)
                {
                    RectTransform cardRect = sidebar.GetComponent<RectTransform>();
                    StartCoroutine(UIAnimator.ScaleIn(cardRect, 0.35f));
                }
            }
            else
            {
                canvasObject.SetActive(false);
            }
        }
    }

    public void RefreshSaveButtons(bool canSave, bool canLoad)
    {
        if (saveButton != null)
            saveButton.interactable = canSave;

        if (loadButton != null)
            loadButton.interactable = canLoad;
    }

    private void CreateCanvas(int sortingOrder)
    {
        canvasObject = new GameObject("Friendly Menu UI");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
    }

    private Button CreateButton(
        Transform parent,
        string objectName,
        string label,
        Vector2 anchoredPosition,
        Color color,
        UnityAction action)
    {
        GameObject buttonObject = CreateUIObject(objectName, parent);
        Image image = buttonObject.AddComponent<Image>();
        
        // Базовий колір - темне напівпрозоре скло (Magical Frosted Glass)
        image.color = new Color(0.02f, 0.05f, 0.1f, 0.55f);
        
        // Використовуємо вбудований спрайт Unity для заокруглених кутів
        Sprite roundedSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        if (roundedSprite != null)
        {
            image.sprite = roundedSprite;
            image.type = Image.Type.Sliced;
            // Збільшуємо мультиплікатор, щоб кути були ідеально заокруглені (Pill shape)
            image.pixelsPerUnitMultiplier = 4f; 
        }

        // Кольорове обведення (Магічний Glow ефект)
        Outline outline = buttonObject.AddComponent<Outline>();
        outline.effectColor = new Color(color.r, color.g, color.b, 0.8f);
        outline.effectDistance = new Vector2(1f, -1f);

        // М'яка тінь
        Shadow btnShadow = buttonObject.AddComponent<Shadow>();
        btnShadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
        btnShadow.effectDistance = new Vector2(0f, -5f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        ButtonEnhancer enhancer = buttonObject.AddComponent<ButtonEnhancer>();
        enhancer.themeColor = color;

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f); 
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(360f, 60f);

        TMP_Text text = CreateText(buttonObject.transform, "Label", label, Vector2.zero, Vector2.zero, 24f, FontStyles.Bold);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = Vector2.zero;

        return button;
    }

    private TMP_Text CreateText(
        Transform parent,
        string objectName,
        string textValue,
        Vector2 anchoredPosition,
        Vector2 size,
        float fontSize,
        FontStyles style)
    {
        GameObject textObject = CreateUIObject(objectName, parent);
        TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = textValue;
        text.horizontalAlignment = HorizontalAlignmentOptions.Center;
        text.verticalAlignment = VerticalAlignmentOptions.Middle;
        text.color = Color.white;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.raycastTarget = false;

        // Тінь для тексту, щоб він виглядав об'ємно
        textObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.5f);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f); 
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        return text;
    }

    private GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    private Sprite LoadSpriteFromResources(string name)
    {
        Texture2D tex = Resources.Load<Texture2D>(name);
        if (tex != null)
        {
            return Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        }
        return null;
    }
}
