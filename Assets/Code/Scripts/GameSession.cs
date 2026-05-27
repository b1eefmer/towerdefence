using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance;

    public bool hasCarriedCurrency;
    public int carriedCurrency;
    public int levelEntrySnapshot;
    public string savedSceneName;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void EnsureInstance()
    {
        if (Instance == null)
            new GameObject("GameSession").AddComponent<GameSession>();
    }

    public static void NewGame()
    {
        EnsureInstance();
        Instance.hasCarriedCurrency = false;
        Instance.carriedCurrency = 0;
        Instance.levelEntrySnapshot = 0;
        Instance.savedSceneName = null;
    }
}
