using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [SerializeField] private GameObject pausePanel;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused;

    private void Awake()
    {
        Time.timeScale = 1f;
        SetPaused(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused && BuildMananger.main != null && BuildMananger.main.CancelActivePlacement())
                return;

            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        SetPaused(true);
    }

    public void ResumeGame()
    {
        SetPaused(false);
    }

    public void ReturnToMainMenu()
    {
        SetPaused(false);
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnDisable()
    {
        IsPaused = false;
        Time.timeScale = 1f;
    }

    private void SetPaused(bool paused)
    {
        isPaused = paused;
        IsPaused = paused;

        if (pausePanel != null)
            pausePanel.SetActive(paused);

        Time.timeScale = paused ? 0f : 1f;
    }
}
