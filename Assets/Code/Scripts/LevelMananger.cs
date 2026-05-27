using System;
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

    public int currency;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        GameSession.EnsureInstance();
        main = this;
    }
    private void Start()
    {
        currency = GameSession.Instance.hasCarriedCurrency
            ? GameSession.Instance.carriedCurrency
            : 1000;

        GameSession.Instance.levelEntrySnapshot = currency;
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
        if (paths == null || index < 0 || index >= paths.Length)
        {
            Debug.LogError($"Path index {index} is not configured.");
            return null;
        }

        return paths[index].waypoints;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
