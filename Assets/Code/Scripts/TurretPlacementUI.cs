using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Lightweight runtime-built UI panel shown while a turret is choosing its facing direction.
/// Contains a hint label plus Confirm and Cancel buttons. No scene wiring required.
/// </summary>
public class TurretPlacementUI : MonoBehaviour
{
    private Turret owner;
    private GameObject canvasGO;

    public static TurretPlacementUI Spawn(Turret turret)
    {
        GameObject host = new GameObject("TurretPlacementUI");
        TurretPlacementUI ui = host.AddComponent<TurretPlacementUI>();
        ui.owner = turret;
        ui.BuildCanvas();
        EnsureEventSystem();
        return ui;
    }

    public void Close()
    {
        if (canvasGO != null) Destroy(canvasGO);
        if (gameObject != null) Destroy(gameObject);
    }

    private void BuildCanvas()
    {
        canvasGO = new GameObject("TurretPlacementCanvas");

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject panel = CreatePanel(canvasGO.transform);
        CreateHintLabel(panel.transform);
        CreateButton(panel.transform, "Confirm (Enter)", new Vector2(-140f, 25f),
            new Color(0.18f, 0.62f, 0.28f), OnConfirm);
        CreateButton(panel.transform, "Cancel (Esc)", new Vector2(140f, 25f),
            new Color(0.68f, 0.22f, 0.22f), OnCancel);
    }

    private GameObject CreatePanel(Transform parent)
    {
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(parent, false);

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.72f);

        RectTransform rt = bg.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 60f);
        rt.sizeDelta = new Vector2(620f, 150f);

        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.35f);
        outline.effectDistance = new Vector2(2f, -2f);

        return panel;
    }

    private void CreateHintLabel(Transform parent)
    {
        GameObject labelGO = new GameObject("Hint");
        labelGO.transform.SetParent(parent, false);

        Text label = labelGO.AddComponent<Text>();
        label.text = "W / A / S / D or arrow keys - choose direction\nEnter - confirm    Esc - cancel";
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.fontSize = 22;
        label.font = LoadDefaultFont();
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rt = label.rectTransform;
        rt.anchorMin = new Vector2(0f, 0.55f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = new Vector2(20f, 0f);
        rt.offsetMax = new Vector2(-20f, -10f);
    }

    private void CreateButton(Transform parent, string text, Vector2 anchoredPos, Color color, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonGO = new GameObject("Button_" + text);
        buttonGO.transform.SetParent(parent, false);

        Image image = buttonGO.AddComponent<Image>();
        image.color = color;

        Button button = buttonGO.AddComponent<Button>();
        ColorBlock cb = button.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
        cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        cb.selectedColor = Color.white;
        cb.disabledColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        cb.colorMultiplier = 1f;
        cb.fadeDuration = 0.05f;
        button.colors = cb;
        button.onClick.AddListener(onClick);

        RectTransform rt = image.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(240f, 50f);

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);

        Text label = textGO.AddComponent<Text>();
        label.text = text;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.fontSize = 20;
        label.fontStyle = FontStyle.Bold;
        label.font = LoadDefaultFont();
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform textRT = label.rectTransform;
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
    }

    private static Font LoadDefaultFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return font;
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;

        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    private void OnConfirm()
    {
        if (owner != null) owner.ConfirmPlacement();
    }

    private void OnCancel()
    {
        if (owner != null) owner.CancelPlacement();
    }
}
