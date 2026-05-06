using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenu : MonoBehaviour
{
    private const string MainSceneName = "SampleScene";

    private void Start()
    {
        Button loadButton = FindButton("LoadButton");
        if (loadButton != null)
        {
            loadButton.interactable = SaveSystem.HasSave();
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(MainSceneName);
    }

    public void Options()
    {
        SceneManager.LoadScene("OptionsScene");
    }

    public void LoadGame()
    {
        SaveSystem.LoadGame();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    private Button FindButton(string objectName)
    {
        GameObject foundObject = GameObject.Find(objectName);
        if (foundObject == null)
        {
            return null;
        }

        return foundObject.GetComponent<Button>();
    }
}
