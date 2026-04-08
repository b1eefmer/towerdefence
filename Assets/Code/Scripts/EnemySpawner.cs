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
    [SerializeField] private int maxWaves = 10;
    [SerializeField] private int currencyIncome = 30;      

    
    private int[][] wavePlan = new int[][]
    {
        new int[] { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }, // Wave 1
        new int[] { 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1 }, // Wave 2
        new int[] { 2,2 },                            // Wave 3
        new int[] { 0,0,0,0,0,0,0,0, 1,1,1,1, 2,2 }, // Wave 4
        new int[] { 0,0,0,0,0,0,0,0, 1,1,1,1,1, 2,2,2 }, // Wave 5
        new int[] { 0,0,0,0,0,0,0,0,0, 1,1,1,1,1,1, 2,2,2,2,2 }, // Wave 6
        new int[] { 2,2,2,2,2,2, 0,0,0,0,0,0,0,0, 1,1,1,1,1,1,1 }, // Wave 7
        new int[] { 2,2,2,2,2,2,2,2, 0,0,0,0,0,0,0,0, 1,1,1,1,1,1,1 }, // Wave 8
        new int[] { 2,2,2,2,2,2,2,2,2,2, 0,0,0,0,0,0,0,0, 1,1,1,1,1,1,1,1 }, // Wave 9
        new int[] { 2,2,2,2,2,2,2,2,2,2,2,2, 0,0,0,0,0,0,0,0,0, 1,1,1,1,1,1,1,1 } // Wave 10
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
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
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
        
        return 1.0f;
    }
}