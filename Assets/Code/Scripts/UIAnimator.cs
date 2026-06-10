using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class UIAnimator
{
    // Анімація масштабування (Scale) з кривою Ease Out Back
    public static IEnumerator ScaleIn(RectTransform target, float duration)
    {
        if (target == null) yield break;

        target.localScale = Vector3.zero;
        target.gameObject.SetActive(true);

        float time = 0;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / duration);
            // Ease Out Back формула
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            float eased = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);

            target.localScale = new Vector3(eased, eased, eased);
            yield return null;
        }
        target.localScale = Vector3.one;
    }

    public static IEnumerator ScaleOut(RectTransform target, float duration, System.Action onComplete = null)
    {
        if (target == null) yield break;

        float time = 0;
        Vector3 startScale = target.localScale;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / duration);
            // Ease In Back формула
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            float eased = c3 * t * t * t - c1 * t * t;

            target.localScale = Vector3.LerpUnclamped(startScale, Vector3.zero, eased);
            yield return null;
        }
        target.localScale = Vector3.zero;
        target.gameObject.SetActive(false);
        onComplete?.Invoke();
    }

    // Анімація прозорості (Fade)
    public static IEnumerator FadeIn(CanvasGroup group, float duration)
    {
        if (group == null) yield break;

        group.alpha = 0f;
        group.gameObject.SetActive(true);

        float time = 0;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            group.alpha = Mathf.Clamp01(time / duration);
            yield return null;
        }
        group.alpha = 1f;
    }

    public static IEnumerator FadeOut(CanvasGroup group, float duration, System.Action onComplete = null)
    {
        if (group == null) yield break;

        float time = 0;
        float startAlpha = group.alpha;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(startAlpha, 0f, time / duration);
            yield return null;
        }
        group.alpha = 0f;
        group.gameObject.SetActive(false);
        onComplete?.Invoke();
    }
}
