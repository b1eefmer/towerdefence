using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenuButtons : MonoBehaviour
{
    private const string PauseBackgroundName = "PauseBlurBackground";
    private const string PauseFrameName = "PauseFrame";
    private const string PauseBoardName = "PauseBoard";
    private const string PauseTitleName = "PauseTitle";
    private static Sprite cachedWhiteSprite;
    private static Sprite cachedRoundedFrameSprite;
    private static Sprite cachedChalkboardSprite;

    private void Start()
    {
        BuildPauseOverlay();
    }

    public void OnResume()
    {
        PauseManager.Instance.ResumeGame();
    }

   
    public void OnSave()
    {
        PauseManager.Instance.SaveGame();
    }

    public void OnLoad()
    {
        PauseManager.Instance.LoadGame();
    }

 
    public void OnBack()
    {
        PauseManager.Instance.BackToMenu();
    }

    private void BuildPauseOverlay()
    {
        Canvas pauseCanvas = FindPauseCanvas();
        if (pauseCanvas == null)
        {
            return;
        }

        pauseCanvas.overrideSorting = true;
        pauseCanvas.sortingOrder = 500;

        CreateBlurBackground(pauseCanvas.transform);
        DisableLegacyPauseBackground(pauseCanvas.transform);

        RectTransform frame = GetOrCreateFrame(pauseCanvas.transform);
        ConfigureTitle(frame);
        PositionButtons(frame);
    }

    private Canvas FindPauseCanvas()
    {
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
        {
            Canvas canvas = root.GetComponentInChildren<Canvas>(true);
            if (canvas != null)
            {
                return canvas;
            }
        }

        return null;
    }

    private void CreateBlurBackground(Transform canvasTransform)
    {
        RawImage background = canvasTransform.Find(PauseBackgroundName)?.GetComponent<RawImage>();
        if (background == null)
        {
            GameObject backgroundObject = new GameObject(PauseBackgroundName, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            backgroundObject.transform.SetParent(canvasTransform, false);
            backgroundObject.transform.SetSiblingIndex(0);
            background = backgroundObject.GetComponent<RawImage>();
        }

        RectTransform rect = background.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        background.texture = PauseManager.GetPausedBackgroundTexture();
        background.color = background.texture != null
            ? new Color(1f, 1f, 1f, 0.96f)
            : new Color(0f, 0f, 0f, 0.55f);
        background.raycastTarget = false;
    }

    private void DisableLegacyPauseBackground(Transform canvasTransform)
    {
        Transform pauseUiTransform = canvasTransform.Find("PauseUI");
        if (pauseUiTransform == null)
        {
            return;
        }

        Image pauseUiImage = pauseUiTransform.GetComponent<Image>();
        if (pauseUiImage == null)
        {
            return;
        }

        pauseUiImage.enabled = false;
        pauseUiImage.raycastTarget = false;
    }

    private RectTransform GetOrCreateFrame(Transform canvasTransform)
    {
        RectTransform frame = canvasTransform.Find(PauseFrameName) as RectTransform;
        if (frame == null)
        {
            GameObject frameObject = new GameObject(PauseFrameName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline));
            frameObject.transform.SetParent(canvasTransform, false);
            frame = frameObject.GetComponent<RectTransform>();
        }

        frame.anchorMin = new Vector2(0.5f, 0.5f);
        frame.anchorMax = new Vector2(0.5f, 0.5f);
        frame.pivot = new Vector2(0.5f, 0.5f);
        frame.sizeDelta = new Vector2(840f, 560f);
        frame.anchoredPosition = new Vector2(0f, 8f);

        Image frameImage = frame.GetComponent<Image>();
        frameImage.sprite = GetRoundedFrameSprite();
        frameImage.type = Image.Type.Sliced;
        frameImage.color = new Color(0.03f, 0.03f, 0.03f, 0.98f);
        frameImage.raycastTarget = false;

        Outline outline = frame.GetComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.35f);
        outline.effectDistance = new Vector2(1f, 1f);
        outline.useGraphicAlpha = true;

        ConfigureBoardSurface(frame);

        return frame;
    }

    private void ConfigureTitle(RectTransform frame)
    {
        TextMeshProUGUI title = frame.Find(PauseTitleName)?.GetComponent<TextMeshProUGUI>();
        if (title == null)
        {
            GameObject titleObject = new GameObject(PauseTitleName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            titleObject.transform.SetParent(frame, false);
            title = titleObject.GetComponent<TextMeshProUGUI>();
        }

        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.sizeDelta = new Vector2(320f, 80f);
        titleRect.anchoredPosition = new Vector2(0f, -56f);

        title.text = "PAUSED";
        if (TMP_Settings.defaultFontAsset != null)
        {
            title.font = TMP_Settings.defaultFontAsset;
        }
        title.fontSize = 44f;
        title.alignment = TextAlignmentOptions.Center;
        title.color = new Color(0.93f, 0.97f, 0.93f, 1f);
        title.raycastTarget = false;
    }

    private void PositionButtons(RectTransform frame)
    {
        PositionButton(frame, "ResumeButton (1)", new Vector2(0f, 104f), true);
        PositionButton(frame, "SaveButton", new Vector2(0f, 10f), true);
        PositionButton(frame, "BackButton", new Vector2(0f, -84f), true);
    }

    private void PositionButton(RectTransform frame, string buttonName, Vector2 anchoredPosition, bool active)
    {
        Transform buttonTransform = frame.parent.Find(buttonName);
        if (buttonTransform == null)
        {
            return;
        }

        RectTransform buttonRect = buttonTransform as RectTransform;
        if (buttonRect == null)
        {
            return;
        }

        buttonRect.SetParent(frame, false);
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = anchoredPosition;
        buttonRect.sizeDelta = new Vector2(230f, 70f);
        buttonRect.gameObject.SetActive(active);
    }

    private void ConfigureBoardSurface(RectTransform frame)
    {
        Transform existingBoard = frame.Find(PauseBoardName);
        Image board;

        if (existingBoard == null)
        {
            GameObject boardObject = new GameObject(PauseBoardName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            boardObject.transform.SetParent(frame, false);
            boardObject.transform.SetSiblingIndex(0);
            board = boardObject.GetComponent<Image>();
        }
        else
        {
            board = existingBoard.GetComponent<Image>();
        }

        RectTransform boardRect = board.rectTransform;
        boardRect.anchorMin = new Vector2(0f, 0f);
        boardRect.anchorMax = new Vector2(1f, 1f);
        boardRect.offsetMin = new Vector2(20f, 20f);
        boardRect.offsetMax = new Vector2(-20f, -20f);
        boardRect.anchoredPosition = Vector2.zero;

        board.sprite = GetChalkboardSprite();
        board.type = Image.Type.Sliced;
        board.color = Color.white;
        board.raycastTarget = false;
    }

    private Sprite GetWhiteSprite()
    {
        if (cachedWhiteSprite == null)
        {
            cachedWhiteSprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
                new Vector2(0.5f, 0.5f));
        }

        return cachedWhiteSprite;
    }

    private Sprite GetRoundedFrameSprite()
    {
        if (cachedRoundedFrameSprite == null)
        {
            cachedRoundedFrameSprite = CreateRoundedRectSprite(
                256,
                256,
                26f,
                _ => Color.white);
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
