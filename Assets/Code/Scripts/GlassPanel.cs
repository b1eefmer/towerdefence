using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class GlassPanel : MonoBehaviour
{
    [Header("Налаштування кольору панелі")]
    [SerializeField] private Color slateDark = new Color(0.1f, 0.11f, 0.16f, 0.85f); // #1A1C29 з прозорістю
    [SerializeField] private bool applyOnAwake = true;

    private void Awake()
    {
        if (applyOnAwake)
        {
            ApplyGlassEffect();
        }
    }

    public void ApplyGlassEffect()
    {
        Image bgImage = GetComponent<Image>();
        if (bgImage != null)
        {
            bgImage.color = slateDark;
        }

        // Для панелей ми також можемо гарантувати, що в них є CanvasGroup для керування прозорістю
        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group == null)
        {
            gameObject.AddComponent<CanvasGroup>();
        }
    }
}
