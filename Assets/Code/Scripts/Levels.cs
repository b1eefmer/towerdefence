using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    private readonly string[] levelScenes = { "SampleScene", "FireScene" };

    private void Awake()
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

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;

        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
        {
            PauseManager.Instance.ResumeGame();
        }

        string currentScene = SceneManager.GetActiveScene().name;
        int currentIndex = System.Array.IndexOf(levelScenes, currentScene);
        int nextIndex = currentIndex + 1;

        if (nextIndex < levelScenes.Length)
        {
            string nextSceneName = levelScenes[nextIndex];
            LevelTransition teleport = FindFirstObjectByType<LevelTransition>();
            if (teleport != null)
            {
                teleport.StartTeleport(nextSceneName);
            }
            else
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
        else
        {
            Debug.Log("No more levels available.");
        }
    }

    public void LoadSceneDirectly(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
