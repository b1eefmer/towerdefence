using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

 
    private string[] levelScenes = new string[] { "SampleScene", "FireScene" };

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

    public void LoadNextLevel()
    {

        Time.timeScale = 1f;

        Scene pauseScene = SceneManager.GetSceneByName("PauseScene");
        if (pauseScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(pauseScene);
        }


        string currentScene = SceneManager.GetActiveScene().name;
        int currentIndex = System.Array.IndexOf(levelScenes, currentScene);
        int nextIndex = currentIndex + 1;

        if (nextIndex < levelScenes.Length)
        {
            SceneManager.LoadScene(levelScenes[nextIndex]);
        }
        else
        {
            Debug.Log("Koniec gry – brak dalszych poziomów");
        
        }
    }
}