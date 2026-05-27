using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Turret : PlacedTower
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
    [SerializeField] private float targetingWidth = 0.75f;
    [SerializeField] private float bps = 1f; 
    [SerializeField] private int baseUpgradeCost = 100;

    private float bpsBase;
    private float targetingRangeBase;

    private Transform target;
    private float timeUntilFire;
    private Vector2 shootDirection = Vector2.up;

    void Start()
    {
        bpsBase = bps;
        targetingRangeBase = targetingRange;

        upgradeButton.onClick.AddListener(Upgrade);
    }

    private void Update () 
    {
        FindTarget();

        if (target == null)
        {
            return;
        }

        timeUntilFire += Time.deltaTime;
        if (timeUntilFire >= 1f / bps)
        {
            Shoot();
            timeUntilFire = 0f;
        }
    }

    private void Shoot ()
    {
        GameObject bulletObj = Instantiate(bulletPrefab, firingPoint.position, Quaternion.identity);
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        bulletScript.Init(shootDirection, LayerMask.NameToLayer("GroundProjectile"));

        if (shootSound != null)
            VolumeSettings.PlaySfx(shootSound);

        Debug.Log("Shoot");
    }

    private void FindTarget () 
    {
        target = null;

        Vector2 center = (Vector2)transform.position + shootDirection * (targetingRange * 0.5f);
        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
        Vector2 size = new Vector2(targetingRange, targetingWidth);
        RaycastHit2D[] hits = Physics2D.BoxCastAll(center, size, angle, Vector2.zero, 0f, enemyMask);

        float closestDistance = float.MaxValue;
        foreach (RaycastHit2D hit in hits)
        {
            Vector2 toEnemy = hit.transform.position - transform.position;
            float forwardDistance = Vector2.Dot(toEnemy, shootDirection);
            if (forwardDistance < 0f || forwardDistance > targetingRange)
                continue;

            if (forwardDistance < closestDistance)
            {
                closestDistance = forwardDistance;
                target = hit.transform;
            }
        }
    }
    public override void OpenUpgradeUI ()
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
    public override void Upgrade ()
    {
        if (PauseMenuController.IsPaused) return;

        if (CalculateCost() > LevelMananger.main.currency) return;

        int upgradeCost = CalculateCost();
        LevelMananger.main.SpendCurrency(upgradeCost);
        totalSpent += upgradeCost;

        level++;

        bps = CalculateBPS();
        //targetingRange = CalculateRange();

        CloseUpgradeUI();
        Debug.Log("New BPS: " + bps);
        //Debug.Log("New Range: " + targetingRange);
        Debug.Log("New Cost: " + CalculateCost());
    }

    public override void Initialize(Vector2 direction, Plot owner, int buildCost)
    {
        ownerPlot = owner;
        totalSpent = buildCost;

        if (direction != Vector2.zero)
        {
            shootDirection = direction.normalized;
            SetPreviewDirection(shootDirection);
        }
    }

    public override void SetPreviewDirection(Vector2 direction)
    {
        if (turretRotationPoint != null && direction != Vector2.zero)
            turretRotationPoint.up = direction.normalized;
    }

    public override bool TryGetTargetingBox(out float range, out float width)
    {
        range = targetingRange;
        width = targetingWidth;
        return true;
    }

    public override TowerType GetTowerType()
    {
        return TowerType.Normal;
    }

    public override int GetUpgradeCost()
    {
        return CalculateCost();
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
#if UNITY_EDITOR
    private void OnDrawGizmosSelected ()
    {
        Handles.color = Color.green;
        Handles.DrawWireDisc(transform.position, transform.forward, targetingRange);

        Handles.color = Color.cyan;
        Vector2 direction = shootDirection == Vector2.zero ? Vector2.up : shootDirection.normalized;
        Vector2 center = (Vector2)transform.position + direction * (targetingRange * 0.5f);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Matrix4x4 previousMatrix = Handles.matrix;
        Handles.matrix = Matrix4x4.TRS(center, Quaternion.Euler(0f, 0f, angle), Vector3.one);
        Handles.DrawWireCube(Vector3.zero, new Vector3(targetingRange, targetingWidth, 0f));
        Handles.matrix = previousMatrix;
    }
#endif
}
