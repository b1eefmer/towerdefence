using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

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

    public void PauseGame()
    {
        Time.timeScale = 0f;
        SceneManager.LoadSceneAsync("PauseScene", LoadSceneMode.Additive);
    }

    public void ResumeGame()
    {
        SceneManager.UnloadSceneAsync("PauseScene");
        Time.timeScale = 1f;
    }

    public void SaveGame()
    {
        Debug.Log("Zapisano stan gry");
       
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}