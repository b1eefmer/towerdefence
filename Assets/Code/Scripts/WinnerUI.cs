using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class WinnerUI : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI killsText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI timeText;

    [SerializeField] private string nextLevelSceneName;

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

    public void ShowWinner(int kills, int gold, float gameTime)
    {
        if (killsText != null) killsText.text = $"Kills: {kills}";
        if (goldText != null) goldText.text = $"Gold: {gold}";
        if (timeText != null) timeText.text = $"Time: {gameTime:F1} s";

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

    public void NextLevel()
    {
        if (string.IsNullOrWhiteSpace(nextLevelSceneName))
        {
            Debug.LogWarning("Next level scene name is not set.");
            return;
        }

        LevelMananger.main.CarryCurrencyToNextLevel();
        Time.timeScale = 1f;

        if (SceneManager.GetActiveScene().name == "SampleScene" && nextLevelSceneName == "Level2")
        {
            if (panel != null)
                panel.SetActive(false);

            Time.timeScale = 0f;
            InfoSlideController.Show(nextLevelSceneName);
            return;
        }

        SceneManager.LoadScene(nextLevelSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}
