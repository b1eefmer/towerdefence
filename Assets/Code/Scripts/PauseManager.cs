using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    private const string PauseOverlayName = "PauseOverlayRoot";
    private const string PauseBackgroundName = "PauseBlurBackground";
    private const string PauseFrameName = "PauseFrame";
    private const string PauseBoardName = "PauseBoard";
    private const string PauseTitleName = "PauseTitle";

    private bool isPauseRequested;
    private bool isPaused;
    private static Texture2D pausedBackgroundTexture;
    private static Sprite cachedRoundedFrameSprite;
    private static Sprite cachedChalkboardSprite;
    private static Sprite cachedButtonSprite;

    private GameObject pauseOverlayRoot;
    private readonly List<GameObject> hiddenGameplayHudObjects = new();

    public bool IsPaused => isPaused;

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        BindPauseButtonsRuntime();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            ReleasePauseBackground();
            Instance = null;
        }
    }

    public void PauseGame()
    {
        if (!CanPauseCurrentScene() || isPaused || isPauseRequested)
        {
            return;
        }

        StartCoroutine(PauseGameRoutine());
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPauseRequested = false;
        isPaused = false;
        DestroyPauseOverlay();
        SetGameplayHudButtonsVisible(true);
        ReleasePauseBackground();
    }

    public void SaveGame()
    {
        SaveSystem.SaveGame();
    }

    public void LoadGame()
    {
        if (isPaused)
        {
            ResumeGame();
        }

        SaveSystem.LoadGame();
    }

    public void BackToMenu()
    {
        ResumeGame();
        SceneManager.LoadScene("MainMenu");
    }

    public static Texture2D GetPausedBackgroundTexture()
    {
        return pausedBackgroundTexture;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        isPauseRequested = false;
        isPaused = false;
        DestroyPauseOverlay();
        SetGameplayHudButtonsVisible(true);
        ReleasePauseBackground();
        BindPauseButtonsRuntime();
    }

    private bool CanPauseCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        return sceneName == "SampleScene" || sceneName == "FireScene";
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

        if (!CreatePauseOverlay())
        {
            isPauseRequested = false;
            ReleasePauseBackground();
            yield break;
        }

        SetGameplayHudButtonsVisible(false);
        Time.timeScale = 0f;
        isPaused = true;
        isPauseRequested = false;
    }

    private bool CreatePauseOverlay()
    {
        DestroyPauseOverlay();

        Canvas pauseCanvas = FindPauseCanvas();
        if (pauseCanvas == null)
        {
            Debug.LogWarning("Pause failed: no active Canvas was found in the gameplay scene.");
            return false;
        }

        pauseOverlayRoot = new GameObject(
            PauseOverlayName,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(GraphicRaycaster),
            typeof(CanvasScaler));
        pauseOverlayRoot.transform.SetParent(pauseCanvas.transform, false);

        RectTransform overlayRect = pauseOverlayRoot.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Canvas overlayCanvas = pauseOverlayRoot.GetComponent<Canvas>();
        overlayCanvas.overrideSorting = true;
        overlayCanvas.sortingOrder = 5000;
        overlayCanvas.renderMode = pauseCanvas.renderMode;
        overlayCanvas.worldCamera = pauseCanvas.worldCamera;
        overlayCanvas.sortingLayerID = pauseCanvas.sortingLayerID;
        overlayCanvas.planeDistance = pauseCanvas.planeDistance;
        ConfigureOverlayScaler(pauseCanvas, pauseOverlayRoot.GetComponent<CanvasScaler>());

        CreateBlurBackground(pauseOverlayRoot.transform);
        RectTransform frame = CreateFrame(pauseOverlayRoot.transform);
        CreateTitle(frame);
        CreateButton(frame, "ResumeButton", "Resume", new Vector2(0f, 96f), ResumeGame);
        CreateButton(frame, "SaveButton", "Save", new Vector2(0f, 12f), SaveGame);
        CreateButton(frame, "BackButton", "Back", new Vector2(0f, -72f), BackToMenu);

        return true;
    }

    private Canvas FindPauseCanvas()
    {
        Button pauseButton = FindPauseButton();
        if (pauseButton != null)
        {
            Canvas buttonCanvas = pauseButton.GetComponentInParent<Canvas>();
            if (buttonCanvas != null)
            {
                return buttonCanvas.rootCanvas != null ? buttonCanvas.rootCanvas : buttonCanvas;
            }
        }

        Scene activeScene = SceneManager.GetActiveScene();
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Canvas fallbackCanvas = null;

        foreach (Canvas canvas in canvases)
        {
            if (canvas == null || canvas.gameObject.scene != activeScene)
            {
                continue;
            }

            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay && canvas.isRootCanvas)
            {
                return canvas;
            }

            if (fallbackCanvas == null && canvas.isRootCanvas)
            {
                fallbackCanvas = canvas;
            }
        }

        return fallbackCanvas;
    }

    private Button FindPauseButton()
    {
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (Button button in allButtons)
        {
            if (button != null && button.name == "PauseGameButton")
            {
                return button;
            }
        }

        return null;
    }

    private void ConfigureOverlayScaler(Canvas sourceCanvas, CanvasScaler overlayScaler)
    {
        if (overlayScaler == null)
        {
            return;
        }

        CanvasScaler sourceScaler = sourceCanvas != null ? sourceCanvas.GetComponent<CanvasScaler>() : null;
        if (sourceScaler == null)
        {
            overlayScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            overlayScaler.scaleFactor = 1f;
            overlayScaler.referencePixelsPerUnit = 100f;
            return;
        }

        overlayScaler.uiScaleMode = sourceScaler.uiScaleMode;
        overlayScaler.referencePixelsPerUnit = sourceScaler.referencePixelsPerUnit;
        overlayScaler.scaleFactor = sourceScaler.scaleFactor;
        overlayScaler.referenceResolution = sourceScaler.referenceResolution;
        overlayScaler.screenMatchMode = sourceScaler.screenMatchMode;
        overlayScaler.matchWidthOrHeight = sourceScaler.matchWidthOrHeight;
        overlayScaler.physicalUnit = sourceScaler.physicalUnit;
        overlayScaler.fallbackScreenDPI = sourceScaler.fallbackScreenDPI;
        overlayScaler.defaultSpriteDPI = sourceScaler.defaultSpriteDPI;
        overlayScaler.dynamicPixelsPerUnit = sourceScaler.dynamicPixelsPerUnit;
    }

    private void CreateBlurBackground(Transform parent)
    {
        GameObject backgroundObject = new GameObject(PauseBackgroundName, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        backgroundObject.transform.SetParent(parent, false);

        RawImage background = backgroundObject.GetComponent<RawImage>();
        RectTransform rect = background.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        background.texture = pausedBackgroundTexture;
        background.color = background.texture != null
            ? new Color(1f, 1f, 1f, 0.96f)
            : new Color(0f, 0f, 0f, 0.6f);
        background.raycastTarget = true;
    }

    private RectTransform CreateFrame(Transform parent)
    {
        GameObject frameObject = new GameObject(PauseFrameName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline));
        frameObject.transform.SetParent(parent, false);

        RectTransform frame = frameObject.GetComponent<RectTransform>();
        frame.anchorMin = new Vector2(0.5f, 0.5f);
        frame.anchorMax = new Vector2(0.5f, 0.5f);
        frame.pivot = new Vector2(0.5f, 0.5f);
        frame.sizeDelta = new Vector2(760f, 500f);
        frame.anchoredPosition = new Vector2(0f, 8f);

        Image frameImage = frameObject.GetComponent<Image>();
        frameImage.sprite = GetRoundedFrameSprite();
        frameImage.type = Image.Type.Sliced;
        frameImage.color = new Color(0.02f, 0.02f, 0.02f, 0.98f);
        frameImage.raycastTarget = true;

        Outline outline = frameObject.GetComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.42f);
        outline.effectDistance = new Vector2(1f, 1f);
        outline.useGraphicAlpha = true;

        ConfigureBoardSurface(frame);
        return frame;
    }

    private void ConfigureBoardSurface(RectTransform frame)
    {
        GameObject boardObject = new GameObject(PauseBoardName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        boardObject.transform.SetParent(frame, false);
        boardObject.transform.SetSiblingIndex(0);

        Image board = boardObject.GetComponent<Image>();
        RectTransform boardRect = board.rectTransform;
        boardRect.anchorMin = Vector2.zero;
        boardRect.anchorMax = Vector2.one;
        boardRect.offsetMin = new Vector2(18f, 18f);
        boardRect.offsetMax = new Vector2(-18f, -18f);

        board.sprite = GetChalkboardSprite();
        board.type = Image.Type.Sliced;
        board.color = Color.white;
        board.raycastTarget = false;
    }

    private void CreateTitle(RectTransform frame)
    {
        GameObject titleObject = new GameObject(PauseTitleName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        titleObject.transform.SetParent(frame, false);

        TextMeshProUGUI title = titleObject.GetComponent<TextMeshProUGUI>();
        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.sizeDelta = new Vector2(320f, 80f);
        titleRect.anchoredPosition = new Vector2(0f, -54f);

        title.text = "PAUSED";
        if (TMP_Settings.defaultFontAsset != null)
        {
            title.font = TMP_Settings.defaultFontAsset;
        }

        title.fontSize = 42f;
        title.color = new Color(0.92f, 0.96f, 0.92f, 1f);
        title.alignment = TextAlignmentOptions.Center;
        title.raycastTarget = false;
    }

    private void CreateButton(RectTransform frame, string buttonName, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(buttonName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(Outline));
        buttonObject.transform.SetParent(frame, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = anchoredPosition;
        buttonRect.sizeDelta = new Vector2(240f, 58f);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.sprite = GetButtonSprite();
        buttonImage.type = Image.Type.Sliced;
        buttonImage.color = new Color(0.12f, 0.25f, 0.18f, 0.96f);

        Outline buttonOutline = buttonObject.GetComponent<Outline>();
        buttonOutline.effectColor = new Color(0.64f, 0.74f, 0.67f, 0.38f);
        buttonOutline.effectDistance = new Vector2(1f, 1f);
        buttonOutline.useGraphicAlpha = true;

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(onClick);
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.94f);
        colors.pressedColor = new Color(0.86f, 0.92f, 0.88f, 0.88f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.65f, 0.65f, 0.65f, 0.65f);
        button.colors = colors;

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(buttonObject.transform, false);

        TextMeshProUGUI buttonLabel = labelObject.GetComponent<TextMeshProUGUI>();
        RectTransform labelRect = buttonLabel.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        buttonLabel.text = label;
        if (TMP_Settings.defaultFontAsset != null)
        {
            buttonLabel.font = TMP_Settings.defaultFontAsset;
        }

        buttonLabel.fontSize = 28f;
        buttonLabel.alignment = TextAlignmentOptions.Center;
        buttonLabel.color = new Color(0.95f, 0.98f, 0.95f, 1f);
        buttonLabel.raycastTarget = false;
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

    private void SetGameplayHudButtonsVisible(bool visible)
    {
        if (!visible)
        {
            CacheAndHideButton("PauseGameButton");
            return;
        }

        RestoreHiddenGameplayHudObjects();
    }

    private void CacheAndHideButton(string buttonName)
    {
        GameObject buttonObject = GameObject.Find(buttonName);
        if (buttonObject == null)
        {
            return;
        }

        if (!hiddenGameplayHudObjects.Contains(buttonObject))
        {
            hiddenGameplayHudObjects.Add(buttonObject);
        }

        buttonObject.SetActive(false);
    }

    private void RestoreHiddenGameplayHudObjects()
    {
        for (int i = hiddenGameplayHudObjects.Count - 1; i >= 0; i--)
        {
            GameObject hiddenObject = hiddenGameplayHudObjects[i];
            if (hiddenObject == null)
            {
                hiddenGameplayHudObjects.RemoveAt(i);
                continue;
            }

            hiddenObject.SetActive(true);
        }

        hiddenGameplayHudObjects.Clear();
    }

    private void DestroyPauseOverlay()
    {
        if (pauseOverlayRoot != null)
        {
            Destroy(pauseOverlayRoot);
            pauseOverlayRoot = null;
        }
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
            pixels = BlurPass(pixels, targetWidth, targetHeight, true);
            pixels = BlurPass(pixels, targetWidth, targetHeight, false);
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

    private Sprite GetRoundedFrameSprite()
    {
        if (cachedRoundedFrameSprite == null)
        {
            cachedRoundedFrameSprite = CreateRoundedRectSprite(256, 256, 26f, _ => Color.white);
        }

        return cachedRoundedFrameSprite;
    }

    private Sprite GetChalkboardSprite()
    {
        if (cachedChalkboardSprite == null)
        {
            cachedChalkboardSprite = CreateRoundedRectSprite(
                256,
                256,
                22f,
                uv =>
                {
                    float dx = (uv.x - 0.5f) / 0.5f;
                    float dy = (uv.y - 0.5f) / 0.5f;
                    float radial = Mathf.Clamp01(1f - Mathf.Sqrt((dx * dx) + (dy * dy)));
                    float softCenter = Mathf.SmoothStep(0f, 1f, radial);
                    float noise = Mathf.PerlinNoise((uv.x * 7.5f) + 0.17f, (uv.y * 7.5f) + 0.41f) * 0.06f;

                    Color edge = new Color(0.06f, 0.14f, 0.1f, 1f);
                    Color center = new Color(0.18f, 0.34f, 0.24f, 1f);
                    Color baseColor = Color.Lerp(edge, center, softCenter);
                    return new Color(
                        Mathf.Clamp01(baseColor.r + noise),
                        Mathf.Clamp01(baseColor.g + noise),
                        Mathf.Clamp01(baseColor.b + noise),
                        1f);
                });
        }

        return cachedChalkboardSprite;
    }

    private Sprite GetButtonSprite()
    {
        if (cachedButtonSprite == null)
        {
            cachedButtonSprite = CreateRoundedRectSprite(
                128,
                64,
                18f,
                uv =>
                {
                    float vertical = Mathf.Lerp(0.85f, 1f, uv.y);
                    Color top = new Color(0.18f, 0.32f, 0.24f, 1f);
                    Color bottom = new Color(0.1f, 0.2f, 0.14f, 1f);
                    return Color.Lerp(bottom, top, vertical);
                });
        }

        return cachedButtonSprite;
    }

    private Sprite CreateRoundedRectSprite(int width, int height, float cornerRadius, System.Func<Vector2, Color> colorProvider)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float u = x / (float)(width - 1);
                float v = y / (float)(height - 1);
                Color color = colorProvider(new Vector2(u, v));

                if (!IsInsideRoundedRect(x + 0.5f, y + 0.5f, width, height, cornerRadius))
                {
                    color.a = 0f;
                }

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply(false, false);
        return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    private bool IsInsideRoundedRect(float x, float y, float width, float height, float radius)
    {
        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;
        float localX = Mathf.Abs(x - halfWidth);
        float localY = Mathf.Abs(y - halfHeight);

        float innerX = Mathf.Max(localX - (halfWidth - radius), 0f);
        float innerY = Mathf.Max(localY - (halfHeight - radius), 0f);
        return (innerX * innerX) + (innerY * innerY) <= radius * radius;
    }
}
