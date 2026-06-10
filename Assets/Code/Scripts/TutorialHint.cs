using TMPro;
using UnityEngine;

public class TutorialHint : MonoBehaviour
{
    private LineRenderer ring;
    private float pulseTimer;
    private const int Segments = 48;
    private static readonly Color HintColor = new Color(1f, 0.9f, 0.2f, 1f);

    public static TutorialHint Create(Vector3 worldPos, float radius, string keyLabel = null)
    {
        GameObject obj = new GameObject("TutorialHint");
        obj.transform.position = worldPos;
        TutorialHint hint = obj.AddComponent<TutorialHint>();
        hint.BuildRing(radius);
        if (!string.IsNullOrEmpty(keyLabel))
            hint.BuildKeyLabel(keyLabel, radius);
        return hint;
    }

    private void BuildRing(float radius)
    {
        ring = gameObject.AddComponent<LineRenderer>();
        ring.useWorldSpace = false;
        ring.loop = true;
        ring.positionCount = Segments;
        ring.startWidth = 0.08f;
        ring.endWidth = 0.08f;
        ring.material = new Material(Shader.Find("Sprites/Default"));
        ring.sortingOrder = 25;

        for (int i = 0; i < Segments; i++)
        {
            float angle = (float)i / Segments * 2f * Mathf.PI;
            ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
        }
    }

    private void BuildKeyLabel(string text, float radius)
    {
        GameObject labelObj = new GameObject("KeyLabel");
        labelObj.transform.SetParent(transform, false);
        labelObj.transform.localPosition = new Vector3(0f, radius + 0.6f, 0f);

        TextMeshPro tmp = labelObj.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = 3.5f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = HintColor;
        tmp.fontStyle = FontStyles.Bold;

        RectTransform rect = labelObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(4f, 1.2f);
    }

    public void SetWorldPosition(Vector3 worldPos)
    {
        transform.position = worldPos;
    }

    private void Update()
    {
        pulseTimer += Time.deltaTime * 2.8f;
        float alpha = Mathf.Lerp(0.2f, 0.95f, (Mathf.Sin(pulseTimer) + 1f) * 0.5f);
        Color c = HintColor;
        c.a = alpha;
        ring.startColor = c;
        ring.endColor = c;
    }

    public void Dismiss()
    {
        Destroy(gameObject);
    }
}
