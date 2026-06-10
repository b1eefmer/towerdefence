using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveAlertUI : MonoBehaviour
{
    [SerializeField] private float holdDuration = 4f;
    [SerializeField] private float slideInDuration = 0.3f;
    [SerializeField] private float slideOutDuration = 0.25f;

    private RectTransform panelRect;
    private TextMeshProUGUI messageLabel;

    private const float PanelHeight = 110f;
    private const float ShownY = -16f;
    private static readonly Color AccentColor = new Color(0.95f, 0.65f, 0.1f, 1f);

    private void Awake()
    {
        BuildUI();
        SetPanelY(PanelHeight);
    }

    private void BuildUI()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // Main panel — dark, centered, not full-width
        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(transform, false);

        Image bg = panelObj.AddComponent<Image>();
        bg.color = new Color(0.07f, 0.08f, 0.11f, 0.95f);

        Outline outline = panelObj.AddComponent<Outline>();
        outline.effectColor = new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.55f);
        outline.effectDistance = new Vector2(0f, -2f);

        panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.sizeDelta = new Vector2(640f, PanelHeight);

        // Left amber stripe
        GameObject stripeObj = new GameObject("Stripe");
        stripeObj.transform.SetParent(panelObj.transform, false);
        Image stripe = stripeObj.AddComponent<Image>();
        stripe.color = AccentColor;
        RectTransform stripeRect = stripeObj.GetComponent<RectTransform>();
        stripeRect.anchorMin = new Vector2(0f, 0f);
        stripeRect.anchorMax = new Vector2(0f, 1f);
        stripeRect.pivot = new Vector2(0f, 0.5f);
        stripeRect.anchoredPosition = Vector2.zero;
        stripeRect.sizeDelta = new Vector2(5f, 0f);

        // Small header
        GameObject headerObj = new GameObject("Header");
        headerObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI header = headerObj.AddComponent<TextMeshProUGUI>();
        header.text = "▲   INCOMING";
        header.alignment = TextAlignmentOptions.Center;
        header.color = AccentColor;
        header.fontSize = 13f;
        header.fontStyle = FontStyles.Bold;
        header.raycastTarget = false;
        RectTransform headerRect = headerObj.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 1f);
        headerRect.anchorMax = new Vector2(1f, 1f);
        headerRect.pivot = new Vector2(0.5f, 1f);
        headerRect.anchoredPosition = new Vector2(0f, -10f);
        headerRect.sizeDelta = new Vector2(0f, 20f);

        // Message
        GameObject textObj = new GameObject("Message");
        textObj.transform.SetParent(panelObj.transform, false);
        messageLabel = textObj.AddComponent<TextMeshProUGUI>();
        messageLabel.alignment = TextAlignmentOptions.Center;
        messageLabel.color = Color.white;
        messageLabel.fontSize = 24f;
        messageLabel.fontStyle = FontStyles.Bold;
        messageLabel.raycastTarget = false;
        RectTransform textRect = messageLabel.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.offsetMin = new Vector2(16f, 8f);
        textRect.offsetMax = new Vector2(-16f, -36f);
    }

    public void ShowAlert(string message)
    {
        StopAllCoroutines();
        messageLabel.text = message;
        StartCoroutine(AlertRoutine());
    }

    private IEnumerator AlertRoutine()
    {
        float t = 0f;
        while (t < slideInDuration)
        {
            SetPanelY(Mathf.Lerp(PanelHeight, ShownY, Mathf.SmoothStep(0f, 1f, t / slideInDuration)));
            t += Time.deltaTime;
            yield return null;
        }
        SetPanelY(ShownY);

        yield return new WaitForSeconds(holdDuration);

        t = 0f;
        while (t < slideOutDuration)
        {
            SetPanelY(Mathf.Lerp(ShownY, PanelHeight, Mathf.SmoothStep(0f, 1f, t / slideOutDuration)));
            t += Time.deltaTime;
            yield return null;
        }
        SetPanelY(PanelHeight);
    }

    private void SetPanelY(float y)
    {
        panelRect.anchoredPosition = new Vector2(0f, y);
    }
}
