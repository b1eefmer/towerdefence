using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveSystem
{
    private const string SaveFileName = "savegame.json";

    private static bool initialized;
    private static GameSaveData pendingLoadData;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeOnLoad()
    {
        EnsureInitialized();
    }

    public static bool SaveGame()
    {
        EnsureInitialized();

        LevelMananger levelManager = LevelMananger.main;
        EnemySpawner enemySpawner = UnityEngine.Object.FindFirstObjectByType<EnemySpawner>();
        BaseHealth baseHealth = UnityEngine.Object.FindFirstObjectByType<BaseHealth>();
        BuildMananger buildManager = BuildMananger.main;

        if (levelManager == null || enemySpawner == null || baseHealth == null || buildManager == null)
        {
            Debug.LogWarning("Save failed: required gameplay managers were not found.");
            return false;
        }

        Plot[] plots = UnityEngine.Object.FindObjectsByType<Plot>(FindObjectsSortMode.None);
        EnemyMovement[] enemies = UnityEngine.Object.FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None);

        List<PlotSaveData> savedPlots = new List<PlotSaveData>();
        foreach (Plot plot in plots)
        {
            if (plot == null || !plot.HasTower())
            {
                continue;
            }

            GameObject towerObject = plot.GetTowerObject();
            int towerIndex = buildManager.GetTowerIndexByInstance(towerObject);
            if (towerIndex < 0)
            {
                continue;
            }

            Turret turret = towerObject.GetComponent<Turret>();
            savedPlots.Add(new PlotSaveData
            {
                x = plot.transform.position.x,
                y = plot.transform.position.y,
                towerIndex = towerIndex,
                towerLevel = turret != null ? turret.GetLevel() : 1
            });
        }

        List<EnemySaveData> savedEnemies = new List<EnemySaveData>();
        foreach (EnemyMovement enemyMovement in enemies)
        {
            if (enemyMovement == null)
            {
                continue;
            }

            Health health = enemyMovement.GetComponent<Health>();
            if (health == null)
            {
                continue;
            }

            savedEnemies.Add(new EnemySaveData
            {
                enemyPrefabName = NormalizeName(enemyMovement.gameObject.name),
                x = enemyMovement.transform.position.x,
                y = enemyMovement.transform.position.y,
                currentHitPoints = health.GetCurrentHitPoints(),
                pathIndex = enemyMovement.GetPathIndex(),
                moveSpeed = enemyMovement.GetMoveSpeed(),
                baseSpeed = enemyMovement.GetBaseSpeed()
            });
        }

        GameSaveData saveData = new GameSaveData
        {
            sceneName = GetGameplaySceneName(),
            currency = levelManager.currency,
            currentWave = enemySpawner.GetCurrentWave(),
            enemiesLeftToSpawn = enemySpawner.GetEnemiesLeftToSpawn(),
            enemiesAlive = enemySpawner.GetEnemiesAlive(),
            spawnIndex = enemySpawner.GetSpawnIndex(),
            timeSinceLastSpawn = enemySpawner.GetTimeSinceLastSpawn(),
            isSpawning = enemySpawner.IsSpawning(),
            totalKills = enemySpawner.GetTotalKills(),
            totalGoldEarned = enemySpawner.GetTotalGold(),
            elapsedGameTime = enemySpawner.GetGameTime(),
            baseLives = baseHealth.GetCurrentLives(),
            plots = savedPlots.ToArray(),
            enemies = savedEnemies.ToArray()
        };

        File.WriteAllText(GetSavePath(), JsonUtility.ToJson(saveData, true));
        Debug.Log($"Game saved to {GetSavePath()}");
        return true;
    }

    public static bool LoadGame()
    {
        EnsureInitialized();

        if (!HasSave())
        {
            Debug.LogWarning("Load failed: save file not found.");
            return false;
        }

        string json = File.ReadAllText(GetSavePath());
        GameSaveData loadedData = JsonUtility.FromJson<GameSaveData>(json);
        if (loadedData == null || string.IsNullOrWhiteSpace(loadedData.sceneName))
        {
            Debug.LogWarning("Load failed: save file is invalid.");
            return false;
        }

        pendingLoadData = loadedData;
        Time.timeScale = 1f;

        Scene pauseScene = SceneManager.GetSceneByName("PauseScene");
        if (pauseScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(pauseScene);
        }

        SceneManager.LoadScene(loadedData.sceneName);
        return true;
    }

    public static bool HasSave()
    {
        return File.Exists(GetSavePath());
    }

    private static void EnsureInitialized()
    {
        if (initialized)
        {
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        initialized = true;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pendingLoadData == null || scene.name != pendingLoadData.sceneName)
        {
            return;
        }

        ApplyLoadedData(pendingLoadData);
        pendingLoadData = null;
    }

    private static void ApplyLoadedData(GameSaveData saveData)
    {
        LevelMananger levelManager = LevelMananger.main;
        EnemySpawner enemySpawner = UnityEngine.Object.FindFirstObjectByType<EnemySpawner>();
        BaseHealth baseHealth = UnityEngine.Object.FindFirstObjectByType<BaseHealth>();
        BuildMananger buildManager = BuildMananger.main;

        if (levelManager == null || enemySpawner == null || baseHealth == null || buildManager == null)
        {
            Debug.LogWarning("Load failed: required gameplay managers were not found after scene load.");
            return;
        }

        levelManager.SetCurrency(saveData.currency);
        baseHealth.RestoreLives(saveData.baseLives);

        Plot[] plots = UnityEngine.Object.FindObjectsByType<Plot>(FindObjectsSortMode.None);
        Dictionary<string, Plot> plotsByPosition = new Dictionary<string, Plot>();
        foreach (Plot plot in plots)
        {
            if (plot == null)
            {
                continue;
            }

            plot.ClearTower();
            plotsByPosition[BuildPositionKey(plot.transform.position.x, plot.transform.position.y)] = plot;
        }

        if (saveData.plots != null)
        {
            foreach (PlotSaveData plotData in saveData.plots)
            {
                string positionKey = BuildPositionKey(plotData.x, plotData.y);
                if (!plotsByPosition.TryGetValue(positionKey, out Plot plot))
                {
                    continue;
                }

                GameObject towerPrefab = buildManager.GetTowerPrefab(plotData.towerIndex);
                plot.RestoreTower(towerPrefab, plotData.towerLevel);
            }
        }

        if (saveData.enemies != null)
        {
            foreach (EnemySaveData enemyData in saveData.enemies)
            {
                GameObject enemyPrefab = enemySpawner.GetEnemyPrefabByName(enemyData.enemyPrefabName);
                if (enemyPrefab == null)
                {
                    continue;
                }

                GameObject enemyObject = UnityEngine.Object.Instantiate(
                    enemyPrefab,
                    new Vector3(enemyData.x, enemyData.y, 0f),
                    Quaternion.identity);

                EnemyMovement enemyMovement = enemyObject.GetComponent<EnemyMovement>();
                Health health = enemyObject.GetComponent<Health>();

                if (enemyMovement != null)
                {
                    enemyMovement.RestoreState(enemyData.pathIndex, enemyData.moveSpeed, enemyData.baseSpeed);
                }

                if (health != null)
                {
                    health.SetCurrentHitPoints(enemyData.currentHitPoints);
                }
            }
        }

        enemySpawner.RestoreState(
            saveData.currentWave,
            saveData.enemiesLeftToSpawn,
            saveData.enemiesAlive,
            saveData.spawnIndex,
            saveData.timeSinceLastSpawn,
            saveData.isSpawning,
            saveData.totalKills,
            saveData.totalGoldEarned,
            saveData.elapsedGameTime);

        Debug.Log($"Game loaded from {GetSavePath()}");
    }

    private static string GetGameplaySceneName()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded && scene.name != "PauseScene")
            {
                return scene.name;
            }
        }

        return SceneManager.GetActiveScene().name;
    }

    private static string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, SaveFileName);
    }

    private static string BuildPositionKey(float x, float y)
    {
        int roundedX = Mathf.RoundToInt(x * 1000f);
        int roundedY = Mathf.RoundToInt(y * 1000f);
        return $"{roundedX}:{roundedY}";
    }

    private static string NormalizeName(string objectName)
    {
        return objectName.Replace("(Clone)", "").Trim();
    }

    [Serializable]
    private class GameSaveData
    {
        public string sceneName;
        public int currency;
        public int currentWave;
        public int enemiesLeftToSpawn;
        public int enemiesAlive;
        public int spawnIndex;
        public float timeSinceLastSpawn;
        public bool isSpawning;
        public int totalKills;
        public int totalGoldEarned;
        public float elapsedGameTime;
        public int baseLives;
        public PlotSaveData[] plots;
        public EnemySaveData[] enemies;
    }

    [Serializable]
    private class PlotSaveData
    {
        public float x;
        public float y;
        public int towerIndex;
        public int towerLevel;
    }

    [Serializable]
    private class EnemySaveData
    {
        public string enemyPrefabName;
        public float x;
        public float y;
        public int currentHitPoints;
        public int pathIndex;
        public float moveSpeed;
        public float baseSpeed;
    }
}
