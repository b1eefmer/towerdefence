using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    private const string KeyEnabled = "tutorial_enabled";
    private const string KeyStep = "tut_step";
    private const int StepComplete = 99;

    private static readonly string[] Instructions = {
        "Select a platform to place your first tower",
        "Aim with W / A / S / D,  then press ENTER to place",
        "Press  START WAVE  to begin",
        "Collect coins dropped by defeated enemies — they disappear over time!"
    };

    [SerializeField] private int tutorialPlotId = 125;

    private int currentStep;
    private TutorialHint activeHint;
    private EnemySpawner spawner;
    private Plot[] plots;
    private Plot tutorialPlot;

    private GameObject tutorialCanvas;
    private TextMeshProUGUI instructionLabel;
    private RectTransform panelRect;
    private Image panelBg;
    private const float NormalPanelY = 172f;
    private const float PanelHeight = 52f;

    private void Start()
    {
        if (PlayerPrefs.GetInt(KeyEnabled, 1) == 0) { enabled = false; return; }
        if (PlayerPrefs.GetInt(KeyStep, 0) >= StepComplete) { enabled = false; return; }

        spawner = FindFirstObjectByType<EnemySpawner>();
        plots = FindObjectsByType<Plot>(FindObjectsSortMode.None);
        tutorialPlot = FindPlotById(tutorialPlotId);

        BuildTextPanel();
        currentStep = InferStartStep();
        ShowStep(currentStep);
    }

    private int InferStartStep()
    {
        foreach (Plot p in plots)
            if (p.IsBuilt) return 2;
        return 0;
    }

    private void Update()
    {
        switch (currentStep)
        {
            case 0:
                foreach (Plot p in plots)
                {
                    if (p.IsPlacing) { AdvanceTo(1); return; }
                }
                RefreshStep0Hint();
                break;

            case 1:
                foreach (Plot p in plots)
                {
                    if (p.IsBuilt) { AdvanceTo(2); return; }
                }
                break;

            case 2:
                if (spawner != null && spawner.WaveRunning)
                    AdvanceTo(3);
                else
                    RefreshStep2Hint();
                break;

            case 3:
                // waiting for coroutine to complete
                break;
        }
    }

    private void ShowStep(int step)
    {
        DismissHint();

        if (instructionLabel != null && step < Instructions.Length)
            instructionLabel.text = Instructions[step];

        // During placement (step 1) PlacementUIController already shows the UI — hide our panel to avoid overlap
        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(step != 1);

        switch (step)
        {
            case 0:
                if (tutorialPlot != null)
                    activeHint = TutorialHint.Create(tutorialPlot.transform.position, 0.7f);
                break;

            case 1:
                // No ring: PlacementUIController handles direction UI
                break;

            case 2:
                activeHint = TutorialHint.Create(GetStartButtonWorldPos(), 1.2f);
                break;

            case 3:
                if (panelRect != null)
                    panelRect.anchoredPosition = new Vector2(0f, -PanelHeight);
                StartCoroutine(DelayedComplete(10f));
                break;
        }
    }

    private void AdvanceTo(int step)
    {
        currentStep = step;
        PlayerPrefs.SetInt(KeyStep, step);
        PlayerPrefs.Save();
        ShowStep(step);
    }

    private void CompleteTutorial()
    {
        DismissHint();
        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(false);
        PlayerPrefs.SetInt(KeyEnabled, 0);
        PlayerPrefs.SetInt(KeyStep, StepComplete);
        PlayerPrefs.Save();
        enabled = false;
    }

    private IEnumerator DelayedComplete(float seconds)
    {
        const float slideDuration = 0.3f;
        const float targetY = 20f;

        if (panelBg != null)
        {
            panelBg.color = new Color(0.95f, 0.78f, 0.05f, 0.95f);
            instructionLabel.color = new Color(0.08f, 0.05f, 0f, 1f);
        }

        float t = 0f;
        while (t < slideDuration)
        {
            if (panelRect != null)
                panelRect.anchoredPosition = new Vector2(0f, Mathf.Lerp(-PanelHeight, targetY, t / slideDuration));
            t += Time.deltaTime;
            yield return null;
        }
        if (panelRect != null)
            panelRect.anchoredPosition = new Vector2(0f, targetY);

        yield return new WaitForSeconds(seconds);

        t = 0f;
        while (t < slideDuration)
        {
            if (panelRect != null)
                panelRect.anchoredPosition = new Vector2(0f, Mathf.Lerp(targetY, -PanelHeight, t / slideDuration));
            t += Time.deltaTime;
            yield return null;
        }

        CompleteTutorial();
    }

    private void DismissHint()
    {
        if (activeHint != null)
        {
            activeHint.Dismiss();
            activeHint = null;
        }
    }

    private void RefreshStep0Hint()
    {
        // Position is fixed to tutorialPlot — nothing to refresh
    }

    private void RefreshStep2Hint()
    {
        if (activeHint == null || spawner == null) return;
        Button btn = spawner.StartButton;
        if (btn == null || !btn.gameObject.activeInHierarchy) return;
        activeHint.SetWorldPosition(GetStartButtonWorldPos());
    }

    private Plot FindPlotById(int id)
    {
        foreach (Plot p in plots)
            if (p.PlotId == id) return p;
        Debug.LogWarning($"TutorialManager: plot with id {id} not found.");
        return null;
    }

    private Vector3 GetStartButtonWorldPos()
    {
        if (spawner == null) return Vector3.zero;
        Button btn = spawner.StartButton;
        if (btn == null) return Vector3.zero;

        Camera cam = Camera.main;
        if (cam == null) return Vector3.zero;

        Vector3 screenPos = btn.transform.position;
        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, cam.nearClipPlane + 5f));
        worldPos.z = 0f;
        return worldPos;
    }

    private void BuildTextPanel()
    {
        tutorialCanvas = new GameObject("TutorialUI");

        Canvas canvas = tutorialCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 85;

        CanvasScaler scaler = tutorialCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        // No GraphicRaycaster — this panel must not block clicks on the game world

        GameObject panel = new GameObject("Panel", typeof(RectTransform));
        panel.transform.SetParent(tutorialCanvas.transform, false);

        panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0.07f, 0.08f, 0.11f, 0.92f);

        panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, NormalPanelY);
        panelRect.sizeDelta = new Vector2(570f, PanelHeight);

        GameObject textObj = new GameObject("Instruction", typeof(RectTransform));
        textObj.transform.SetParent(panel.transform, false);

        instructionLabel = textObj.AddComponent<TextMeshProUGUI>();
        instructionLabel.alignment = TextAlignmentOptions.Center;
        instructionLabel.color = Color.white;
        instructionLabel.fontSize = 26f; // matches PlacementUI instruction text

        RectTransform textRect = instructionLabel.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12f, 4f);
        textRect.offsetMax = new Vector2(-12f, -4f);
    }

    private void OnDestroy()
    {
        DismissHint();
        if (tutorialCanvas != null)
            Destroy(tutorialCanvas);
    }
}
