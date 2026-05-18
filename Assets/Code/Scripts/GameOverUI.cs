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
            panel.SetActive(false);
    }

    public void ShowGameOver(int kills, int gold, float gameTime)
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

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}