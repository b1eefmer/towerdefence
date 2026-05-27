using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TurretSlowmo : PlacedTower
{
    [Header("References")]
    [SerializeField] private Transform turretRotationPoint;
    [SerializeField] private LayerMask enemyMask;

    [Header("Attributes")]
    [SerializeField] private float targetingRange = 5f;
    [SerializeField] private float aps = 4f; // attacks per second
    [SerializeField] private float freezeTime = 1f;
    [SerializeField] private int baseUpgradeCost = 100;

    private float baseAps;
    private float timeUntilFire;

    private void Awake()
    {
        baseAps = aps;
    }
    private void Update()
    {
        timeUntilFire += Time.deltaTime;
        if (timeUntilFire >= 1f / aps)
        {
            Debug.Log("Freeze");
            FreezeEnemies();
            timeUntilFire = 0f;
        }
    }
    private void FreezeEnemies()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, targetingRange, (Vector2)transform.position, 0f, enemyMask);

        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit2D hit = hits[i];
                
                EnemyMovement em = hit.transform.GetComponent<EnemyMovement>();
                em.UpdateSpeed(0.5f);

                StartCoroutine(ResetEnemySpeed(em));
            }
        }
    }
    private IEnumerator ResetEnemySpeed(EnemyMovement em)
    {
        yield return new WaitForSeconds(freezeTime);

        em.ResetSpeed();
    }

    public override void Initialize(Vector2 direction, Plot owner, int buildCost)
    {
        ownerPlot = owner;
        totalSpent = buildCost;
    }

    public override TowerType GetTowerType()
    {
        return TowerType.Slow;
    }

    public override int GetUpgradeCost()
    {
        return Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 0.8f));
    }

    public override void Upgrade()
    {
        if (PauseMenuController.IsPaused || !CanUpgrade()) return;

        int upgradeCost = GetUpgradeCost();
        LevelMananger.main.SpendCurrency(upgradeCost);
        totalSpent += upgradeCost;
        level++;
        aps = baseAps * Mathf.Pow(level, 0.4f);
    }

    public override void RestoreState(int savedLevel, int savedTotalSpent)
    {
        base.RestoreState(savedLevel, savedTotalSpent);
        aps = baseAps * Mathf.Pow(level, 0.4f);
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Handles.color = Color.green;
        Handles.DrawWireDisc(transform.position, transform.forward, targetingRange);
    }
#endif
}
