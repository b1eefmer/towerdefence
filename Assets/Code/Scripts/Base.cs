using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BaseHealth : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    private bool isGameOver = false;

    [Header("Legacy Hearts (auto-hidden)")]
    [SerializeField] private Image[] hearts;

    [Header("Effects")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource damageSound;

    [Header("Audio")]
    [SerializeField] private AudioSource baseHitSound;
    [SerializeField] private AudioSource gameOverSound;
    [SerializeField] private MusicManager musicManager;

    [Header("Game Over")]
    [SerializeField] private GameOverUI gameOverUI;

    private GameObject hpCanvas;
    private TextMeshProUGUI hpLabel;

    private void Start()
    {
        currentHealth = maxHealth;
        HideHearts();
        BuildHpDisplay();
        UpdateHpDisplay();
        isGameOver = false;
    }

    public void TakeDamage(int amount)
    {
        if (isGameOver) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHpDisplay();

        if (animator != null) animator.SetTrigger("Damage");
        if (damageSound != null) VolumeSettings.PlaySfx(damageSound);
        if (baseHitSound != null) VolumeSettings.PlaySfx(baseHitSound);

        if (currentHealth <= 0)
            GameOver();
    }

    public void SetLives(int lives)
    {
        currentHealth = Mathf.Clamp(lives, 0, maxHealth);
        UpdateHpDisplay();
    }

    public int GetLives()
    {
        return currentHealth;
    }

    private void HideHearts()
    {
        if (hearts == null) return;
        foreach (Image h in hearts)
            if (h != null) h.gameObject.SetActive(false);
    }

    private void BuildHpDisplay()
    {
        hpCanvas = new GameObject("HP UI");

        Canvas canvas = hpCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;

        CanvasScaler scaler = hpCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panel = new GameObject("Panel", typeof(RectTransform));
        panel.transform.SetParent(hpCanvas.transform, false);

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.082f, 0.075f, 0.165f, 0.92f);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(16f, -16f);
        panelRect.sizeDelta = new Vector2(170f, 44f);

        GameObject textObj = new GameObject("Label", typeof(RectTransform));
        textObj.transform.SetParent(panel.transform, false);

        hpLabel = textObj.AddComponent<TextMeshProUGUI>();
        hpLabel.alignment = TextAlignmentOptions.MidlineLeft;
        hpLabel.fontSize = 24f;

        RectTransform textRect = hpLabel.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12f, 0f);
        textRect.offsetMax = new Vector2(-6f, 0f);
    }

    private void UpdateHpDisplay()
    {
        if (hpLabel == null) return;
        float frac = (float)currentHealth / maxHealth;
        hpLabel.color = frac > 0.35f ? Color.white : new Color(0.8f, 0.12f, 0.05f, 1f);
        hpLabel.text = $"HP  {currentHealth} / {maxHealth}";
    }

    private void OnDestroy()
    {
        if (hpCanvas != null)
            Destroy(hpCanvas);
    }

    private void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        int totalKills = 0;
        int totalGold = 0;
        float gameTime = 0f;
        if (spawner != null)
        {
            totalKills = spawner.GetTotalKills();
            totalGold = spawner.GetTotalGold();
            gameTime = spawner.GetGameTime();
        }

        if (musicManager != null)
            musicManager.StopMusic();

        if (gameOverSound != null)
            VolumeSettings.PlaySfx(gameOverSound);

        if (gameOverUI != null)
            gameOverUI.ShowGameOver(totalKills, totalGold, gameTime);
        else
            Time.timeScale = 0f;
    }
}
