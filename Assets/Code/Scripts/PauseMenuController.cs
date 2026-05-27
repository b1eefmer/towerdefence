using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [SerializeField] private GameObject pausePanel;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused;
    private VolumeSettingsPanel volumePanel;
    private FriendlyMenuView menuView;

    private void Awake()
    {
        Time.timeScale = 1f;
        volumePanel = GetComponent<VolumeSettingsPanel>();
        if (volumePanel == null)
            volumePanel = gameObject.AddComponent<VolumeSettingsPanel>();

        volumePanel.Initialize(false);

        menuView = GetComponent<FriendlyMenuView>();
        if (menuView == null)
            menuView = gameObject.AddComponent<FriendlyMenuView>();

        menuView.BuildPauseMenu(this, volumePanel);
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

    public void RestartLevel()
    {
        SetPaused(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SaveGame()
    {
        SaveSystem.SaveCurrentGame();

        if (menuView != null)
            menuView.RefreshSaveButtons(GetCanSave(), SaveSystem.HasSave());
    }

    public void LoadGame()
    {
        if (SaveSystem.LoadSavedGame())
            SetPaused(false);
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

        if (volumePanel != null)
            volumePanel.SetVisible(paused);

        if (menuView != null)
        {
            menuView.SetVisible(paused);
            menuView.RefreshSaveButtons(GetCanSave(), SaveSystem.HasSave());
        }

        Time.timeScale = paused ? 0f : 1f;
    }

    private bool GetCanSave()
    {
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        return spawner != null && spawner.CanSaveNow;
    }
}
