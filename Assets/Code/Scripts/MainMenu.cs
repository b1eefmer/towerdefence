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
        SceneManager.LoadScene("OptionsScene");
    }

    public void QuitGame()
    {

        UnityEditor.EditorApplication.isPlaying = false;

        Application.Quit();

    }
}
