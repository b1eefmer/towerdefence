using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Turret : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("References")]
    [SerializeField] private Transform turretRotationPoint;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firingPoint;
    [SerializeField] private AudioSource shootSound;
    [SerializeField] private GameObject upgradeUI;
    [SerializeField] private Button upgradeButton;

    [Header("Attributes")]
    [SerializeField] private float targetingRange = 5f;
    [SerializeField] private float rotationSpeed = 200f;
    [SerializeField] private float bps = 1f; 
    [SerializeField] private int baseUpgradeCost = 100;
    [SerializeField] private Vector2 upgradeUiOffset = new Vector2(170f, 20f);
    [SerializeField] private Vector2 upgradeUiSize = new Vector2(240f, 150f);
    [SerializeField] private Vector2 upgradeButtonSize = new Vector2(100f, 20f);
    [SerializeField] private Vector3 upgradeUiWorldOffset = new Vector3(2.2f, 0.45f, 0f);
    [SerializeField] private float upgradeUiAutoCloseDelay = 3f;
    [SerializeField] private Color rangeIndicatorColor = new Color(0.25f, 1f, 0.35f, 0.9f);
    [SerializeField] private float rangeIndicatorWidth = 0.06f;

    private float bpsBase;
    private float targetingRangeBase;

    private Transform target;
    private float timeUntilFire;

    private int level = 1;
    private RectTransform upgradeUiRect;
    private RectTransform upgradeButtonRect;
    private TextMeshProUGUI upgradeText;
    private TextMeshProUGUI upgradeInfoText;
    private Canvas upgradeCanvas;
    private Image upgradeBackground;
    private Image upgradeButtonImage;
    private SpriteRenderer[] turretSpriteRenderers;
    private UpgradeUIHandler upgradeUiHandler;
    private float upgradeUiIdleTime;
    private LineRenderer rangeIndicator;
    private const int RangeIndicatorSegments = 72;

    private void Awake()
    {
        CacheBaseStats();
        CacheUpgradeUiReferences();
        EnsureRangeIndicator();
    }

    void Start()
    {
        CacheBaseStats();
        CacheUpgradeUiReferences();

        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(Upgrade);
        }
    }

    private void Update () 
    {
        UpdateUpgradeUiLifetime();
        HandleUpgradeButtonFallback();

        if (target == null) 
        {
            FindTarget();
            return;
        }
        RotateTowardsTarget();
        if (!CheckTargetIsInRange())
        {
            target = null;
        }
        else
        {
            timeUntilFire += Time.deltaTime;
            if (timeUntilFire >= 1f / bps)
            {
                Shoot();
                timeUntilFire = 0f;
            }
        }
    }
    private void Shoot ()
    {
        GameObject bulletObj = Instantiate(bulletPrefab, firingPoint.position, Quaternion.identity);
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        bulletScript.SetTarget(target);

        shootSound.Play();
        Debug.Log("Shoot");
    }
    private void FindTarget () 
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, targetingRange, (Vector2)transform.position, 0f, enemyMask);

        if (hits.Length > 0)
        {
            target = hits[0].transform;
        }
    }
    private bool CheckTargetIsInRange ()
    {
        return Vector2.Distance(target.position, transform.position) <= targetingRange;
    }
    private void RotateTowardsTarget () 
    {
        float angle = Mathf.Atan2(target.position.y - transform.position.y, target.position.x - transform.position.x) * Mathf.Rad2Deg - 90f;

        Quaternion targetRotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
        turretRotationPoint.rotation = Quaternion.RotateTowards(turretRotationPoint.rotation, targetRotation, rotationSpeed*Time.deltaTime);
    }
    public void OpenUpgradeUI ()
    {
        Debug.Log($"[UpgradeUI] Open requested for {name}. upgradeUI assigned: {upgradeUI != null}, upgradeButton assigned: {upgradeButton != null}");

        ConfigureUpgradeUi();

        if (upgradeUI == null)
        {
            Debug.LogWarning($"[UpgradeUI] Missing upgradeUI on {name}");
            return;
        }

        upgradeUiIdleTime = 0f;
        upgradeUI.SetActive(true);
        ShowRangeIndicator();
        Debug.Log($"[UpgradeUI] {name} active={upgradeUI.activeSelf} hierarchyActive={upgradeUI.activeInHierarchy}");

        if (upgradeUiRect != null)
        {
            Debug.Log($"[UpgradeUI] {name} rect anchored={upgradeUiRect.anchoredPosition} size={upgradeUiRect.sizeDelta} world={upgradeUiRect.position}");
        }

        if (upgradeCanvas != null)
        {
            Debug.Log($"[UpgradeUI] {name} canvas overrideSorting={upgradeCanvas.overrideSorting} sortingLayerID={upgradeCanvas.sortingLayerID} sortingOrder={upgradeCanvas.sortingOrder} renderMode={upgradeCanvas.renderMode}");
        }

        if (upgradeText != null)
        {
            Debug.Log($"[UpgradeUI] {name} text='{upgradeText.text}'");
        }
        else
        {
            Debug.LogWarning($"[UpgradeUI] {name} missing upgrade text component");
        }
    }
    public void CloseUpgradeUI()
    {
        if (upgradeUI != null)
        {
            upgradeUI.SetActive(false);
            upgradeUiIdleTime = 0f;
            Debug.Log($"[UpgradeUI] Closed for {name}");
        }

        HideRangeIndicator();

        if (UIManager.main != null)
        {
            UIManager.main.SetHoveringState(false);
        }
    }
    public void Upgrade ()
    {
        if (CalculateCost() > LevelMananger.main.currency) return;

        LevelMananger.main.SpendCurrency(CalculateCost());

        level++;

        bps = CalculateBPS();
        //targetingRange = CalculateRange();

        CloseUpgradeUI();
        Debug.Log("New BPS: " + bps);
        //Debug.Log("New Range: " + targetingRange);
        Debug.Log("New Cost: " + CalculateCost());
    }

    private void HandleUpgradeButtonFallback()
    {
        if (upgradeUI == null || upgradeButton == null || !upgradeUI.activeInHierarchy || !upgradeButton.isActiveAndEnabled)
        {
            return;
        }

        if (UIManager.main != null && UIManager.main.IsHoveringUI())
        {
            return;
        }

        if (!TryGetPointerDownPosition(out Vector2 screenPosition))
        {
            return;
        }

        RectTransform buttonRect = upgradeButton.transform as RectTransform;
        if (buttonRect == null)
        {
            return;
        }

        Canvas canvas = upgradeButton.GetComponentInParent<Canvas>();
        Camera eventCamera = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            eventCamera = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
        }

        if (!RectTransformUtility.RectangleContainsScreenPoint(buttonRect, screenPosition, eventCamera))
        {
            return;
        }

        Upgrade();
    }

    public int GetLevel()
    {
        return level;
    }

    public void SetLevel(int newLevel)
    {
        CacheBaseStats();
        level = Mathf.Max(1, newLevel);
        bps = CalculateBPS();
    }

    private void CacheBaseStats()
    {
        if (bpsBase <= 0f)
        {
            bpsBase = bps;
        }

        if (targetingRangeBase <= 0f)
        {
            targetingRangeBase = targetingRange;
        }
    }

    private void CacheUpgradeUiReferences()
    {
        if (upgradeUI == null)
        {
            return;
        }

        upgradeUiRect = upgradeUI.GetComponent<RectTransform>();
        upgradeCanvas = upgradeUI.GetComponent<Canvas>();
        upgradeBackground = upgradeUI.GetComponent<Image>();
        upgradeUiHandler = upgradeUI.GetComponent<UpgradeUIHandler>();
        turretSpriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        if (upgradeBackground == null)
        {
            upgradeBackground = upgradeUI.AddComponent<Image>();
        }

        if (upgradeButton != null)
        {
            upgradeButtonRect = upgradeButton.transform as RectTransform;
            upgradeText = upgradeButton.GetComponentInChildren<TextMeshProUGUI>(true);
            upgradeButtonImage = upgradeButton.GetComponent<Image>();
        }

        EnsureUpgradeInfoText();
    }

    private void ConfigureUpgradeUi()
    {
        CacheUpgradeUiReferences();

        Debug.Log($"[UpgradeUI] Configure for {name}. rect={upgradeUiRect != null}, buttonRect={upgradeButtonRect != null}, text={upgradeText != null}, canvas={upgradeCanvas != null}");

        if (upgradeUiRect != null)
        {
            if (upgradeCanvas != null && upgradeCanvas.renderMode == RenderMode.WorldSpace)
            {
                upgradeUiRect.localPosition = upgradeUiWorldOffset;
                upgradeUiRect.localRotation = Quaternion.identity;
                upgradeUiRect.localScale = new Vector3(0.015f, 0.015f, 0.015f);
            }
            else
            {
                upgradeUiRect.anchorMin = new Vector2(0f, 0.5f);
                upgradeUiRect.anchorMax = new Vector2(0f, 0.5f);
                upgradeUiRect.pivot = new Vector2(0f, 0.5f);
                upgradeUiRect.anchoredPosition = upgradeUiOffset;
            }

            upgradeUiRect.sizeDelta = upgradeUiSize;
        }

        if (upgradeCanvas != null)
        {
            upgradeCanvas.overrideSorting = true;
            upgradeCanvas.worldCamera = Camera.main;

            int sortingLayerId = 0;
            int sortingOrder = 50;

            if (turretSpriteRenderers != null && turretSpriteRenderers.Length > 0)
            {
                SpriteRenderer frontSprite = turretSpriteRenderers
                    .OrderByDescending(renderer => renderer.sortingOrder)
                    .FirstOrDefault();

                if (frontSprite != null)
                {
                    sortingLayerId = frontSprite.sortingLayerID;
                    sortingOrder = frontSprite.sortingOrder + 20;
                }
            }

            upgradeCanvas.sortingLayerID = sortingLayerId;
            upgradeCanvas.sortingOrder = sortingOrder;
        }

        if (upgradeBackground != null)
        {
            upgradeBackground.color = new Color(0.08f, 0.1f, 0.12f, 0.92f);
            upgradeBackground.raycastTarget = true;
        }

        if (upgradeButtonRect != null)
        {
            upgradeButtonRect.anchorMin = new Vector2(0.5f, 0.5f);
            upgradeButtonRect.anchorMax = new Vector2(0.5f, 0.5f);
            upgradeButtonRect.pivot = new Vector2(0.5f, 0.5f);
            upgradeButtonRect.anchoredPosition = new Vector2(0f, -34f);
            upgradeButtonRect.sizeDelta = upgradeButtonSize;
        }

        if (upgradeButtonImage != null)
        {
            upgradeButtonImage.color = new Color(0.2f, 0.72f, 0.3f, 1f);
        }

        if (upgradeInfoText != null)
        {
            upgradeInfoText.alignment = TextAlignmentOptions.Center;
            upgradeInfoText.enableAutoSizing = false;
            upgradeInfoText.fontSize = 18f;
            upgradeInfoText.text = $"Current Level: {level}\nUpgrade Cost: {CalculateCost()}";
        }

        if (upgradeText != null)
        {
            upgradeText.alignment = TextAlignmentOptions.Center;
            upgradeText.enableAutoSizing = false;
            upgradeText.fontSize = 22f;
            upgradeText.text = "Upgrade";
        }
    }
    private int CalculateCost ()
    {
        return Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 0.8f));
    }
    private float CalculateBPS()
    {
        return bpsBase * Mathf.Pow(level, 0.6f);
    }
    private float CalculateRange()
    {
        return targetingRangeBase * Mathf.Pow(level, 0.4f);
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

    private void UpdateUpgradeUiLifetime()
    {
        if (upgradeUI == null || !upgradeUI.activeInHierarchy)
        {
            upgradeUiIdleTime = 0f;
            HideRangeIndicator();
            return;
        }

        UpdateRangeIndicatorShape();

        bool pointerInsideUpgradeUi = upgradeUiHandler != null && upgradeUiHandler.mouse_over;
        if (pointerInsideUpgradeUi)
        {
            upgradeUiIdleTime = 0f;
            return;
        }

        upgradeUiIdleTime += Time.deltaTime;
        if (upgradeUiIdleTime >= upgradeUiAutoCloseDelay)
        {
            CloseUpgradeUI();
        }
    }

    private void EnsureRangeIndicator()
    {
        if (rangeIndicator != null)
        {
            UpdateRangeIndicatorShape();
            return;
        }

        GameObject indicatorObject = new GameObject("RangeIndicator");
        indicatorObject.transform.SetParent(transform, false);
        indicatorObject.transform.localPosition = Vector3.zero;
        indicatorObject.transform.localRotation = Quaternion.identity;

        rangeIndicator = indicatorObject.AddComponent<LineRenderer>();
        rangeIndicator.useWorldSpace = true;
        rangeIndicator.loop = true;
        rangeIndicator.positionCount = RangeIndicatorSegments;
        rangeIndicator.startWidth = rangeIndicatorWidth;
        rangeIndicator.endWidth = rangeIndicatorWidth;
        rangeIndicator.startColor = rangeIndicatorColor;
        rangeIndicator.endColor = rangeIndicatorColor;
        rangeIndicator.material = new Material(Shader.Find("Sprites/Default"));
        rangeIndicator.sortingLayerName = "Default";
        rangeIndicator.sortingOrder = 100;
        rangeIndicator.enabled = false;

        UpdateRangeIndicatorShape();
    }

    private void UpdateRangeIndicatorShape()
    {
        if (rangeIndicator == null)
        {
            return;
        }

        for (int i = 0; i < RangeIndicatorSegments; i++)
        {
            float angle = i * Mathf.PI * 2f / RangeIndicatorSegments;
            Vector3 point = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * targetingRange;
            rangeIndicator.SetPosition(i, point);
        }
    }

    private void ShowRangeIndicator()
    {
        EnsureRangeIndicator();
        UpdateRangeIndicatorShape();

        if (rangeIndicator != null)
        {
            rangeIndicator.enabled = true;
        }
    }

    private void HideRangeIndicator()
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.enabled = false;
        }
    }

    private void EnsureUpgradeInfoText()
    {
        if (upgradeUI == null)
        {
            return;
        }

        Transform existingInfo = upgradeUI.transform.Find("UpgradeInfoText");
        if (existingInfo != null)
        {
            upgradeInfoText = existingInfo.GetComponent<TextMeshProUGUI>();
            return;
        }

        GameObject infoObject = new GameObject("UpgradeInfoText");
        infoObject.transform.SetParent(upgradeUI.transform, false);
        infoObject.layer = upgradeUI.layer;

        RectTransform infoRect = infoObject.AddComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0.5f, 1f);
        infoRect.anchorMax = new Vector2(0.5f, 1f);
        infoRect.pivot = new Vector2(0.5f, 1f);
        infoRect.anchoredPosition = new Vector2(0f, -18f);
        infoRect.sizeDelta = new Vector2(200f, 56f);

        upgradeInfoText = infoObject.AddComponent<TextMeshProUGUI>();
        upgradeInfoText.raycastTarget = false;

        if (upgradeText != null)
        {
            upgradeInfoText.font = upgradeText.font;
            upgradeInfoText.fontSharedMaterial = upgradeText.fontSharedMaterial;
            upgradeInfoText.color = Color.white;
        }
    }

    private void OnDrawGizmosSelected ()
    {
        Handles.color = Color.green;
        Handles.DrawWireDisc(transform.position, transform.forward, targetingRange);
    }
}
