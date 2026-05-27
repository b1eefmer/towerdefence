using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlacementUIController : MonoBehaviour
{
    private GameObject panel;

    private void Awake()
    {
        CreateView();
        Hide();
    }

    public void Show()
    {
        panel.SetActive(true);
    }

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void CreateView()
    {
        GameObject canvasObject = new GameObject("Placement UI");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        panel = CreateUIObject("Placement Panel", canvasObject.transform);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.07f, 0.08f, 0.11f, 0.92f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 24f);
        panelRect.sizeDelta = new Vector2(570f, 126f);

        GameObject instructionObject = CreateUIObject("Instruction", panel.transform);
        TextMeshProUGUI instruction = instructionObject.AddComponent<TextMeshProUGUI>();
        instruction.text = "Choose direction: W / A / S / D";
        instruction.alignment = TextAlignmentOptions.Center;
        instruction.fontSize = 26f;
        instruction.color = Color.white;
        RectTransform instructionRect = instructionObject.GetComponent<RectTransform>();
        instructionRect.anchorMin = new Vector2(0f, 1f);
        instructionRect.anchorMax = new Vector2(1f, 1f);
        instructionRect.pivot = new Vector2(0.5f, 1f);
        instructionRect.anchoredPosition = new Vector2(0f, -12f);
        instructionRect.sizeDelta = new Vector2(-24f, 40f);

        Button confirmButton = CreateButton(
            panel.transform,
            "Confirm Button",
            "Confirm (Enter)",
            new Vector2(-132f, 18f),
            new Color(0.20f, 0.58f, 0.28f, 1f));
        confirmButton.onClick.AddListener(Confirm);

        Button cancelButton = CreateButton(
            panel.transform,
            "Cancel Button",
            "Cancel (Esc)",
            new Vector2(132f, 18f),
            new Color(0.60f, 0.22f, 0.22f, 1f));
        cancelButton.onClick.AddListener(Cancel);
    }

    private Button CreateButton(
        Transform parent,
        string objectName,
        string label,
        Vector2 position,
        Color color)
    {
        GameObject buttonObject = CreateUIObject(objectName, parent);
        Image image = buttonObject.AddComponent<Image>();
        image.color = color;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(238f, 46f);

        GameObject labelObject = CreateUIObject("Label", buttonObject.transform);
        TextMeshProUGUI labelText = labelObject.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.fontSize = 22f;
        labelText.color = Color.white;
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;

        return button;
    }

    private GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    private void Confirm()
    {
        BuildMananger.main.ConfirmActivePlacement();
    }

    private void Cancel()
    {
        BuildMananger.main.CancelActivePlacement();
    }
}
