using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class LevelMananger : MonoBehaviour
{
    public static LevelMananger main;
    private const string FireSceneName = "FireScene";
    private const string RuntimeBaseName = "Base";
    private static Sprite cachedFireBaseSprite;
    private static Sprite cachedHeartSprite;
    private static Sprite cachedRuntimeMarkerSprite;

    public Transform startPoint;
    public Transform[] path;

    public int currency;

    private Button startWaveButton;
    private Button menuToggleButton;
    private Button pauseButton;
    private readonly List<Button> towerSelectionButtons = new();
    private EnemySpawner cachedEnemySpawner;
    private TextMeshProUGUI hudCurrencyText;
    private int displayedCurrency = int.MinValue;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        main = this;
    }
    void Start()
    {
        currency = 1000;
        EnsureFireSceneBase();
    }
    public void IncreaseCurrency(int amount)
    {
        currency += amount;
    }

    public void SetCurrency(int amount)
    {
        currency = Mathf.Max(0, amount);
    }

    public bool SpendCurrency(int amount)
    {
        if (amount <= currency)
        {
            // BUY ITEM
            currency -= amount;
            return true;
        }
        else
        {
            Debug.Log("You do not have enough to purchase this item");
            return false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleGameplayUiFallback();
        UpdateHudCurrency();
        PositionRuntimeHeartUi();
    }

    private void HandleGameplayUiFallback()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != "SampleScene" && sceneName != "FireScene")
        {
            return;
        }

        if (SceneManager.GetSceneByName("PauseScene").isLoaded)
        {
            return;
        }

        if (!TryGetPointerDownPosition(out Vector2 screenPosition))
        {
            return;
        }

        CacheGameplayUiReferences();

        if (TryInvokeButtonAtScreenPoint(pauseButton, screenPosition))
        {
            return;
        }

        if (TryInvokeButtonAtScreenPoint(menuToggleButton, screenPosition))
        {
            return;
        }

        if (TryInvokeTowerSelectionButton(screenPosition))
        {
            return;
        }

        if (!IsButtonAtScreenPoint(startWaveButton, screenPosition))
        {
            return;
        }

        if (startWaveButton != null)
        {
            startWaveButton.onClick.Invoke();
        }

        cachedEnemySpawner ??= FindFirstObjectByType<EnemySpawner>();
        if (cachedEnemySpawner != null && !cachedEnemySpawner.IsSpawning())
        {
            cachedEnemySpawner.StartWaveFromButton();
        }
    }

    private void CacheGameplayUiReferences()
    {
        if (startWaveButton == null)
        {
            startWaveButton = FindButton("Start");
        }

        if (menuToggleButton == null)
        {
            menuToggleButton = FindButton("Menu Toggle");
        }

        if (pauseButton == null)
        {
            pauseButton = FindButton("PauseGameButton");
        }

        CacheTowerSelectionButtons();
    }

    private Button FindButton(string objectName)
    {
        GameObject targetObject = GameObject.Find(objectName);
        return targetObject != null ? targetObject.GetComponent<Button>() : null;
    }

    private bool TryInvokeButtonAtScreenPoint(Button button, Vector2 screenPosition)
    {
        if (!IsButtonAtScreenPoint(button, screenPosition))
        {
            return false;
        }

        button.onClick.Invoke();
        return true;
    }

    private bool IsButtonAtScreenPoint(Button button, Vector2 screenPosition)
    {
        if (button == null || !button.isActiveAndEnabled || !button.gameObject.activeInHierarchy || !button.interactable)
        {
            return false;
        }

        RectTransform rectTransform = button.transform as RectTransform;
        if (rectTransform == null)
        {
            return false;
        }

        Canvas canvas = button.GetComponentInParent<Canvas>();
        Camera eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPosition, eventCamera);
    }

    private bool TryGetPointerDownPosition(out Vector2 screenPosition)
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            screenPosition = Mouse.current.position.ReadValue();
            return true;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            return true;
        }
