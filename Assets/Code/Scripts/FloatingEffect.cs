using UnityEngine;

public class FloatingEffect : MonoBehaviour
{
    public float amplitude = 15f; // Наскільки пікселів вгору/вниз
    public float speed = 1.5f;

    private RectTransform rectTransform;
    private Vector2 startPos;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
            startPos = rectTransform.anchoredPosition;
    }

    void Update()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = startPos + new Vector2(0f, Mathf.Sin(Time.unscaledTime * speed) * amplitude);
        }
    }
}
