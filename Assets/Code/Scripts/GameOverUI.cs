using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI killsText;
    public TextMeshProUGUI goldText;   
    public TextMeshProUGUI timeText;    

    void Start()
    {
        if (panel != null)
        {
            panel.SetActive(false);
            
            // Автоматично додаємо ефект скла, якщо його немає
            if (panel.GetComponent<GlassPanel>() == null)
                panel.AddComponent<GlassPanel>();
        }
    }

    public void ShowGameOver(int kills, int gold, float gameTime)
    {
        if (killsText != null) killsText.text = $"Zabici: {kills}";
        if (goldText != null) goldText.text = $"Zloto: {gold}"; // Змінено для уникнення проблем з кодуванням
        if (timeText != null) timeText.text = $"Czas: {gameTime:F1} s";

        panel.SetActive(true);
        
        // Анімація панелі
        CanvasGroup group = panel.GetComponent<CanvasGroup>();
        if (group == null) group = panel.AddComponent<CanvasGroup>();
        StartCoroutine(UIAnimator.FadeIn(group, 0.3f));

        RectTransform rect = panel.GetComponent<RectTransform>();
        if (rect != null) StartCoroutine(UIAnimator.ScaleIn(rect, 0.4f));

        // Додаємо покращення кнопок (Hover/Scale)
        foreach (UnityEngine.UI.Button btn in panel.GetComponentsInChildren<UnityEngine.UI.Button>(true))
        {
            if (btn.GetComponent<ButtonEnhancer>() == null)
                btn.gameObject.AddComponent<ButtonEnhancer>();
        }

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}
