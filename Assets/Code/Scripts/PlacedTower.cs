using UnityEngine;

public abstract class PlacedTower : MonoBehaviour
{
    protected Plot ownerPlot;
    protected int totalSpent;
    protected int level = 1;

    public abstract void Initialize(Vector2 direction, Plot owner, int buildCost);
    public abstract TowerType GetTowerType();
    public abstract int GetUpgradeCost();
    public abstract void Upgrade();

    public virtual void SetPreviewDirection(Vector2 direction)
    {
    }

    public virtual bool TryGetTargetingBox(out float range, out float width)
    {
        range = 0f;
        width = 0f;
        return false;
    }

    public int GetLevel()
    {
        return level;
    }

    public int GetSellValue()
    {
        return totalSpent / 2;
    }

    public bool CanUpgrade()
    {
        return LevelMananger.main != null &&
               LevelMananger.main.currency >= GetUpgradeCost();
    }

    public virtual void OpenUpgradeUI()
    {
    }

    public virtual void Sell()
    {
        if (LevelMananger.main != null)
            LevelMananger.main.IncreaseCurrency(GetSellValue());

        if (ownerPlot != null)
            ownerPlot.OnTowerSold();

        Destroy(gameObject);
    }

    public virtual void RestoreState(int savedLevel, int savedTotalSpent)
    {
        level = savedLevel;
        totalSpent = savedTotalSpent;
    }
}
