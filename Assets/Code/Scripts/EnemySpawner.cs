using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private Button startButton;
    [SerializeField] private WinnerUI winnerUI;

    [Header("Attributes")]
    [SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private int maxWaves = 5;
    [SerializeField] private int currencyIncome = 30;

    [Header("Spawn Rate")]
    [Tooltip("Base spawn rate on wave 1.")]
    [SerializeField] private float enemiesPerSecond = 0.5f;
    [Tooltip("Exponent used to grow EPS per wave. Higher = faster ramp-up.")]
    [SerializeField] private float difficultyScalingFactor = 0.75f;
    [Tooltip("Hard ceiling for spawn rate at high waves.")]
    [SerializeField] private float enemiesPerSecondCap = 15f;

    
    // Enemy indices: 0 = Basic, 1 = Speed, 2 = Tank, 3 = Fly (AirTurret only)
    private int[][] wavePlan = new int[][]
    {
        // Wave 1 - Знакомство: только базовые враги, чтобы освоить постройку и направление турелей.
        new int[] { 0, 0, 0, 0, 0, 0, 0, 0 },

        // Wave 2 - Скорость: появляются быстрые враги вперемешку с базовыми.
        new int[] { 0, 0, 1, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0, 0 },

        // Wave 3 - Воздушная угроза: первые FlyEnemy. Без AirTurret пройти невозможно.
        new int[] { 0, 0, 3, 0, 1, 3, 0, 3, 3, 0, 1, 3, 0, 0, 1, 3 },

        // Wave 4 - Танки наступают: тяжёлые цели + продолжающееся давление с воздуха.
        new int[] { 0, 2, 0, 1, 1, 2, 0, 0, 2, 0, 3, 3, 0, 2, 0, 1, 1, 2 },

        // Wave 5 - Финальный шторм: все типы врагов, плотные группы танков и летающих.
        new int[] { 2, 2, 0, 0, 1, 3, 2, 0, 1, 3, 2, 0, 0, 1, 1, 3, 3, 2, 2, 0, 0, 1, 3, 2, 0, 1, 3, 2, 2, 3 }
    };

    [Header("Events")]
    public static UnityEvent onEnemyDestroy = new UnityEvent();

    private int currentWave = 1;
    private float timeSinceLastSpawn;
    private int enemiesAlive;
    private int enemiesLeftToSpawn;
    private float eps;
    private bool isSpawning = false;
    private int spawnIndex = 0;


    private int totalKills = 0;
    private int totalGoldEarned = 0;
    private float gameStartTime;

    
    public int GetTotalKills() { return totalKills; }
    public int GetTotalGold() { return totalGoldEarned; }
    public float GetGameTime() { return Time.time - gameStartTime; }

    private void Awake()
    {
        onEnemyDestroy.AddListener(EnemyDestroyed);
    }

    private void OnDestroy()
    {
        onEnemyDestroy.RemoveListener(EnemyDestroyed);
    }

    private void Start()
    {
        gameStartTime = Time.time;
        UpdateWaveUI();
        startButton.onClick.AddListener(StartWave);
        startButton.interactable = true;
    }

    private void Update()
    {
        if (!isSpawning) return;

        timeSinceLastSpawn += Time.deltaTime;
        if (timeSinceLastSpawn > (1f / eps) && enemiesLeftToSpawn > 0)
        {
            SpawnEnemy();
            enemiesLeftToSpawn--;
            enemiesAlive++;
            timeSinceLastSpawn = 0f;
        }

        if (enemiesAlive == 0 && enemiesLeftToSpawn == 0)
        {
            EndWave();
        }
    }

    private void StartWave()
    {
        startButton.interactable = false;
        startButton.gameObject.SetActive(false);
        timeSinceLastSpawn = 0f;
        isSpawning = true;

        if (currentWave - 1 < wavePlan.Length)
        {
            enemiesLeftToSpawn = wavePlan[currentWave - 1].Length;
            spawnIndex = 0;
        }
        else
        {
            enemiesLeftToSpawn = 0;
        }
        eps = EnemiesPerSecond();
    }

    private void SpawnEnemy()
    {
        if (currentWave - 1 >= wavePlan.Length) return;
        int enemyIndex = wavePlan[currentWave - 1][spawnIndex];

        if (enemyPrefabs == null || enemyIndex < 0 || enemyIndex >= enemyPrefabs.Length || enemyPrefabs[enemyIndex] == null)
        {
            Debug.LogWarning($"EnemySpawner: wave {currentWave} requests enemy index {enemyIndex}, but enemyPrefabs has only {(enemyPrefabs == null ? 0 : enemyPrefabs.Length)} slots. Skipping spawn.");
            spawnIndex++;
            return;
        }

        GameObject prefabToSpawn = enemyPrefabs[enemyIndex];
        Instantiate(prefabToSpawn, LevelMananger.main.startPoint.position, Quaternion.identity);
        spawnIndex++;
    }

    private void EnemyDestroyed()
    {
        enemiesAlive--;
    }

    public static void AddStats(int goldEarned)
    {
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.totalKills++;
            spawner.totalGoldEarned += goldEarned;
        }
    }

    private void EndWave()
    {
        isSpawning = false;
        LevelMananger.main.IncreaseCurrency(currencyIncome);
        totalGoldEarned += currencyIncome;

        if (currentWave >= maxWaves)
        {
            Winner();
            return;
        }

        currentWave++;
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
        waveText.text = $"Wave {currentWave} / {maxWaves}";
    }

    private float EnemiesPerSecond()
    {
        return Mathf.Clamp(
            enemiesPerSecond * Mathf.Pow(currentWave, difficultyScalingFactor),
            0f,
            enemiesPerSecondCap);
    }
}