#endif

        if (Input.GetMouseButtonDown(0))
        {
            screenPosition = Input.mousePosition;
            return true;
        }

        screenPosition = default;
        return false;
    }

    private void CacheTowerSelectionButtons()
    {
        if (towerSelectionButtons.Count > 0)
        {
            towerSelectionButtons.RemoveAll(button => button == null);
            if (towerSelectionButtons.Count > 0)
            {
                return;
            }
        }

        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (Button button in allButtons)
        {
            if (button == null || !button.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (!IsTowerSelectionButton(button))
            {
                continue;
            }

            towerSelectionButtons.Add(button);
        }
    }

    private bool TryInvokeTowerSelectionButton(Vector2 screenPosition)
    {
        CacheTowerSelectionButtons();

        foreach (Button towerButton in towerSelectionButtons)
        {
            if (!TryInvokeButtonAtScreenPoint(towerButton, screenPosition))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private bool IsTowerSelectionButton(Button button)
    {
        if (button != null)
        {
            string buttonName = button.name;
            if (buttonName == "Basic Turret" || buttonName == "Sniper Turret" || buttonName == "Slowmo Turret")
            {
                return true;
            }
        }

        Button.ButtonClickedEvent onClick = button.onClick;
        int persistentEventCount = onClick.GetPersistentEventCount();
        for (int i = 0; i < persistentEventCount; i++)
        {
            if (onClick.GetPersistentMethodName(i) != "SetSelectedTower")
            {
                continue;
            }

            Object target = onClick.GetPersistentTarget(i);
            if (target is BuildMananger)
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateHudCurrency()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != "SampleScene" && sceneName != "FireScene")
        {
            return;
        }

        EnsureHudCurrencyText();
        if (hudCurrencyText == null || displayedCurrency == currency)
        {
            return;
        }

        displayedCurrency = currency;
        hudCurrencyText.text = $"Gold: {currency}";
    }

    private void EnsureHudCurrencyText()
    {
        RectTransform menuRect = null;
        GameObject menuObject = GameObject.Find("Menu");
        if (menuObject != null)
        {
            menuRect = menuObject.GetComponent<RectTransform>();
        }

        GameObject existingHudObject = GameObject.Find("HudCurrencyText");
        if (existingHudObject != null)
        {
            hudCurrencyText = existingHudObject.GetComponent<TextMeshProUGUI>();
        }

        if (hudCurrencyText != null)
        {
            PositionHudCurrencyText(menuRect);
            return;
        }

        TextMeshProUGUI styleSource = FindHudTextStyleSource();
        if (styleSource == null)
        {
            return;
        }

        Transform hudParent = ResolveHudParent(menuRect, styleSource);

        if (hudParent == null)
        {
            return;
        }

        GameObject textObject = new GameObject("HudCurrencyText");
        textObject.layer = styleSource.gameObject.layer;
        textObject.transform.SetParent(hudParent, false);

        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(1f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(-95f, -95f);
        rectTransform.sizeDelta = new Vector2(220f, 50f);

        hudCurrencyText = textObject.AddComponent<TextMeshProUGUI>();
        CopyTextStyle(styleSource, hudCurrencyText);
        hudCurrencyText.alignment = TextAlignmentOptions.Left;
        hudCurrencyText.text = $"Gold: {currency}";
        displayedCurrency = currency;
        PositionHudCurrencyText(menuRect);
    }

    private TextMeshProUGUI FindHudTextStyleSource()
    {
        string[] candidateNames = { "WaveText", "Currency", "GoldText (1)", "GoldText" };
        foreach (string candidateName in candidateNames)
        {
            GameObject candidateObject = GameObject.Find(candidateName);
            if (candidateObject == null)
            {
                continue;
            }

            TextMeshProUGUI candidateText = candidateObject.GetComponent<TextMeshProUGUI>();
            if (candidateText != null)
            {
                return candidateText;
            }
        }

        return FindFirstObjectByType<TextMeshProUGUI>();
    }

    private Transform ResolveHudParent(RectTransform menuRect, TextMeshProUGUI styleSource)
    {
        if (menuRect != null && menuRect.parent != null)
        {
            return menuRect.parent;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            return canvas.transform;
        }

        if (styleSource.transform.parent != null && styleSource.transform.parent.parent != null)
        {
            return styleSource.transform.parent.parent;
        }

        return styleSource.transform.parent;
    }

    private void CopyTextStyle(TextMeshProUGUI source, TextMeshProUGUI target)
    {
        target.font = source.font;
        target.fontSharedMaterial = source.fontSharedMaterial;
        target.fontSize = source.fontSize;
        target.fontStyle = source.fontStyle;
        target.color = source.color;
        target.enableAutoSizing = false;
        target.raycastTarget = false;
        target.horizontalAlignment = HorizontalAlignmentOptions.Left;
        target.verticalAlignment = VerticalAlignmentOptions.Middle;
    }

    private void PositionHudCurrencyText(RectTransform menuRect)
    {
        if (hudCurrencyText == null)
        {
            return;
        }

        RectTransform rectTransform = hudCurrencyText.rectTransform;
        if (menuRect != null)
        {
            Transform fixedParent = menuRect.parent;
            if (fixedParent != null && rectTransform.parent != fixedParent)
            {
                rectTransform.SetParent(fixedParent, false);
            }

            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(menuRect.rect.width + 20f, -40f);
            rectTransform.sizeDelta = new Vector2(220f, 50f);
            return;
        }

        rectTransform.anchorMin = new Vector2(1f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(1f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(-95f, -95f);
        rectTransform.sizeDelta = new Vector2(220f, 50f);
    }

    private void EnsureFireSceneBase()
    {
        if (SceneManager.GetActiveScene().name != FireSceneName)
        {
            return;
        }

        if (FindFirstObjectByType<BaseHealth>() != null)
        {
            return;
        }

        GameObject baseObject = new GameObject(RuntimeBaseName);
        baseObject.transform.position = ResolveFireSceneBasePosition();
        baseObject.transform.localScale = new Vector3(1.15f, 1.15f, 1f);

        SpriteRenderer spriteRenderer = baseObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = LoadSpriteFromProjectAsset(
            ref cachedFireBaseSprite,
            Path.Combine(Application.dataPath, "03ee3d07-500b-4365-85a5-85e5aad4191f.png"),
            100f);
        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        spriteRenderer.sortingLayerName = "Default";
        spriteRenderer.sortingOrder = 60;

        CreateRuntimeBaseVisuals(baseObject.transform, spriteRenderer);

        BaseHealth baseHealth = baseObject.AddComponent<BaseHealth>();
        Image[] runtimeHearts = CreateRuntimeHeartUi();
        baseHealth.ConfigureRuntimeSetup(runtimeHearts, 4);
    }

    private Vector3 ResolveFireSceneBasePosition()
    {
        string[] routeNames = { "Path", "Path (1)", "Path (2)", "Path 1", "Path 2", "Path 3" };
        List<Vector3> routeEndpoints = new List<Vector3>();

        foreach (string routeName in routeNames)
        {
            GameObject routeObject = GameObject.Find(routeName);
            if (routeObject == null || routeObject.transform.childCount == 0)
            {
                continue;
            }

            Transform endpoint = null;
            for (int i = routeObject.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = routeObject.transform.GetChild(i);
                if (child == null || child.name.ToLowerInvariant().Contains("start"))
                {
                    continue;
                }

                endpoint = child;
                break;
            }

            if (endpoint != null)
            {
                routeEndpoints.Add(endpoint.position);
            }
        }

        if (routeEndpoints.Count == 0)
        {
            return new Vector3(20.5f, 1.7f, 0f);
        }

        Vector3 averagedPosition = Vector3.zero;
        for (int i = 0; i < routeEndpoints.Count; i++)
        {
            averagedPosition += routeEndpoints[i];
        }

        return averagedPosition / routeEndpoints.Count;
    }

    private Image[] CreateRuntimeHeartUi()
    {
        GameObject existingContainer = GameObject.Find("RuntimeBaseHearts");
        if (existingContainer != null)
        {
            return existingContainer.GetComponentsInChildren<Image>(true);
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            return new Image[0];
        }

        Sprite heartSprite = LoadSpriteFromProjectAsset(
            ref cachedHeartSprite,
            Path.Combine(Application.dataPath, "Heart.png"),
            100f);

        GameObject container = new GameObject("RuntimeBaseHearts");
        RectTransform containerRect = container.AddComponent<RectTransform>();
        container.transform.SetParent(canvas.transform, false);
        containerRect.anchorMin = new Vector2(0f, 1f);
        containerRect.anchorMax = new Vector2(0f, 1f);
        containerRect.pivot = new Vector2(0f, 0.5f);
        containerRect.anchoredPosition = new Vector2(300f, -40f);
        containerRect.sizeDelta = new Vector2(170f, 48f);

        List<Image> createdHearts = new List<Image>();
        for (int i = 0; i < 4; i++)
        {
            GameObject heartObject = new GameObject($"Heart {i + 1}");
            RectTransform heartRect = heartObject.AddComponent<RectTransform>();
            heartObject.transform.SetParent(container.transform, false);
            heartRect.anchorMin = new Vector2(0f, 1f);
            heartRect.anchorMax = new Vector2(0f, 1f);
            heartRect.pivot = new Vector2(0f, 1f);
            heartRect.anchoredPosition = new Vector2(i * 42f, 0f);
            heartRect.sizeDelta = new Vector2(36f, 36f);

            Image heartImage = heartObject.AddComponent<Image>();
            heartImage.sprite = heartSprite;
            heartImage.preserveAspect = true;
            createdHearts.Add(heartImage);
        }

        return createdHearts.ToArray();
    }

    private void PositionRuntimeHeartUi()
    {
        if (SceneManager.GetActiveScene().name != FireSceneName)
        {
            return;
        }

        GameObject heartContainerObject = GameObject.Find("RuntimeBaseHearts");
        if (heartContainerObject == null)
        {
            return;
        }

        RectTransform heartContainerRect = heartContainerObject.GetComponent<RectTransform>();
        if (heartContainerRect == null)
        {
            return;
        }

        RectTransform currencyRect = hudCurrencyText != null
            ? hudCurrencyText.rectTransform
            : GameObject.Find("HudCurrencyText")?.GetComponent<RectTransform>();

        if (currencyRect != null)
        {
            Transform fixedParent = currencyRect.parent;
            if (fixedParent != null && heartContainerRect.parent != fixedParent)
            {
                heartContainerRect.SetParent(fixedParent, false);
            }

            heartContainerRect.anchorMin = new Vector2(0f, 1f);
            heartContainerRect.anchorMax = new Vector2(0f, 1f);
            heartContainerRect.pivot = new Vector2(0f, 0.5f);
            heartContainerRect.anchoredPosition = new Vector2(
                currencyRect.anchoredPosition.x + currencyRect.sizeDelta.x - 10f,
                currencyRect.anchoredPosition.y);
            heartContainerRect.sizeDelta = new Vector2(170f, 48f);
            return;
        }

        heartContainerRect.anchorMin = new Vector2(1f, 1f);
        heartContainerRect.anchorMax = new Vector2(1f, 1f);
        heartContainerRect.pivot = new Vector2(1f, 0.5f);
        heartContainerRect.anchoredPosition = new Vector2(-24f, -40f);
        heartContainerRect.sizeDelta = new Vector2(170f, 48f);
    }

    private void CreateRuntimeBaseVisuals(Transform baseTransform, SpriteRenderer baseSpriteRenderer)
    {
        if (baseTransform == null)
        {
            return;
        }

        baseSpriteRenderer.color = new Color(1f, 1f, 1f, 1f);

        GameObject highlightObject = new GameObject("BaseHighlight");
        highlightObject.transform.SetParent(baseTransform, false);
        highlightObject.transform.localPosition = new Vector3(0f, 0f, 0.1f);
        highlightObject.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        highlightObject.transform.localScale = new Vector3(3.6f, 3.6f, 1f);

        SpriteRenderer highlightRenderer = highlightObject.AddComponent<SpriteRenderer>();
        highlightRenderer.sprite = CreateFallbackSprite(ref cachedRuntimeMarkerSprite, 100f);
        highlightRenderer.color = new Color(1f, 0.78f, 0.18f, 0.35f);
        highlightRenderer.sortingLayerName = "Default";
        highlightRenderer.sortingOrder = 58;

        GameObject frameObject = new GameObject("BaseFrame");
        frameObject.transform.SetParent(baseTransform, false);
        frameObject.transform.localPosition = new Vector3(0f, 0f, 0.05f);
        frameObject.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        frameObject.transform.localScale = new Vector3(2.6f, 2.6f, 1f);

        SpriteRenderer frameRenderer = frameObject.AddComponent<SpriteRenderer>();
        frameRenderer.sprite = CreateFallbackSprite(ref cachedRuntimeMarkerSprite, 100f);
        frameRenderer.color = new Color(1f, 0.95f, 0.72f, 0.16f);
        frameRenderer.sortingLayerName = "Default";
        frameRenderer.sortingOrder = 59;
    }

    private Sprite LoadSpriteFromProjectAsset(ref Sprite cache, string filePath, float pixelsPerUnit)
    {
        if (cache != null)
        {
            return cache;
        }

        if (!File.Exists(filePath))
        {
            return CreateFallbackSprite(ref cache, pixelsPerUnit);
        }

        byte[] imageBytes = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
        if (!texture.LoadImage(imageBytes))
        {
            Object.Destroy(texture);
            return CreateFallbackSprite(ref cache, pixelsPerUnit);
        }

        texture.name = Path.GetFileNameWithoutExtension(filePath);
        cache = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit);
        cache.name = texture.name;
        return cache;
    }

    private Sprite CreateFallbackSprite(ref Sprite cache, float pixelsPerUnit)
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        cache = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        return cache;
    }
}
