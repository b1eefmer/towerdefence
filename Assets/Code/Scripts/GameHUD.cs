using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class GameHUD : MonoBehaviour
{
    private GameObject canvasObject;
    private TextMeshProUGUI goldText;
    private TextMeshProUGUI healthText;
    private TextMeshProUGUI waveText;
    private Button startWaveButton;

    private EnemySpawner spawner;
    private BaseHealth baseHealth;

    private void Start()
    {
        spawner = FindFirstObjectByType<EnemySpawner>();
        baseHealth = FindFirstObjectByType<BaseHealth>();

        CreateHUD();
        
        // Спробуємо знайти старе меню і сховати його, щоб не заважало
        GameObject oldMenu = GameObject.Find("Menu");
        if (oldMenu != null)
        {
            CanvasGroup group = oldMenu.GetComponent<CanvasGroup>();
            if (group == null) group = oldMenu.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }
    }

    private void Update()
    {
        // Оновлюємо дані на нашому красивому HUD
        if (LevelMananger.main != null && goldText != null)
        {
            goldText.text = $"<color=#FFD700>Gold:</color> {LevelMananger.main.currency}";
        }

        if (baseHealth != null && healthText != null)
        {
            healthText.text = $"<color=#FF4444>Base HP:</color> {baseHealth.GetLives()}";
        }

        if (spawner != null && waveText != null)
        {
            waveText.text = $"Wave: {spawner.CurrentWave} / {spawner.TotalWaves}";
        }
    }

    private void CreateHUD()
    {
        canvasObject = new GameObject("Glassmorphism HUD");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        CreateTopBar(canvasObject.transform);
        CreateBottomDock(canvasObject.transform);
    }

    private void CreateTopBar(Transform parent)
    {
        GameObject topBar = CreateUIObject("Top Bar", parent);
        Image image = topBar.AddComponent<Image>();
        image.color = new Color(0.02f, 0.05f, 0.1f, 0.6f); // Glass

        RectTransform rect = topBar.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, 80f);

        Outline outline = topBar.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0.75f, 0.85f, 0.4f);
        outline.effectDistance = new Vector2(0f, -2f);

        // Health
        healthText = CreateText(topBar.transform, "Health Text", "", new Vector2(250f, 0f), new Vector2(300f, 80f), 32f, TextAlignmentOptions.Left);
        
        // Gold
        goldText = CreateText(topBar.transform, "Gold Text", "", new Vector2(550f, 0f), new Vector2(300f, 80f), 32f, TextAlignmentOptions.Left);

        // Wave
        waveText = CreateText(topBar.transform, "Wave Text", "", new Vector2(0f, 0f), new Vector2(400f, 80f), 36f, TextAlignmentOptions.Center);
        RectTransform waveRect = waveText.GetComponent<RectTransform>();
        waveRect.anchorMin = new Vector2(0.5f, 0.5f);
        waveRect.anchorMax = new Vector2(0.5f, 0.5f);

        // Start Wave Button
        startWaveButton = CreateButton(topBar.transform, "Start Wave Button", "START WAVE", new Vector2(-200f, 0f), new Color(0f, 0.75f, 0.85f, 1f));
        RectTransform btnRect = startWaveButton.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1f, 0.5f);
        btnRect.anchorMax = new Vector2(1f, 0.5f);
        btnRect.pivot = new Vector2(1f, 0.5f);

        startWaveButton.onClick.AddListener(() => {
            // Шукаємо оригінальну кнопку StartWave і клікаємо її через код
            Button[] allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var b in allButtons)
            {
                if (b.name == "StartButton" || b.name.Contains("Start"))
                {
                    b.onClick.Invoke();
                }
            }
        });
    }

    private void CreateBottomDock(Transform parent)
    {
        GameObject dock = CreateUIObject("Bottom Dock", parent);
        Image image = dock.AddComponent<Image>();
        image.color = new Color(0.02f, 0.05f, 0.1f, 0.6f); // Glass

        Sprite roundedSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        if (roundedSprite != null)
        {
            image.sprite = roundedSprite;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 4f;
        }

        Outline outline = dock.AddComponent<Outline>();
        outline.effectColor = new Color(0.15f, 0.25f, 0.45f, 0.8f);
        outline.effectDistance = new Vector2(0f, 2f);

        RectTransform rect = dock.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 40f);
        
        // Зчитуємо вежі з BuildManager
        if (BuildMananger.main != null)
        {
            // Отримуємо вежі через reflection або робимо метод в BuildManager
            // Оскільки towers приватне, ми просто викличемо GetPrefabByType або візьмемо 3 стандартні
        }

        // Hardcode ширину доки: 3 вежі по 160px + відступи
        rect.sizeDelta = new Vector2(600f, 140f);

        float startX = -180f;
        float spacing = 180f;

        // Tower 1
        CreateDockButton(dock.transform, 0, "Basic", 100, new Vector2(startX, 0f));
        // Tower 2
        CreateDockButton(dock.transform, 1, "Sniper", 150, new Vector2(startX + spacing, 0f));
        // Tower 3
        CreateDockButton(dock.transform, 2, "Slowmo", 200, new Vector2(startX + spacing * 2, 0f));
    }

    private void CreateDockButton(Transform parent, int index, string name, int cost, Vector2 pos)
    {
        Button btn = CreateButton(parent, "Tower_" + name, name + "\n<color=#FFD700>" + cost + "</color>", pos, new Color(0.15f, 0.25f, 0.45f, 1f));
        RectTransform rect = btn.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(140f, 100f);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        btn.onClick.AddListener(() => {
            if (BuildMananger.main != null)
            {
                BuildMananger.main.SetSelectedTower(index);
                Debug.Log("Selected Tower: " + name);
            }
        });
    }

    private GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private Button CreateButton(Transform parent, string objectName, string label, Vector2 position, Color color)
    {
        GameObject buttonObject = CreateUIObject(objectName, parent);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.02f, 0.05f, 0.1f, 0.55f);

        Sprite roundedSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        if (roundedSprite != null)
        {
            image.sprite = roundedSprite;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 4f;
        }

        Outline outline = buttonObject.AddComponent<Outline>();
        outline.effectColor = new Color(color.r, color.g, color.b, 0.8f);
        outline.effectDistance = new Vector2(1f, -1f);

        Shadow btnShadow = buttonObject.AddComponent<Shadow>();
        btnShadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
        btnShadow.effectDistance = new Vector2(0f, -5f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        ButtonEnhancer enhancer = buttonObject.AddComponent<ButtonEnhancer>();
        enhancer.themeColor = color;

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(240f, 50f);

        TextMeshProUGUI text = CreateText(buttonObject.transform, "Label", label, Vector2.zero, Vector2.zero, 24f, TextAlignmentOptions.Center);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;

        return button;
    }

    private TextMeshProUGUI CreateText(Transform parent, string name, string textValue, Vector2 pos, Vector2 size, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject go = CreateUIObject(name, parent);
        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.text = textValue;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.fontStyle = FontStyles.Bold;
        text.alignment = alignment;

        go.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.5f);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);

        return text;
    }
}
