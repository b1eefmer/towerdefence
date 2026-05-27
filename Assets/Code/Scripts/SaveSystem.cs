using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class TowerSaveData
{
    public int plotId;
    public TowerType type;
    public int level;
    public int totalSpent;
    public Vector2 direction;
}

[Serializable]
public class SaveData
{
    public string sceneName;
    public int nextWaveIndex;
    public int currency;
    public int baseLives;
    public int levelEntryCurrency;
    public List<TowerSaveData> towers = new List<TowerSaveData>();
}

public static class SaveSystem
{
    private const string FileName = "save.json";

    private static SaveData pendingLoadData;

    public static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    public static bool HasSave()
    {
        return File.Exists(SavePath);
    }

    public static bool SaveCurrentGame()
    {
        EnemySpawner spawner = UnityEngine.Object.FindFirstObjectByType<EnemySpawner>();
        if (spawner == null || !spawner.CanSaveNow)
            return false;

        WriteSave(CreateSaveData(spawner));
        return true;
    }

    public static void AutoSaveAfterWave(EnemySpawner spawner)
    {
        if (spawner == null || !spawner.CanSaveNow || spawner.CurrentWave > spawner.TotalWaves)
            return;

        WriteSave(CreateSaveData(spawner));
    }

    public static bool LoadSavedGame()
    {
        if (!TryReadSave(out SaveData data))
            return false;

        GameSession.EnsureInstance();
        pendingLoadData = data;
        SceneManager.LoadScene(data.sceneName);
        return true;
    }

    public static void ApplyPendingLoadIfAny()
    {
        if (pendingLoadData == null)
            return;

        if (SceneManager.GetActiveScene().name != pendingLoadData.sceneName)
            return;

        SaveData data = pendingLoadData;
        pendingLoadData = null;

        CurrencyDrop[] drops = UnityEngine.Object.FindObjectsByType<CurrencyDrop>(FindObjectsSortMode.None);
        foreach (CurrencyDrop drop in drops)
            UnityEngine.Object.Destroy(drop.gameObject);

        if (LevelMananger.main != null)
            LevelMananger.main.currency = data.currency;

        GameSession.EnsureInstance();
        GameSession.Instance.levelEntrySnapshot = data.levelEntryCurrency;

        BaseHealth baseHealth = UnityEngine.Object.FindFirstObjectByType<BaseHealth>();
        if (baseHealth != null)
            baseHealth.SetLives(data.baseLives);

        EnemySpawner spawner = UnityEngine.Object.FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
            spawner.RestoreBetweenWaves(data.nextWaveIndex);

        Plot[] plots = UnityEngine.Object.FindObjectsByType<Plot>(FindObjectsSortMode.None);
        foreach (TowerSaveData towerData in data.towers)
        {
            Plot plot = FindPlotById(plots, towerData.plotId);
            if (plot != null)
                plot.RestoreTower(towerData.type, towerData.level, towerData.totalSpent, towerData.direction);
            else
                Debug.LogWarning($"No plot found for saved plot id {towerData.plotId}.");
        }
    }

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    private static SaveData CreateSaveData(EnemySpawner spawner)
    {
        CurrencyDrop[] drops = UnityEngine.Object.FindObjectsByType<CurrencyDrop>(FindObjectsSortMode.None);
        foreach (CurrencyDrop drop in drops)
            UnityEngine.Object.Destroy(drop.gameObject);

        SaveData data = new SaveData
        {
            sceneName = SceneManager.GetActiveScene().name,
            nextWaveIndex = spawner.CurrentWave,
            currency = LevelMananger.main != null ? LevelMananger.main.currency : 0,
            baseLives = GetBaseLives(),
            levelEntryCurrency = GameSession.Instance != null ? GameSession.Instance.levelEntrySnapshot : 1000,
            towers = new List<TowerSaveData>()
        };

        Plot[] plots = UnityEngine.Object.FindObjectsByType<Plot>(FindObjectsSortMode.None);
        foreach (Plot plot in plots)
        {
            if (plot.TryCreateTowerSaveData(out TowerSaveData towerData))
                data.towers.Add(towerData);
        }

        return data;
    }

    private static int GetBaseLives()
    {
        BaseHealth baseHealth = UnityEngine.Object.FindFirstObjectByType<BaseHealth>();
        return baseHealth != null ? baseHealth.GetLives() : 0;
    }

    private static void WriteSave(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"Game saved to {SavePath}");
    }

    private static bool TryReadSave(out SaveData data)
    {
        data = null;
        if (!File.Exists(SavePath))
            return false;

        string json = File.ReadAllText(SavePath);
        data = JsonUtility.FromJson<SaveData>(json);
        return data != null && !string.IsNullOrWhiteSpace(data.sceneName);
    }

    private static Plot FindPlotById(Plot[] plots, int plotId)
    {
        foreach (Plot plot in plots)
        {
            if (plot.PlotId == plotId)
                return plot;
        }

        return null;
    }
}
