using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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
    private FireRoute[] cachedFireRoutes;
    private Coroutine fireWaveCoroutine;
    private bool fireWaveScheduleRunning;

    
    public int GetTotalKills() { return totalKills; }
    public int GetTotalGold() { return totalGoldEarned; }
    public float GetGameTime() { return Time.time - gameStartTime; }
    public int GetCurrentWave() { return currentWave; }
    public int GetEnemiesLeftToSpawn() { return enemiesLeftToSpawn; }
    public int GetEnemiesAlive() { return enemiesAlive; }
    public int GetSpawnIndex() { return spawnIndex; }
    public float GetTimeSinceLastSpawn() { return timeSinceLastSpawn; }
    public bool IsSpawning() { return isSpawning; }

    private void Awake()
    {
        onEnemyDestroy.AddListener(EnemyDestroyed);
        ResolveReferences();
    }

    private void Start()
    {
        ResolveReferences();
        gameStartTime = Time.time;
        UpdateWaveUI();

        if (startButton == null)
        {
            Debug.LogError("Start button is not assigned on EnemySpawner.");
            return;
        }

        if (startButton.onClick.GetPersistentEventCount() == 0)
        {
            startButton.onClick.AddListener(StartWaveFromButton);
        }

        startButton.interactable = true;
        startButton.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!isSpawning) return;

        if (IsFireScene() && HasFireRouteSetup())
        {
            if (!fireWaveScheduleRunning && enemiesAlive == 0)
            {
                EndWave();
            }

            return;
        }

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

    public void StartWaveFromButton()
    {
        if (!isSpawning)
        {
            StartWave();
        }
    }

    private void StartWave()
    {
        startButton.interactable = false;
        startButton.gameObject.SetActive(false);
        timeSinceLastSpawn = 0f;
        isSpawning = true;

        if (IsFireScene() && TryResolveFireRoutes(out FireRoute[] fireRoutes))
        {
            cachedFireRoutes = fireRoutes;
            StartFireWave();
            enemiesLeftToSpawn = 0;
            spawnIndex = 0;
            return;
        }
        else if (currentWave - 1 < wavePlan.Length)
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

        if (IsFireScene() && cachedFireRoutes != null && cachedFireRoutes.Length > 0)
        {
            FireRoute route = cachedFireRoutes[spawnIndex % cachedFireRoutes.Length];
            GameObject enemyObject = Instantiate(prefabToSpawn, route.SpawnPosition, Quaternion.identity);
            EnemyMovement enemyMovement = enemyObject.GetComponent<EnemyMovement>();
            if (enemyMovement != null)
            {
                enemyMovement.SetHardcodedPath(route.spawnPoint, route.waypoints);
            }
        }
        else
        {
            Instantiate(prefabToSpawn, LevelMananger.main.startPoint.position, Quaternion.identity);
        }

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
        if (fireWaveCoroutine != null)
        {
            StopCoroutine(fireWaveCoroutine);
            fireWaveCoroutine = null;
        }

        fireWaveScheduleRunning = false;
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
        if (waveText == null)
        {
            Debug.LogError("waveText is NULL!");
            return;
        }
        waveText.text = $"Wave {currentWave} / {maxWaves}";
    }

    private float EnemiesPerSecond()
    {
        
        return 1.0f;
    }

    public GameObject GetEnemyPrefabByName(string prefabName)
    {
        string normalizedName = NormalizeName(prefabName);

        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            GameObject enemyPrefab = enemyPrefabs[i];
            if (enemyPrefab != null && NormalizeName(enemyPrefab.name) == normalizedName)
            {
                return enemyPrefab;
            }
        }

        return null;
    }

    public void RestoreState(
        int restoredWave,
        int restoredEnemiesLeftToSpawn,
        int restoredEnemiesAlive,
        int restoredSpawnIndex,
        float restoredTimeSinceLastSpawn,
        bool restoredIsSpawning,
        int restoredTotalKills,
        int restoredTotalGoldEarned,
        float elapsedGameTime)
    {
        currentWave = Mathf.Clamp(restoredWave, 1, maxWaves);
        enemiesLeftToSpawn = Mathf.Max(0, restoredEnemiesLeftToSpawn);
        enemiesAlive = Mathf.Max(0, restoredEnemiesAlive);
        spawnIndex = Mathf.Max(0, restoredSpawnIndex);
        timeSinceLastSpawn = Mathf.Max(0f, restoredTimeSinceLastSpawn);
        isSpawning = restoredIsSpawning;
        totalKills = Mathf.Max(0, restoredTotalKills);
        totalGoldEarned = Mathf.Max(0, restoredTotalGoldEarned);
        gameStartTime = Time.time - Mathf.Max(0f, elapsedGameTime);
        eps = EnemiesPerSecond();

        UpdateWaveUI();

        if (startButton != null)
        {
            bool showStartButton = !isSpawning && currentWave <= maxWaves;
            startButton.interactable = showStartButton;
            startButton.gameObject.SetActive(showStartButton);
        }
    }

    private string NormalizeName(string objectName)
    {
        return objectName.Replace("(Clone)", "").Trim();
    }

    private void ResolveReferences()
    {
        if (waveText == null)
        {
            GameObject waveTextObject = GameObject.Find("WaveText");
            if (waveTextObject != null)
            {
                waveText = waveTextObject.GetComponent<TextMeshProUGUI>();
            }
        }

        if (startButton == null)
        {
            GameObject startButtonObject = GameObject.Find("Start");
            if (startButtonObject != null)
            {
                startButton = startButtonObject.GetComponent<Button>();
            }
        }

        if (winnerUI == null)
        {
            winnerUI = FindFirstObjectByType<WinnerUI>(FindObjectsInactive.Include);
        }
    }

    private bool IsFireScene()
    {
        return SceneManager.GetActiveScene().name == "FireScene";
    }

    private bool HasFireRouteSetup()
    {
        return cachedFireRoutes != null && cachedFireRoutes.Length >= 3;
    }

    private bool TryResolveFireRoutes(out FireRoute[] routes)
    {
        if (cachedFireRoutes != null && cachedFireRoutes.Length >= 3)
        {
            routes = cachedFireRoutes;
            return true;
        }

        List<FireRoute> resolvedRoutes = new List<FireRoute>();
        string[][] routeNameCandidates =
        {
            new[] { "Path", "Path 1", "Path1", "Route", "Route 1", "Route1" },
            new[] { "Path (1)", "Path 2", "Path2", "Route (1)", "Route 2", "Route2" },
            new[] { "Path (2)", "Path 3", "Path3", "Route (2)", "Route 3", "Route3" }
        };

        for (int i = 0; i < routeNameCandidates.Length; i++)
        {
            FireRoute route = ResolveFireRoute(routeNameCandidates[i]);
            if (route != null)
            {
                resolvedRoutes.Add(route);
            }
        }

        routes = resolvedRoutes.ToArray();
        if (routes.Length >= 3)
        {
            cachedFireRoutes = routes;
            return true;
        }

        Debug.LogWarning("Could not resolve 3 hardcoded FireScene routes. Expected names like Path, Path (1), Path (2) with child points.");
        return false;
    }

    private FireRoute ResolveFireRoute(string[] routeObjectNames)
    {
        foreach (string routeObjectName in routeObjectNames)
        {
            GameObject routeObject = GameObject.Find(routeObjectName);
            if (routeObject == null)
            {
                continue;
            }

            List<Transform> waypointList = new List<Transform>();
            Transform spawnPoint = null;

            for (int i = 0; i < routeObject.transform.childCount; i++)
            {
                Transform child = routeObject.transform.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                string childName = child.name.ToLowerInvariant();
                if (childName.Contains("start"))
                {
                    spawnPoint = child;
                    continue;
                }

                waypointList.Add(child);
            }

            if (spawnPoint == null && routeObject.transform.childCount > 0)
            {
                spawnPoint = routeObject.transform.GetChild(0);
            }

            if (waypointList.Count == 0 && routeObject.transform.childCount > 1)
            {
                for (int i = 1; i < routeObject.transform.childCount; i++)
                {
                    waypointList.Add(routeObject.transform.GetChild(i));
                }
            }

            if (waypointList.Count == 0 && spawnPoint != null)
            {
                waypointList.Add(spawnPoint);
            }

            if (spawnPoint != null && waypointList.Count > 0)
            {
                return new FireRoute(spawnPoint, waypointList.ToArray());
            }
        }

        return null;
    }

    private void StartFireWave()
    {
        if (cachedFireRoutes == null || cachedFireRoutes.Length < 3 || enemyPrefabs == null || enemyPrefabs.Length < 2)
        {
            return;
        }

        if (fireWaveCoroutine != null)
        {
            StopCoroutine(fireWaveCoroutine);
        }

        fireWaveCoroutine = StartCoroutine(RunFireWaveSchedule(currentWave));
    }

    private IEnumerator RunFireWaveSchedule(int waveNumber)
    {
        List<FireSpawnEvent> schedule = BuildFireWaveSchedule(waveNumber);
        if (schedule.Count == 0)
        {
            fireWaveScheduleRunning = false;
            yield break;
        }

        fireWaveScheduleRunning = true;
        float previousDelay = 0f;

        for (int i = 0; i < schedule.Count; i++)
        {
            FireSpawnEvent spawnEvent = schedule[i];
            float waitTime = Mathf.Max(0f, spawnEvent.delay - previousDelay);
            if (waitTime > 0f)
            {
                yield return new WaitForSeconds(waitTime);
            }

            SpawnFireEnemy(spawnEvent.routeIndex, spawnEvent.enemyIndex);
            previousDelay = spawnEvent.delay;
        }

        fireWaveScheduleRunning = false;
        fireWaveCoroutine = null;
    }

    private void SpawnFireEnemy(int routeIndex, int enemyIndex)
    {
        if (cachedFireRoutes == null || cachedFireRoutes.Length == 0)
        {
            return;
        }

        int clampedRouteIndex = Mathf.Clamp(routeIndex, 0, cachedFireRoutes.Length - 1);
        int clampedEnemyIndex = Mathf.Clamp(enemyIndex, 0, enemyPrefabs.Length - 1);

        FireRoute route = cachedFireRoutes[clampedRouteIndex];
        if (route == null)
        {
            return;
        }

        GameObject prefabToSpawn = enemyPrefabs[clampedEnemyIndex];
        GameObject enemyObject = Instantiate(prefabToSpawn, route.SpawnPosition, Quaternion.identity);
        EnemyMovement enemyMovement = enemyObject.GetComponent<EnemyMovement>();
        if (enemyMovement != null)
        {
            enemyMovement.SetHardcodedPath(route.spawnPoint, route.waypoints);
        }

        enemiesAlive++;
    }

    private List<FireSpawnEvent> BuildFireWaveSchedule(int waveNumber)
    {
        List<FireSpawnEvent> events = new List<FireSpawnEvent>();

        switch (waveNumber)
        {
            case 1:
                AddRouteOnly(events, 0, 0, 0f, 1.4f, 3);
                break;
            case 2:
                AddRouteOnly(events, 0, 0, 0f, 1.1f, 4);
                break;
            case 3:
                AddRouteOnly(events, 0, 0, 0f, 1.0f, 5);
                break;
            case 4:
                AddRouteOnly(events, 0, 0, 0f, 1.1f, 4);
                AddRouteOnly(events, 1, 1, 3f, 0.45f, 2);
                break;
            case 5:
                AddPairedRoutes(events, 0f, 1.3f, 3, 0, 0, 1, 0);
                break;
            case 6:
                AddPairedRoutes(events, 0f, 1.1f, 4, 0, 0, 1, 0);
                break;
            case 7:
                AddPairedRoutes(events, 0f, 1.15f, 3, 0, 0, 1, 0);
                AddRouteOnly(events, 2, 0, 3f, 0.45f, 2);
                break;
            case 8:
                AddThreeRoutes(events, 0f, 1.15f, 3, 0, 0, 1, 0, 2, 0);
                break;
            case 9:
                AddThreeRoutes(events, 0f, 1.05f, 4, 0, 0, 1, 0, 2, 0);
                break;
            case 10:
                AddThreeRoutes(events, 0f, 0.95f, 5, 0, 0, 1, 0, 2, 0);
                break;
        }

        events.Sort((a, b) => a.delay.CompareTo(b.delay));
        return events;
    }

    private void AddRouteOnly(List<FireSpawnEvent> events, int routeIndex, int enemyIndex, float startDelay, float spacing, int count)
    {
        for (int i = 0; i < count; i++)
        {
            events.Add(new FireSpawnEvent(startDelay + spacing * i, routeIndex, enemyIndex));
        }
    }

    private void AddPairedRoutes(
        List<FireSpawnEvent> events,
        float startDelay,
        float spacing,
        int groups,
        int firstRouteIndex,
        int firstEnemyIndex,
        int secondRouteIndex,
        int secondEnemyIndex)
    {
        for (int i = 0; i < groups; i++)
        {
            float delay = startDelay + spacing * i;
            events.Add(new FireSpawnEvent(delay, firstRouteIndex, firstEnemyIndex));
            events.Add(new FireSpawnEvent(delay, secondRouteIndex, secondEnemyIndex));
        }
    }

    private void AddThreeRoutes(
        List<FireSpawnEvent> events,
        float startDelay,
        float spacing,
        int groups,
        int firstRouteIndex,
        int firstEnemyIndex,
        int secondRouteIndex,
        int secondEnemyIndex,
        int thirdRouteIndex,
        int thirdEnemyIndex)
    {
        for (int i = 0; i < groups; i++)
        {
            float delay = startDelay + spacing * i;
            events.Add(new FireSpawnEvent(delay, firstRouteIndex, firstEnemyIndex));
            events.Add(new FireSpawnEvent(delay, secondRouteIndex, secondEnemyIndex));
            events.Add(new FireSpawnEvent(delay, thirdRouteIndex, thirdEnemyIndex));
        }
    }

    private sealed class FireRoute
    {
        public readonly Transform spawnPoint;
        public readonly Transform[] waypoints;

        public Vector3 SpawnPosition => spawnPoint != null ? spawnPoint.position : waypoints[0].position;

        public FireRoute(Transform spawnPoint, Transform[] waypoints)
        {
            this.spawnPoint = spawnPoint;
            this.waypoints = waypoints;
        }
    }

    private readonly struct FireSpawnEvent
    {
        public readonly float delay;
        public readonly int routeIndex;
        public readonly int enemyIndex;

        public FireSpawnEvent(float delay, int routeIndex, int enemyIndex)
        {
            this.delay = delay;
            this.routeIndex = routeIndex;
            this.enemyIndex = enemyIndex;
        }
    }
}
