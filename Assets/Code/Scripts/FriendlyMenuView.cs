using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FriendlyMenuView : MonoBehaviour
{
    private GameObject canvasObject;
    private GameObject root;
    private Button saveButton;
    private Button loadButton;

    public void BuildMainMenu(MainMenuController controller, VolumeSettingsPanel volumePanel)
    {
        CreateCanvas(80);
        CreateRoot(new Color(0.02f, 0.025f, 0.035f, 0.90f));

        GameObject card = CreateCard(root.transform, new Vector2(600f, 580f));
        CreateText(card.transform, "Title", "Tower Defence", new Vector2(0f, -70f), new Vector2(520f, 70f), 46f, FontStyles.Bold);
        CreateText(card.transform, "Subtitle", "Build, defend, survive", new Vector2(0f, -128f), new Vector2(520f, 36f), 24f, FontStyles.Normal);

        CreateButton(card.transform, "Start Button", "Start Game", new Vector2(0f, -195f), new Color(0.20f, 0.58f, 0.28f, 1f), controller.StartGame);
        loadButton = CreateButton(card.transform, "Load Button", "Load Game", new Vector2(0f, -265f), new Color(0.72f, 0.44f, 0.18f, 1f), controller.LoadGame);
        CreateButton(card.transform, "Audio Button", "Audio Settings", new Vector2(0f, -335f), new Color(0.20f, 0.36f, 0.62f, 1f), volumePanel.Open);
        CreateButton(card.transform, "Exit Button", "Exit Game", new Vector2(0f, -405f), new Color(0.60f, 0.22f, 0.22f, 1f), controller.ExitGame);
        CreateTutorialToggle(card.transform, new Vector2(0f, -472f), controller.OnTutorialToggled);
        RefreshSaveButtons(false, SaveSystem.HasSave());
    }

    public void BuildPauseMenu(PauseMenuController controller, VolumeSettingsPanel volumePanel)
    {
        CreateCanvas(90);
        CreateRoot(new Color(0f, 0f, 0f, 0.72f));

        GameObject card = CreateCard(root.transform, new Vector2(560f, 560f));
        CreateText(card.transform, "Title", "Paused", new Vector2(0f, -70f), new Vector2(500f, 64f), 44f, FontStyles.Bold);
        CreateText(card.transform, "Subtitle", "Take a breath, commander", new Vector2(0f, -124f), new Vector2(500f, 34f), 22f, FontStyles.Normal);

        CreateButton(card.transform, "Continue Button", "Continue", new Vector2(0f, -176f), new Color(0.20f, 0.58f, 0.28f, 1f), controller.ResumeGame);
        saveButton = CreateButton(card.transform, "Save Button", "Save Game", new Vector2(0f, -238f), new Color(0.72f, 0.44f, 0.18f, 1f), controller.SaveGame);
        loadButton = CreateButton(card.transform, "Load Button", "Load Game", new Vector2(0f, -300f), new Color(0.20f, 0.36f, 0.62f, 1f), controller.LoadGame);
        CreateButton(card.transform, "Audio Button", "Audio Settings", new Vector2(0f, -362f), new Color(0.20f, 0.36f, 0.62f, 1f), volumePanel.Open);
        CreateButton(card.transform, "Restart Button", "Restart Level", new Vector2(0f, -424f), new Color(0.44f, 0.36f, 0.20f, 1f), controller.RestartLevel);
        CreateButton(card.transform, "Main Menu Button", "Main Menu", new Vector2(0f, -486f), new Color(0.60f, 0.22f, 0.22f, 1f), controller.ReturnToMainMenu);

        SetVisible(false);
    }

    public void SetVisible(bool visible)
    {
        if (canvasObject != null)
            canvasObject.SetActive(visible);
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

    private void CreateRoot(Color backgroundColor)
    {
        root = CreateUIObject("Root", canvasObject.transform);
        Image image = root.AddComponent<Image>();
        image.color = backgroundColor;

        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private GameObject CreateCard(Transform parent, Vector2 size)
    {
        GameObject card = CreateUIObject("Menu Card", parent);
        Image image = card.AddComponent<Image>();
        image.color = new Color(0.07f, 0.08f, 0.11f, 0.96f);

        RectTransform rect = card.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;

        return card;
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
        image.color = color;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(320f, 54f);

        TMP_Text text = CreateText(buttonObject.transform, "Label", label, Vector2.zero, Vector2.zero, 23f, FontStyles.Bold);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

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
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.raycastTarget = false;

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        return text;
    }

    private void CreateTutorialToggle(Transform parent, Vector2 anchoredPosition, UnityAction<bool> onValueChanged)
    {
        GameObject row = CreateUIObject("Tutorial Toggle Row", parent);
        RectTransform rowRect = row.GetComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0.5f, 1f);
        rowRect.anchorMax = new Vector2(0.5f, 1f);
        rowRect.pivot = new Vector2(0.5f, 0.5f);
        rowRect.anchoredPosition = anchoredPosition;
        rowRect.sizeDelta = new Vector2(320f, 40f);

        // Checkbox background
        GameObject checkObj = CreateUIObject("Checkbox", row.transform);
        Image checkBg = checkObj.AddComponent<Image>();
        checkBg.color = new Color(0.15f, 0.18f, 0.25f, 1f);
        RectTransform checkRect = checkObj.GetComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0f, 0.5f);
        checkRect.anchorMax = new Vector2(0f, 0.5f);
        checkRect.pivot = new Vector2(0f, 0.5f);
        checkRect.anchoredPosition = new Vector2(0f, 0f);
        checkRect.sizeDelta = new Vector2(28f, 28f);

        // Checkmark graphic
        GameObject checkmarkObj = CreateUIObject("Checkmark", checkObj.transform);
        Image checkmark = checkmarkObj.AddComponent<Image>();
        checkmark.color = new Color(0.3f, 0.85f, 0.4f, 1f);
        RectTransform checkmarkRect = checkmarkObj.GetComponent<RectTransform>();
        checkmarkRect.anchorMin = new Vector2(0.15f, 0.15f);
        checkmarkRect.anchorMax = new Vector2(0.85f, 0.85f);
        checkmarkRect.offsetMin = Vector2.zero;
        checkmarkRect.offsetMax = Vector2.zero;

        // Label (fills the row, offset past the checkbox)
        GameObject labelObj = CreateUIObject("Label", row.transform);
        TMP_Text label = labelObj.AddComponent<TextMeshProUGUI>();
        label.text = "Tutorial";
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.color = Color.white;
        label.fontSize = 20f;
        label.raycastTarget = false;
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(38f, 0f);
        labelRect.offsetMax = Vector2.zero;

        // Toggle component
        Toggle toggle = row.AddComponent<Toggle>();
        toggle.targetGraphic = checkBg;
        toggle.graphic = checkmark;
        toggle.isOn = PlayerPrefs.GetInt("tutorial_enabled", 1) == 1;
        toggle.onValueChanged.AddListener(onValueChanged);
    }

    private GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }
}
