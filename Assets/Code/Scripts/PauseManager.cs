using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;
    private const string PauseSceneName = "PauseScene";
    private bool isPauseSceneLoading;
    private bool isPauseRequested;
    private static Texture2D pausedBackgroundTexture;

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
        if (pauseScene.isLoaded || isPauseSceneLoading || isPauseRequested)
        {
            return;
        }

        StartCoroutine(PauseGameRoutine());
    }

    public void ResumeGame()
    {
        Scene pauseScene = SceneManager.GetSceneByName(PauseSceneName);
        if (pauseScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(PauseSceneName);
        }

        isPauseSceneLoading = false;
        isPauseRequested = false;
        Time.timeScale = 1f;
        ReleasePauseBackground();
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
        isPauseRequested = false;
        Time.timeScale = 1f;
        ReleasePauseBackground();
        SceneManager.LoadScene("MainMenu");
    }

    public static Texture2D GetPausedBackgroundTexture()
    {
        return pausedBackgroundTexture;
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

    private IEnumerator PauseGameRoutine()
    {
        isPauseRequested = true;
        yield return new WaitForEndOfFrame();

        CapturePausedBackground();

        Time.timeScale = 0f;
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(PauseSceneName, LoadSceneMode.Additive);
        if (loadOperation == null)
        {
            isPauseSceneLoading = false;
            isPauseRequested = false;
            Debug.LogError($"Could not load scene '{PauseSceneName}'.");
            yield break;
        }

        isPauseSceneLoading = true;
        loadOperation.completed += _ =>
        {
            isPauseSceneLoading = false;
            isPauseRequested = false;
        };
    }

    private void CapturePausedBackground()
    {
        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        if (screenshot == null)
        {
            return;
        }

        ReleasePauseBackground();
        pausedBackgroundTexture = CreateBlurredCopy(screenshot, 4, 3);
        Destroy(screenshot);
    }

    private void ReleasePauseBackground()
    {
        if (pausedBackgroundTexture != null)
        {
            Destroy(pausedBackgroundTexture);
            pausedBackgroundTexture = null;
        }
    }

    private Texture2D CreateBlurredCopy(Texture2D source, int downsampleFactor, int blurIterations)
    {
        int targetWidth = Mathf.Max(1, source.width / Mathf.Max(1, downsampleFactor));
        int targetHeight = Mathf.Max(1, source.height / Mathf.Max(1, downsampleFactor));

        Texture2D downsampled = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[targetWidth * targetHeight];

        for (int y = 0; y < targetHeight; y++)
        {
            float v = targetHeight == 1 ? 0f : y / (float)(targetHeight - 1);
            for (int x = 0; x < targetWidth; x++)
            {
                float u = targetWidth == 1 ? 0f : x / (float)(targetWidth - 1);
                pixels[(y * targetWidth) + x] = source.GetPixelBilinear(u, v);
            }
        }

        for (int iteration = 0; iteration < blurIterations; iteration++)
        {
            pixels = BlurPass(pixels, targetWidth, targetHeight, horizontal: true);
            pixels = BlurPass(pixels, targetWidth, targetHeight, horizontal: false);
        }

        downsampled.SetPixels(pixels);
        downsampled.Apply(false, false);
        return downsampled;
    }

    private Color[] BlurPass(Color[] pixels, int width, int height, bool horizontal)
    {
        Color[] result = new Color[pixels.Length];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color sum = Color.black;
                int samples = 0;

                for (int offset = -1; offset <= 1; offset++)
                {
                    int sampleX = horizontal ? Mathf.Clamp(x + offset, 0, width - 1) : x;
                    int sampleY = horizontal ? y : Mathf.Clamp(y + offset, 0, height - 1);
                    sum += pixels[(sampleY * width) + sampleX];
                    samples++;
                }

                result[(y * width) + x] = sum / Mathf.Max(1, samples);
            }
        }

        return result;
    }
}
