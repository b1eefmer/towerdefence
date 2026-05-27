using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

[Serializable]
public class SpawnEntry
{
    public GameObject enemyPrefab;
    public int pathIndex;
}

[Serializable]
public class WaveDefinition
{
    public SpawnEntry[] enemies;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WaveDefinition[] waves;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private Button startButton;
    [SerializeField] private WinnerUI winnerUI;

    [Header("Attributes")]
    [SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private float epsStart = 0.5f;
    [SerializeField] private float epsEnd = 3f;
    [SerializeField] private float collectingTime = 4f;

    [Header("Events")]
    public static UnityEvent onEnemyRemoved = new UnityEvent();

    private enum WaveState
    {
        Idle,
        Spawning,
        CollectingDrops,
        Done
    }

    private int currentWave = 1;
    private float timeSinceLastSpawn;
    private int enemiesAlive;
    private int enemiesLeftToSpawn;
    private float eps;
    private int spawnIndex = 0;
    private WaveState waveState = WaveState.Idle;


    private int totalKills = 0;
    private int totalGoldEarned = 0;
    private float gameStartTime;

    
    public int GetTotalKills() { return totalKills; }
    public int GetTotalGold() { return totalGoldEarned; }
    public float GetGameTime() { return Time.time - gameStartTime; }

    private void Awake()
    {
        onEnemyRemoved.AddListener(EnemyRemoved);
    }

    private void OnDestroy()
    {
        onEnemyRemoved.RemoveListener(EnemyRemoved);
    }

    private void Start()
    {
        gameStartTime = Time.time;
        UpdateWaveUI();
        startButton.onClick.AddListener(StartWave);
        startButton.interactable = waves != null && waves.Length > 0;

        if (!startButton.interactable)
            Debug.LogError("No waves configured for EnemySpawner.");
    }

    private void Update()
    {
        if (waveState != WaveState.Spawning) return;

        timeSinceLastSpawn += Time.deltaTime;
        if (timeSinceLastSpawn > (1f / eps) && enemiesLeftToSpawn > 0)
        {
            if (SpawnEnemy())
                enemiesAlive++;

            enemiesLeftToSpawn--;
            timeSinceLastSpawn = 0f;
        }

        if (enemiesAlive == 0 && enemiesLeftToSpawn == 0)
        {
            waveState = WaveState.CollectingDrops;
            StartCoroutine(CollectDropsThenEndWave());
        }
    }

    private void StartWave()
    {
        if (waveState != WaveState.Idle)
            return;

        startButton.interactable = false;
        startButton.gameObject.SetActive(false);
        timeSinceLastSpawn = 0f;
        waveState = WaveState.Spawning;

        if (currentWave - 1 < waves.Length)
        {
            enemiesLeftToSpawn = waves[currentWave - 1].enemies.Length;
            spawnIndex = 0;
        }
        else
        {
            enemiesLeftToSpawn = 0;
        }
        eps = EnemiesPerSecond();
    }

    private bool SpawnEnemy()
    {
        if (currentWave - 1 >= waves.Length) return false;

        SpawnEntry entry = waves[currentWave - 1].enemies[spawnIndex];
        Transform[] path = LevelMananger.main.GetPath(entry.pathIndex);
        if (entry.enemyPrefab == null || path == null || path.Length == 0)
        {
            Debug.LogError($"Invalid spawn entry in wave {currentWave} at index {spawnIndex}.");
            spawnIndex++;
            return false;
        }

        GameObject enemy = Instantiate(entry.enemyPrefab, path[0].position, Quaternion.identity);
        EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
        if (movement != null)
            movement.SetPath(entry.pathIndex);

        spawnIndex++;
        return true;
    }

    private void EnemyRemoved()
    {
        enemiesAlive--;
    }

    public static void RegisterKill()
    {
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
            spawner.totalKills++;
    }

    public static void RegisterCollectedGold(int amount)
    {
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
            spawner.totalGoldEarned += amount;
    }

    private IEnumerator CollectDropsThenEndWave()
    {
        yield return new WaitForSeconds(collectingTime);

        CurrencyDrop[] remainingDrops = FindObjectsByType<CurrencyDrop>(FindObjectsSortMode.None);
        foreach (CurrencyDrop drop in remainingDrops)
            Destroy(drop.gameObject);

        waveState = WaveState.Done;
        EndWave();
    }

    private void EndWave()
    {
        if (currentWave >= waves.Length)
        {
            Winner();
            return;
        }

        currentWave++;
        waveState = WaveState.Idle;
        UpdateWaveUI();
        StartCoroutine(ShowStartButtonWithDelay());
    }

    private IEnumerator ShowStartButtonWithDelay()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        startButton.interactable = true;
        startButton.gameObject.SetActive(true);
    }

    private void Winner()
    {
        float gameTime = Time.time - gameStartTime;
        if (winnerUI != null)
            winnerUI.ShowWinner(totalKills, totalGoldEarned, gameTime);
        else
            Debug.LogError("WinnerUI not assigned!");

        enabled = false;
        startButton.gameObject.SetActive(false);
    }

    private void UpdateWaveUI()
    {
        int totalWaves = waves == null ? 0 : waves.Length;
        waveText.text = $"Wave {currentWave} / {totalWaves}";
    }

    private float EnemiesPerSecond()
    {
        float progress = Mathf.InverseLerp(1f, waves.Length, currentWave);
        return Mathf.Lerp(epsStart, epsEnd, progress);
    }
}
