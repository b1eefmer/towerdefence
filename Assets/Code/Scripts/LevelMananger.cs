using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyPath
{
    public Transform[] waypoints;
}

public class LevelMananger : MonoBehaviour
{
    public static LevelMananger main;

    public Transform startPoint;
    [SerializeField] private EnemyPath[] paths;
    [SerializeField] private int startingCurrency = 10000;
    [SerializeField] private int minimumStartingCurrency;

    public int currency;
    private readonly List<Transform[]> runtimePaths = new List<Transform[]>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        GameSession.EnsureInstance();
        main = this;
        BuildRuntimePaths();
    }
    private void Start()
    {
        InitializeCurrency();
        StartCoroutine(ApplyPendingLoadAfterSceneStart());
    }

    private void InitializeCurrency()
    {
        int baseCurrency = GameSession.Instance.hasCarriedCurrency
            ? GameSession.Instance.carriedCurrency
            : startingCurrency;

        currency = Mathf.Max(baseCurrency, minimumStartingCurrency);
        GameSession.Instance.levelEntrySnapshot = currency;
    }

    private IEnumerator ApplyPendingLoadAfterSceneStart()
    {
        yield return null;
        SaveSystem.ApplyPendingLoadIfAny();
    }

    public void IncreaseCurrency(int amount)
    {
        currency += amount;
    }

    public void CarryCurrencyToNextLevel()
    {
        GameSession.Instance.carriedCurrency = currency;
        GameSession.Instance.hasCarriedCurrency = true;
    }

    public bool SpendCurrency(int amount)
    {
        if (amount <= currency)
        {
            // BUY ITEM
            currency -= amount;
            return true;
        }
        else
        {
            Debug.Log("You do not have enough to purchase this item");
            return false;
        }
    }

    public Transform[] GetPath(int index)
    {
        if (runtimePaths.Count == 0)
            BuildRuntimePaths();

        if (index < 0 || index >= runtimePaths.Count)
        {
            Debug.LogError($"Path index {index} is not configured.");
            return null;
        }

        return runtimePaths[index];
    }

    private void BuildRuntimePaths()
    {
        runtimePaths.Clear();

        AddNamedPath("Path1");
        AddNamedPath("Path2");
        AddNamedPath("FlyPath1");
        AddNamedPath("FlyPath2");

        if (runtimePaths.Count > 0)
            return;

        if (paths == null)
            return;

        foreach (EnemyPath path in paths)
        {
            if (path != null && IsValidPath(path.waypoints))
                runtimePaths.Add(path.waypoints);
        }
    }

    private void AddNamedPath(string pathObjectName)
    {
        GameObject pathObject = GameObject.Find(pathObjectName);
        if (pathObject == null)
            return;

        Transform pathRoot = pathObject.transform;
        Transform[] waypoints = new Transform[pathRoot.childCount];
        for (int i = 0; i < pathRoot.childCount; i++)
            waypoints[i] = pathRoot.GetChild(i);

        if (IsValidPath(waypoints))
            runtimePaths.Add(waypoints);
    }

    private bool IsValidPath(Transform[] waypoints)
    {
        if (waypoints == null || waypoints.Length == 0)
            return false;

        foreach (Transform waypoint in waypoints)
        {
            if (waypoint == null)
                return false;
        }

        return true;
    }
}
