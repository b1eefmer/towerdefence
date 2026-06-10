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
            panel.SetActive(false);
    }

    public void ShowWinner(int kills, int gold, float gameTime)
    {
        if (killsText != null) killsText.text = $"Kills: {kills}";
        if (goldText != null) goldText.text = $"Gold: {gold}";
        if (timeText != null) timeText.text = $"Time: {gameTime:F1} s";

        panel.SetActive(true);
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

        if (nextLevelSceneName == "Level2")
            LevelMananger.main.IncreaseCurrency(1000);

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
