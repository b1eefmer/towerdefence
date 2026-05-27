using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "SampleScene";
    private VolumeSettingsPanel volumePanel;

    private void Awake()
    {
        GameSession.EnsureInstance();
        Time.timeScale = 1f;

        volumePanel = GetComponent<VolumeSettingsPanel>();
        if (volumePanel == null)
            volumePanel = gameObject.AddComponent<VolumeSettingsPanel>();

        volumePanel.Initialize(true);

        FriendlyMenuView menuView = GetComponent<FriendlyMenuView>();
        if (menuView == null)
            menuView = gameObject.AddComponent<FriendlyMenuView>();

        menuView.BuildMainMenu(this, volumePanel);
    }

    public void StartGame()
    {
        GameSession.NewGame();
        SceneManager.LoadScene(gameSceneName);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
