using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EnemySpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("References")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Attributes")]
    [SerializeField] private int baseEnemies= 8;
    [SerializeField] private float enemiesPerSecond = 0.5f;
    [SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private float difficultyScalingFactor = 0.75f;
    [SerializeField] private float enemiesPerSecondCap = 15f;
    [SerializeField] private int currencyIncome = 30;

    [Header("Events")]
    public static UnityEvent onEnemyDestroy = new UnityEvent();

    private int spawnIndex = 0;
    private int[][] wavePlan = new int[][]
{
    new int[] { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
    // Wave 1: basic only | 15 enemies | Gold: 52 (11*2 + 30)

    new int[] { 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1 },
    // Wave 2: fast only | 15 enemies | Gold: 50 (10*2 + 30)

    new int[] { 2,2 },
    // Wave 3: tank only | 3 enemies | Gold: 36 (1*6 + 30)

    new int[] { 0,0,0,0,0,0,0,0, 1,1,1,1, 2,2 },
    // Wave 4: mixed intro | 14 enemies | Gold: 66
    // 8 basic, 4 fast, 2 tank
    // Tanks are at the end, not at the start

    new int[] { 0,0,0,0,0,0,0,0, 1,1,1,1,1, 2,2,2 },
    // Wave 5: mixed | 16 enemies | Gold: 74
    // 8 basic, 5 fast, 3 tank
    // Still no tank-first pressure

    new int[] { 0,0,0,0,0,0,0,0,0, 1,1,1,1,1,1, 2,2,2,2,2 },
    // Wave 6: mixed | 20 enemies | Gold: 90
    // 9 basic, 6 fast, 5 tank
    // Last wave before tank-first pattern

    new int[] { 2,2,2,2,2,2, 0,0,0,0,0,0,0,0, 1,1,1,1,1,1,1 },
    // Wave 7: tank-first begins | 21 enemies | Gold: 96
    // 6 tank, 8 basic, 7 fast

    new int[] { 2,2,2,2,2,2,2,2, 0,0,0,0,0,0,0,0, 1,1,1,1,1,1,1 },
    // Wave 8: more tanks | 23 enemies | Gold: 108
    // 8 tank, 8 basic, 7 fast

    new int[] { 2,2,2,2,2,2,2,2,2,2, 0,0,0,0,0,0,0,0, 1,1,1,1,1,1,1,1 },
    // Wave 9: heavy tank pressure | 26 enemies | Gold: 122
    // 10 tank, 8 basic, 8 fast

    new int[] { 2,2,2,2,2,2,2,2,2,2,2,2, 0,0,0,0,0,0,0,0,0, 1,1,1,1,1,1,1,1 },
    // Wave 10: strongest wave | 29 enemies | Gold: 136
    // 12 tank, 9 basic, 8 fast
};
    private int currentWave = 1;
    private int maxWaves = 10;
    private float timeSinceLastSpawn;
    private int enemiesAlive;
    private int enemiesLeftToSpawn;
    private float eps; // enemies Per second
    private bool isSpawning = false;
    private void Awake()
    {
        onEnemyDestroy.AddListener(EnemyDestroyes);
    }
    private void Start()
    {
        StartCoroutine(StartWave()); 
    }
    private void Update()
    {
        if (!isSpawning) return;

        timeSinceLastSpawn += Time.deltaTime;

        if (timeSinceLastSpawn > (1f /  eps) && (enemiesLeftToSpawn>0))
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
    private void SpawnEnemy()
    {
        if (currentWave - 1 >= wavePlan.Length) return;

        int enemyIndex = wavePlan[currentWave - 1][spawnIndex];
        GameObject prefabToSpawn = enemyPrefabs[enemyIndex];

        Instantiate(prefabToSpawn, LevelMananger.main.startPoint.position, Quaternion.identity);

        spawnIndex++;
        //int index = Random.Range(0, enemyPrefabs.Length);
        //GameObject prefabToSpawn = enemyPrefabs[index];
        //Instantiate(prefabToSpawn, LevelMananger.main.startPoint.position, Quaternion.identity);

    }
    private void EnemyDestroyes()
    {
        enemiesAlive--;
    }
    private IEnumerator StartWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        isSpawning = true;
        spawnIndex = 0;
        enemiesLeftToSpawn = wavePlan[currentWave - 1].Length;
        eps = EnemiesPerSecond();
    }

    private void EndWave()
    {
        isSpawning = false;
        timeSinceLastSpawn = 0f;
        currentWave++;
        LevelMananger.main.IncreaseCurrency(currencyIncome);
        if (currentWave > maxWaves)
        {
            Debug.Log("You win");
            return;
        }
        StartCoroutine(StartWave());
    }

    private int EnemiesPerWave()
    {
        return Mathf.RoundToInt(baseEnemies * Mathf.Pow(currentWave, difficultyScalingFactor));
    }
    private float EnemiesPerSecond()
    {
        return Mathf.Clamp(enemiesPerSecond * Mathf.Pow(currentWave, difficultyScalingFactor), 0, enemiesPerSecondCap);
    }
}
