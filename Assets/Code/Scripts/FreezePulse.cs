using System.Collections;
using UnityEngine;

public class FreezePulse : MonoBehaviour
{
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private int segments = 48;
    [SerializeField] private float lineWidth = 0.08f;
    [SerializeField] private Color pulseColor = new Color(0.4f, 0.85f, 1f, 0.9f);

    private LineRenderer lineRenderer;
    private float maxRadius;

    public void Init(float radius, Color color)
    {
        maxRadius = radius;
        pulseColor = color;
        SetupLineRenderer();
        StartCoroutine(AnimatePulse());
    }

    private void SetupLineRenderer()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.positionCount = segments;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.startColor = pulseColor;
        lineRenderer.endColor = pulseColor;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.sortingOrder = 10;
    }

    private IEnumerator AnimatePulse()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float currentRadius = Mathf.Lerp(0f, maxRadius, t);
            float alpha = Mathf.Lerp(pulseColor.a, 0f, t);

            DrawCircle(currentRadius);

            Color color = pulseColor;
            color.a = alpha;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    private void DrawCircle(float radius)
    {
        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * 2f * Mathf.PI;
            lineRenderer.SetPosition(i, new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f));
        }
    }
}
