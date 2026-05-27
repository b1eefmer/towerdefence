using UnityEngine;

public abstract class PlacedTower : MonoBehaviour
{
    private static PlacedTower selectedTower;

    protected Plot ownerPlot;
    protected int totalSpent;
    protected int level = 1;
    private UpgradeUIHandler actionUI;
    private SpriteRenderer[] highlightSprites;
    private Color[] originalSpriteColors;

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

    public int GetTotalSpent()
    {
        return totalSpent;
    }

    public virtual Vector2 GetDirection()
    {
        return Vector2.zero;
    }

    public bool CanUpgrade()
    {
        return LevelMananger.main != null &&
               LevelMananger.main.currency >= GetUpgradeCost();
    }

    public virtual void OpenUpgradeUI()
    {
        if (PauseMenuController.IsPaused) return;

        if (selectedTower != null && selectedTower != this)
            selectedTower.Deselect();

        bool wasAlreadySelected = selectedTower == this;
        selectedTower = this;

        if (!wasAlreadySelected)
            SetSelectedVisual(true);

        if (actionUI == null)
            actionUI = UpgradeUIHandler.Create(transform);

        actionUI.Bind(this);
        actionUI.gameObject.SetActive(true);
    }

    public void Deselect()
    {
        SetSelectedVisual(false);

        if (selectedTower == this)
            selectedTower = null;
    }

    public virtual void Sell()
    {
        Deselect();

        if (UIManager.main != null)
            UIManager.main.SetHoveringState(false);

        if (LevelMananger.main != null)
            LevelMananger.main.IncreaseCurrency(GetSellValue());

        if (ownerPlot != null)
            ownerPlot.OnTowerSold();

        Destroy(gameObject);
    }

    private void SetSelectedVisual(bool selected)
    {
        if (selected)
        {
            highlightSprites = GetComponentsInChildren<SpriteRenderer>();
            originalSpriteColors = new Color[highlightSprites.Length];

            for (int i = 0; i < highlightSprites.Length; i++)
            {
                originalSpriteColors[i] = highlightSprites[i].color;
                Color selectedColor = Color.Lerp(highlightSprites[i].color, Color.cyan, 0.35f);
                selectedColor.a = highlightSprites[i].color.a;
                highlightSprites[i].color = selectedColor;
            }

            return;
        }

        if (highlightSprites == null || originalSpriteColors == null)
            return;

        for (int i = 0; i < highlightSprites.Length; i++)
        {
            if (highlightSprites[i] != null && i < originalSpriteColors.Length)
                highlightSprites[i].color = originalSpriteColors[i];
        }

        highlightSprites = null;
        originalSpriteColors = null;
    }

    public virtual void RestoreState(int savedLevel, int savedTotalSpent)
    {
        level = savedLevel;
        totalSpent = savedTotalSpent;
    }
}
