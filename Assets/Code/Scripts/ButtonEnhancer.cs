using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class ButtonEnhancer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Button button;
    private Image buttonImage;
    private Color originalColor;
    private Color hoverColor;
    private Color pressedColor;
    private Color disabledColor;

    public Color themeColor = Color.white; // Колір магічного світіння

    private Vector3 originalScale;
    private float animationSpeed = 15f;

    private bool isHovering = false;
    private bool isPressed = false;

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        
        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
            
            // Hover: змішуємо темне скло з кольором теми, роблячи його менш прозорим
            hoverColor = Color.Lerp(originalColor, themeColor, 0.25f);
            hoverColor.a = 0.85f; 
            
            // Pressed: сильніший спалах кольору
            pressedColor = Color.Lerp(originalColor, themeColor, 0.5f);
            pressedColor.a = 0.95f;
            
            disabledColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.2f);
        }

        originalScale = transform.localScale;

        // Вимикаємо стандартні колірні зміни Unity Button, бо ми робимо їх через код
        button.transition = Selectable.Transition.None;
    }

    private void Update()
    {
        // Оновлюємо стан кнопки (Disabled/Enabled)
        if (buttonImage != null)
        {
            if (!button.interactable)
            {
                buttonImage.color = disabledColor;
            }
            else if (isPressed)
            {
                buttonImage.color = Color.Lerp(buttonImage.color, pressedColor, Time.unscaledDeltaTime * animationSpeed);
            }
            else if (isHovering)
            {
                buttonImage.color = Color.Lerp(buttonImage.color, hoverColor, Time.unscaledDeltaTime * animationSpeed);
            }
            else
            {
                buttonImage.color = Color.Lerp(buttonImage.color, originalColor, Time.unscaledDeltaTime * animationSpeed);
            }
        }

        // Анімація масштабу (Squash and Stretch, згідно з правилами Disney - scale 0.97)
        float targetScale = isPressed ? 0.97f : 1f;
        transform.localScale = Vector3.Lerp(transform.localScale, originalScale * targetScale, Time.unscaledDeltaTime * animationSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.interactable) isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        isPressed = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button.interactable) isPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }
}
