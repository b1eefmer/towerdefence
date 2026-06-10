using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TurretSlowmo : PlacedTower
{
    [Header("References")]
    [SerializeField] private LayerMask enemyMask;

    [Header("Attributes")]
    [SerializeField] private float targetingRange = 5f;
    [SerializeField] private float aps = 4f; // attacks per second
    [SerializeField] private float freezeTime = 1f;
    [SerializeField] private int baseUpgradeCost = 100;

    [Header("Visuals")]
    [SerializeField] private Color slowTintColor = new(0.4f, 0.8f, 1f, 1f);
    [SerializeField] private Color rangeRingColor = new(0.4f, 0.8f, 1f, 0.35f);
    [SerializeField] private float rangeRingWidth = 0.05f;
    [SerializeField] private int rangeRingSegments = 64;

    private float baseAps;
    private float timeUntilFire;
    private LineRenderer rangeRing;

    private void Awake()
    {
        baseAps = aps;
    }

    private void Start()
    {
        timeUntilFire = 1f / aps; // fire on the first frame if enemies are already in range
        DrawRangeRing();
    }

    private void Update()
    {
        timeUntilFire += Time.deltaTime;
        if (timeUntilFire >= 1f / aps)
        {
            FreezeEnemies();
            timeUntilFire = 0f;
        }
    }

    private void DrawRangeRing()
    {
        GameObject ringObject = new GameObject("RangeRing");
        ringObject.transform.SetParent(transform, false);

        rangeRing = ringObject.AddComponent<LineRenderer>();
        rangeRing.useWorldSpace = false;
        rangeRing.loop = true;
        rangeRing.positionCount = rangeRingSegments;
        rangeRing.startWidth = rangeRingWidth;
        rangeRing.endWidth = rangeRingWidth;
        rangeRing.startColor = rangeRingColor;
        rangeRing.endColor = rangeRingColor;
        rangeRing.material = new Material(Shader.Find("Sprites/Default"));
        rangeRing.sortingOrder = 5;

        for (int i = 0; i < rangeRingSegments; i++)
        {
            float angle = (float)i / rangeRingSegments * 2f * Mathf.PI;
            rangeRing.SetPosition(i, new Vector3(
                Mathf.Cos(angle) * targetingRange,
                Mathf.Sin(angle) * targetingRange,
                0f));
        }
    }

    private void FreezeEnemies()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, targetingRange, (Vector2)transform.position, 0f, enemyMask);

        if (hits.Length == 0)
            return;

        SpawnFreezePulse();

        for (int i = 0; i < hits.Length; i++)
        {
            EnemyMovement em = hits[i].transform.GetComponent<EnemyMovement>();
            if (em == null) continue;

            em.UpdateSpeed(0.5f);
            em.SetSlowTint(slowTintColor);
            StartCoroutine(ResetEnemySpeed(em));
        }
    }

    private void SpawnFreezePulse()
    {
        GameObject pulseObject = new GameObject("FreezePulse");
        pulseObject.transform.position = transform.position;
        FreezePulse pulse = pulseObject.AddComponent<FreezePulse>();
        pulse.Init(targetingRange, slowTintColor);
    }

    private IEnumerator ResetEnemySpeed(EnemyMovement em)
    {
        yield return new WaitForSeconds(freezeTime);

        if (em != null)
        {
            em.ResetSpeed();
            em.ClearSlowTint();
        }
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
