using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class LevelMananger : MonoBehaviour
{
    public static LevelMananger main;

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
}
