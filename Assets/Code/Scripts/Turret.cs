using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Turret : MonoBehaviour
{
    /// <summary>
    /// True while any turret in the scene is waiting for the player to confirm a facing direction.
    /// Used by Plot and PauseMenuController to redirect input and block other interactions.
    /// </summary>
    public static bool IsAwaitingDirection { get; private set; }
    private static Turret activeAwaitingTurret;

    [Header("References")]
    [SerializeField] private Transform turretRotationPoint;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firingPoint;
    [SerializeField] private AudioSource shootSound;
    [SerializeField] private GameObject upgradeUI;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private SpriteRenderer awaitingHighlight;

    [Header("Attributes")]
    [SerializeField] private float targetingRange = 5f;
    [SerializeField] private float bps = 1f;
    [SerializeField] private int baseUpgradeCost = 100;
    [Range(5f, 180f)]
    [Tooltip("Half-angle of the firing cone in degrees. 45 means a 90 degrees total forward arc.")]
    [SerializeField] private float coneHalfAngle = 45f;

    [Header("Targeting Filter")]
    [Tooltip("Can this turret target Ground enemies?")]
    [SerializeField] private bool canTargetGround = true;
    [Tooltip("Can this turret target Air enemies (e.g. FlyEnemy)?")]
    [SerializeField] private bool canTargetAir = false;

    [Header("Placement")]
    [SerializeField] private Color awaitingTint = new Color(1f, 1f, 0.6f, 1f);
    [Tooltip("Default facing direction when the turret spawns, before the player picks one.")]
    [SerializeField] private Vector2 defaultFacing = Vector2.up;

    private float bpsBase;
    private float targetingRangeBase;
    private Transform target;
    private float timeUntilFire;
    private int level = 1;

    private Vector2 facingDirection;
    private bool awaitingDirection;
    private Color awaitingHighlightOriginalColor;

    private int placementCost;
    private Plot parentPlot;
    private bool moneyCharged;
    private TurretPlacementUI placementUI;

    private void Awake()
    {
        bpsBase = bps;
        targetingRangeBase = targetingRange;

        if (awaitingHighlight != null)
            awaitingHighlightOriginalColor = awaitingHighlight.color;

        facingDirection = defaultFacing.sqrMagnitude > 0.0001f
            ? defaultFacing.normalized
            : Vector2.up;
        ApplyFacing(facingDirection);
    }

    private void Start()
    {
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(Upgrade);
    }

    /// <summary>
    /// Called by Plot right after the turret is instantiated.
    /// Puts the turret into placement-preview mode until the player commits or cancels.
    /// </summary>
    public void BeginPlacement(int cost, Plot plot)
    {
        placementCost = cost;
        parentPlot = plot;
        moneyCharged = false;

        awaitingDirection = true;
        activeAwaitingTurret = this;
        IsAwaitingDirection = true;

        if (awaitingHighlight != null)
            awaitingHighlight.color = awaitingTint;

        placementUI = TurretPlacementUI.Spawn(this);
    }

    private void OnDestroy()
    {
        if (activeAwaitingTurret == this)
        {
            activeAwaitingTurret = null;
            IsAwaitingDirection = false;
        }

        if (placementUI != null)
            placementUI.Close();
    }

    private void Update()
    {
        if (awaitingDirection)
        {
            HandleDirectionInput();
            return;
        }

        if (target == null)
        {
            FindTarget();
            return;
        }

        if (!IsTargetStillValid())
        {
            target = null;
            return;
        }

        timeUntilFire += Time.deltaTime;
        if (timeUntilFire >= 1f / bps)
        {
            Shoot();
            timeUntilFire = 0f;
        }
    }

    private void HandleDirectionInput()
    {
        if (PauseMenuController.IsPaused) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            SetPreviewFacing(Vector2.up);
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            SetPreviewFacing(Vector2.right);
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            SetPreviewFacing(Vector2.down);
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            SetPreviewFacing(Vector2.left);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            ConfirmPlacement();
        else if (Input.GetKeyDown(KeyCode.Escape))
            CancelPlacement();
    }

    private void SetPreviewFacing(Vector2 dir)
    {
        facingDirection = dir;
        ApplyFacing(facingDirection);
    }

    /// <summary>
    /// Hooked up to the Confirm button in the placement UI as well as the Enter key.
    /// </summary>
    public void ConfirmPlacement()
    {
        if (!awaitingDirection) return;

        if (!moneyCharged)
        {
            if (LevelMananger.main == null || !LevelMananger.main.SpendCurrency(placementCost))
            {
                CancelPlacement();
                return;
            }
            moneyCharged = true;
        }

        ExitAwaitingMode();
    }

    /// <summary>
    /// Hooked up to the Cancel button in the placement UI as well as the Escape key.
    /// Destroys the turret and clears the originating plot.
    /// </summary>
    public void CancelPlacement()
    {
        if (!awaitingDirection) return;

        if (parentPlot != null)
            parentPlot.ClearTower();

        ExitAwaitingMode();
        Destroy(gameObject);
    }

    private void ExitAwaitingMode()
    {
        awaitingDirection = false;

        if (activeAwaitingTurret == this)
        {
            activeAwaitingTurret = null;
            IsAwaitingDirection = false;
        }

        if (awaitingHighlight != null)
            awaitingHighlight.color = awaitingHighlightOriginalColor;

        if (placementUI != null)
        {
            placementUI.Close();
            placementUI = null;
        }
    }

    private void ApplyFacing(Vector2 dir)
    {
        if (turretRotationPoint == null) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        turretRotationPoint.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void FindTarget()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, targetingRange, Vector2.zero, 0f, enemyMask);
        if (hits.Length == 0) return;

        float cosThreshold = Mathf.Cos(coneHalfAngle * Mathf.Deg2Rad);
        Transform closest = null;
        float closestSqr = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            Vector2 toEnemy = (Vector2)hits[i].transform.position - (Vector2)transform.position;
            float sqr = toEnemy.sqrMagnitude;
            if (sqr > targetingRange * targetingRange) continue;
            if (sqr < 0.0001f) continue;

            Vector2 dirToEnemy = toEnemy / Mathf.Sqrt(sqr);
            if (Vector2.Dot(dirToEnemy, facingDirection) < cosThreshold) continue;

            if (!CanTargetEnemy(hits[i].transform)) continue;

            if (sqr < closestSqr)
            {
                closestSqr = sqr;
                closest = hits[i].transform;
            }
        }

        target = closest;
    }

    private bool CanTargetEnemy(Transform enemyTransform)
    {
        if (!enemyTransform.TryGetComponent<Health>(out var health)) return false;
        return health.Kind == EnemyKind.Ground ? canTargetGround : canTargetAir;
    }

    private bool IsTargetStillValid()
    {
        if (target == null) return false;
        if (!CanTargetEnemy(target)) return false;

        Vector2 toEnemy = (Vector2)target.position - (Vector2)transform.position;
        float sqr = toEnemy.sqrMagnitude;
        if (sqr > targetingRange * targetingRange) return false;
        if (sqr < 0.0001f) return false;

        float cosThreshold = Mathf.Cos(coneHalfAngle * Mathf.Deg2Rad);
        Vector2 dirToEnemy = toEnemy / Mathf.Sqrt(sqr);
        return Vector2.Dot(dirToEnemy, facingDirection) >= cosThreshold;
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firingPoint == null) return;

        GameObject bulletObj = Instantiate(bulletPrefab, firingPoint.position, Quaternion.identity);
        if (bulletObj.TryGetComponent<Bullet>(out var bulletScript))
        {
            bulletScript.Launch(facingDirection);
            bulletScript.SetDamageFilter(canTargetGround, canTargetAir);
        }

        if (shootSound != null) shootSound.Play();
    }

    public void OpenUpgradeUI()
    {
        if (PauseMenuController.IsPaused) return;
        if (awaitingDirection) return;

        if (upgradeUI != null) upgradeUI.SetActive(true);
    }

    public void CloseUpgradeUI()
    {
        if (upgradeUI != null) upgradeUI.SetActive(false);
        if (UIManager.main != null) UIManager.main.SetHoveringState(false);
    }

    public void Upgrade()
    {
        if (PauseMenuController.IsPaused) return;
        if (CalculateCost() > LevelMananger.main.currency) return;

        LevelMananger.main.SpendCurrency(CalculateCost());
        level++;
        bps = CalculateBPS();

        CloseUpgradeUI();
    }

    private int CalculateCost()
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

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector2 dir = Application.isPlaying ? facingDirection : defaultFacing.normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.up;

        Vector3 origin = transform.position;
        Vector3 forward = new Vector3(dir.x, dir.y, 0f);

        Handles.color = new Color(0f, 1f, 0f, 0.15f);
        Handles.DrawSolidArc(origin, Vector3.forward, RotateVector(forward, -coneHalfAngle), coneHalfAngle * 2f, targetingRange);

        Handles.color = Color.green;
        Handles.DrawWireArc(origin, Vector3.forward, RotateVector(forward, -coneHalfAngle), coneHalfAngle * 2f, targetingRange);
        Handles.DrawLine(origin, origin + RotateVector(forward, -coneHalfAngle) * targetingRange);
        Handles.DrawLine(origin, origin + RotateVector(forward, coneHalfAngle) * targetingRange);
        Handles.DrawLine(origin, origin + forward * targetingRange);
    }

    private static Vector3 RotateVector(Vector3 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector3(v.x * cos - v.y * sin, v.x * sin + v.y * cos, 0f);
    }
#endif
}
