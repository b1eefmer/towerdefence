using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveAlertUI : MonoBehaviour
{
    [SerializeField] private float holdDuration = 2.5f;
    [SerializeField] private float slideInDuration = 0.25f;
    [SerializeField] private float slideOutDuration = 0.25f;
    [SerializeField] private Color panelColor = new Color(0.75f, 0.15f, 0.1f, 0.9f);
    [SerializeField] private Color labelColor = Color.white;

    private RectTransform panelRect;
    private TextMeshProUGUI label;
    private const float PanelHeight = 90f;

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
        gameObject.AddComponent<CanvasScaler>();

        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(transform, false);

        Image bg = panelObj.AddComponent<Image>();
        bg.color = panelColor;

        panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.sizeDelta = new Vector2(0f, PanelHeight);

        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(panelObj.transform, false);

        label = textObj.AddComponent<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.Center;
        label.color = labelColor;
        label.fontSize = 26;
        label.fontStyle = FontStyles.Bold;
        label.margin = new Vector4(10f, 0f, 10f, 0f);

        RectTransform textRect = label.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    public void ShowAlert(string message)
    {
        StopAllCoroutines();
        label.text = message;
        StartCoroutine(AlertRoutine());
    }

    private IEnumerator AlertRoutine()
    {
        float t = 0f;
        while (t < slideInDuration)
        {
            SetPanelY(Mathf.Lerp(PanelHeight, 0f, Mathf.SmoothStep(0f, 1f, t / slideInDuration)));
            t += Time.deltaTime;
            yield return null;
        }
        SetPanelY(0f);

        yield return new WaitForSeconds(holdDuration);

        t = 0f;
        while (t < slideOutDuration)
        {
            SetPanelY(Mathf.Lerp(0f, PanelHeight, Mathf.SmoothStep(0f, 1f, t / slideOutDuration)));
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
