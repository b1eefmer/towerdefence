using TMPro;
using UnityEngine;

public class Menu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TextMeshProUGUI currencyUI;
    [SerializeField] Animator anim;
    private bool isMenuOpen = true;
    public void ToogleMenu()
    {
        isMenuOpen = !isMenuOpen;
        anim.SetBool("MenuOpen", isMenuOpen);
    }
    private void Update()
    {
        if (currencyUI != null && LevelMananger.main != null)
        {
            currencyUI.text = $"<color=#FFD700>Gold:</color> {LevelMananger.main.currency}";
        }
    }
    //public void SetSelected()
    //{

    //}
    void Start()
    {
        // Автоматично знаходимо картинку фону в грі і зменшуємо її, 
        // щоб користувачу не доводилося вручну вішати скрипти.
        SpriteRenderer[] allSprites = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        foreach(SpriteRenderer sr in allSprites)
        {
            if (sr.sprite != null && sr.sprite.name.ToLower().Contains("background"))
            {
                Camera cam = Camera.main;
                if (cam != null && cam.orthographic)
                {
                    // Отримуємо розміри камери (екрана)
                    float screenHeight = cam.orthographicSize * 2f;
                    float screenWidth = screenHeight * cam.aspect;
                    
                    // Отримуємо базовий розмір спрайту
                    float spriteWidth = sr.sprite.rect.width / sr.sprite.pixelsPerUnit;
                    float spriteHeight = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;
                    
                    // Масштабуємо чітко під екран, щоб не вилазило за краї
                    sr.transform.localScale = new Vector3(screenWidth / spriteWidth, screenHeight / spriteHeight, 1f);
                    
                    // Ставимо по центру
                    sr.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 10f);
                }
            }
        }

        // 1. Спочатку застосовуємо стиль до самої панелі бокового меню (якщо є) на магічне скло
        UnityEngine.UI.Image bgImage = GetComponent<UnityEngine.UI.Image>();
        if (bgImage != null)
        {
            bgImage.color = new Color(0.02f, 0.05f, 0.1f, 0.6f);
            UnityEngine.UI.Outline bgOutline = gameObject.GetComponent<UnityEngine.UI.Outline>() ?? gameObject.AddComponent<UnityEngine.UI.Outline>();
            bgOutline.effectColor = new Color(0f, 0.75f, 0.85f, 0.3f);
            bgOutline.effectDistance = new Vector2(1f, -1f);
        }

        // 2. Знаходимо всі кнопки в меню і застосовуємо до них гарний магічний стиль
        UnityEngine.UI.Button[] buttons = GetComponentsInChildren<UnityEngine.UI.Button>(true);
        Sprite roundedSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");

        foreach (UnityEngine.UI.Button btn in buttons)
        {
            UnityEngine.UI.Image img = btn.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
            {
                // Змінюємо колір на яскравий магічний синій, щоб вони не здавалися чорними
                img.color = new Color(0.15f, 0.35f, 0.65f, 0.9f);
                if (roundedSprite != null)
                {
                    img.sprite = roundedSprite;
                    img.type = UnityEngine.UI.Image.Type.Sliced;
                    img.pixelsPerUnitMultiplier = 4f;
                }
            }

            // Додаємо рамку
            UnityEngine.UI.Outline outline = btn.gameObject.GetComponent<UnityEngine.UI.Outline>() ?? btn.gameObject.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = new Color(0f, 0.75f, 0.85f, 0.8f); // Cyan magic glow
            outline.effectDistance = new Vector2(1f, -1f);

            // Додаємо тінь
            UnityEngine.UI.Shadow shadow = btn.gameObject.GetComponent<UnityEngine.UI.Shadow>() ?? btn.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            shadow.effectDistance = new Vector2(0f, -5f);

            // Додаємо Hover анімацію
            ButtonEnhancer enhancer = btn.gameObject.GetComponent<ButtonEnhancer>() ?? btn.gameObject.AddComponent<ButtonEnhancer>();
            enhancer.themeColor = new Color(0f, 0.75f, 0.85f, 1f); // Колір світіння при наведенні

            // 3. Знаходимо текст на кнопці і робимо його красивим
            TMPro.TMP_Text text = btn.GetComponentInChildren<TMPro.TMP_Text>();
            if (text != null)
            {
                text.alignment = TMPro.TextAlignmentOptions.Center;
                text.fontStyle = TMPro.FontStyles.Bold;
                
                // Додаємо об'ємну тінь тексту
                UnityEngine.UI.Shadow textShadow = text.gameObject.GetComponent<UnityEngine.UI.Shadow>() ?? text.gameObject.AddComponent<UnityEngine.UI.Shadow>();
                textShadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
                textShadow.effectDistance = new Vector2(1f, -2f);

                // Динамічно робимо ціну золотою і переносимо на новий рядок
                string originalText = text.text;
                originalText = System.Text.RegularExpressions.Regex.Replace(originalText, "<.*?>", string.Empty);
                
                // Знаходимо числа і фарбуємо їх у золото
                string formattedText = System.Text.RegularExpressions.Regex.Replace(originalText, @"\d+", "<color=#FFD700>$0</color>");
                
                // Якщо є пробіл або тире перед числом, замінюємо на перенесення рядка
                formattedText = System.Text.RegularExpressions.Regex.Replace(formattedText, @"\s*[-]*\s*(<color=#FFD700>\d+</color>)", "\n$1");
                
                text.text = formattedText;
                // Збільшуємо шрифт для читабельності, якщо потрібно
                text.fontSizeMax = 28;
                text.enableAutoSizing = true;
                text.lineSpacing = -20f;
            }
        }
    }
    
}
