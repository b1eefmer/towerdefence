using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AntiAirTurret : PlacedTower
{
    [Header("References")]
    [SerializeField] private Transform turretRotationPoint;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firingPoint;
    [SerializeField] private AudioSource shootSound;
    [SerializeField] private GameObject upgradeUI;
    [SerializeField] private Button upgradeButton;

    [Header("Attributes")]
    [SerializeField] private float targetingRange = 6f;
    [SerializeField] private float sectorHalfAngle = 45f;
    [SerializeField] private float bps = 1.5f;
    [SerializeField] private int baseUpgradeCost = 100;

    private float bpsBase;
    private Vector2 baseDirection = Vector2.up;
    private Transform target;
    private float timeUntilFire;

    private void Awake()
    {
        bpsBase = bps;

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(Upgrade);
    }

    private void Update()
    {
        FindTarget();

        if (target == null)
            return;

        RotateTowardsTarget();

        timeUntilFire += Time.deltaTime;
        if (timeUntilFire >= 1f / bps)
        {
            Shoot();
            timeUntilFire = 0f;
        }
    }

    private void FindTarget()
    {
        target = null;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, targetingRange, enemyMask);
        float closestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            Vector2 toEnemy = hit.transform.position - transform.position;
            if (toEnemy == Vector2.zero)
                continue;

            float angle = Vector2.Angle(baseDirection, toEnemy);
            if (angle > sectorHalfAngle)
                continue;

            float distance = toEnemy.sqrMagnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                target = hit.transform;
            }
        }
    }

    private void RotateTowardsTarget()
    {
        if (turretRotationPoint == null || target == null)
            return;

        Vector2 direction = (target.position - turretRotationPoint.position).normalized;
        turretRotationPoint.up = direction;
    }

    private void Shoot()
    {
        if (target == null)
            return;

        Vector2 direction = (target.position - firingPoint.position).normalized;
        GameObject bulletObj = Instantiate(bulletPrefab, firingPoint.position, Quaternion.identity);
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        bulletScript.Init(direction, LayerMask.NameToLayer("AirProjectile"));

        if (shootSound != null)
            VolumeSettings.PlaySfx(shootSound);
    }

    public override void Initialize(Vector2 direction, Plot owner, int buildCost)
    {
        ownerPlot = owner;
        totalSpent = buildCost;

        if (direction != Vector2.zero)
        {
            baseDirection = direction.normalized;
            SetPreviewDirection(baseDirection);
        }
    }

    public override void SetPreviewDirection(Vector2 direction)
    {
        if (direction == Vector2.zero)
            return;

        baseDirection = direction.normalized;

        if (turretRotationPoint != null)
            turretRotationPoint.up = baseDirection;
    }

    public override TowerType GetTowerType()
    {
        return TowerType.AntiAir;
    }

    public override int GetUpgradeCost()
    {
        return Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 0.8f));
    }

    public override void Upgrade()
    {
        if (PauseMenuController.IsPaused || !CanUpgrade())
            return;

        int upgradeCost = GetUpgradeCost();
        LevelMananger.main.SpendCurrency(upgradeCost);
        totalSpent += upgradeCost;
        level++;
        bps = bpsBase * Mathf.Pow(level, 0.6f);

        CloseUpgradeUI();
    }

    public override void OpenUpgradeUI()
    {
        base.OpenUpgradeUI();
    }

    public void CloseUpgradeUI()
    {
        if (upgradeUI != null)
            upgradeUI.SetActive(false);

        if (UIManager.main != null)
            UIManager.main.SetHoveringState(false);
    }

    public override void RestoreState(int savedLevel, int savedTotalSpent)
    {
        base.RestoreState(savedLevel, savedTotalSpent);
        bps = bpsBase * Mathf.Pow(level, 0.6f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector2 direction = baseDirection == Vector2.zero ? Vector2.up : baseDirection.normalized;
        Vector3 origin = transform.position;
        Vector3 left = Quaternion.Euler(0f, 0f, -sectorHalfAngle) * direction;
        Vector3 right = Quaternion.Euler(0f, 0f, sectorHalfAngle) * direction;

        Handles.color = Color.cyan;
        Handles.DrawWireDisc(origin, transform.forward, targetingRange);
        Handles.DrawLine(origin, origin + left * targetingRange);
        Handles.DrawLine(origin, origin + right * targetingRange);
        Handles.DrawWireArc(origin, transform.forward, left, sectorHalfAngle * 2f, targetingRange);
    }
#endif
}
