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
    [SerializeField] private float rotationSpeed = 200f;
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
    public override void OpenUpgradeUI ()
    {
        if (PauseMenuController.IsPaused) return;

        upgradeUI.SetActive(true);
    }
    public void CloseUpgradeUI()
    {
        upgradeUI.SetActive(false);
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

            if (turretRotationPoint != null)
                turretRotationPoint.up = shootDirection;
        }
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
    }
#endif
}
