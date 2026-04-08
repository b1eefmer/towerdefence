using UnityEngine;

public class LevelMananger : MonoBehaviour
{
    public static LevelMananger main;

    public Transform startPoint;
    public Transform[] path;

    public int currency;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        main = this;
    }
    void Start()
    {
        currency = 150;
    }
    public void IncreaseCurrency(int amount)
    {
        currency += amount;
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
