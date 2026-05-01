using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void Options()
    {
        Debug.Log("Options clicked");
    }

    public void QuitGame()
    {

        UnityEditor.EditorApplication.isPlaying = false;

        Application.Quit();

    }
}
