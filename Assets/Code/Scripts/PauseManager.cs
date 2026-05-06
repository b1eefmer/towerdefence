using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;
    private const string PauseSceneName = "PauseScene";
    private bool isPauseSceneLoading;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        BindPauseButtonsRuntime();
    }

    public void PauseGame()
    {
        Scene pauseScene = SceneManager.GetSceneByName(PauseSceneName);
        if (pauseScene.isLoaded || isPauseSceneLoading)
        {
            return;
        }

        Time.timeScale = 0f;
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(PauseSceneName, LoadSceneMode.Additive);
        if (loadOperation == null)
        {
            isPauseSceneLoading = false;
            Debug.LogError($"Could not load scene '{PauseSceneName}'.");
            return;
        }

        isPauseSceneLoading = true;
        loadOperation.completed += _ => isPauseSceneLoading = false;
    }

    public void ResumeGame()
    {
        Scene pauseScene = SceneManager.GetSceneByName(PauseSceneName);
        if (pauseScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(PauseSceneName);
        }

        isPauseSceneLoading = false;
        Time.timeScale = 1f;
    }

    public void SaveGame()
    {
        SaveSystem.SaveGame();
    }

    public void LoadGame()
    {
        SaveSystem.LoadGame();
    }

    public void BackToMenu()
    {
        isPauseSceneLoading = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    private void BindPauseButtonsRuntime()
    {
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (Button button in allButtons)
        {
            if (button == null || button.name != "PauseGameButton")
            {
                continue;
            }

            if (HasPauseBinding(button))
            {
                continue;
            }

            button.onClick.AddListener(PauseGame);
        }
    }

    private bool HasPauseBinding(Button button)
    {
        Button.ButtonClickedEvent onClick = button.onClick;
        int persistentEventCount = onClick.GetPersistentEventCount();
        for (int i = 0; i < persistentEventCount; i++)
        {
            if (onClick.GetPersistentMethodName(i) != nameof(PauseGame))
            {
                continue;
            }

            Object target = onClick.GetPersistentTarget(i);
            if (target == this)
            {
                return true;
            }
        }

        return false;
    }
}
