using UnityEngine;

public class BuildMananger : MonoBehaviour
{
    public static BuildMananger main;

    [Header("References")]
    [SerializeField] private Tower[] towers;

    private int selectedTower = 0;

    public Plot ActivePlacingPlot { get; private set; }

    private void Awake()
    {
        main = this;
    }

    public Tower GetSelectedTower () 
    {  
        return towers[selectedTower];
    }

    public GameObject GetPrefabByType(TowerType type)
    {
        foreach (Tower tower in towers)
        {
            if (tower.type == type)
                return tower.prefab;
        }

        return null;
    }
    public void SetSelectedTower(int _selectedTower)
    {
        if (PauseMenuController.IsPaused) return;

        CancelActivePlacement();
        selectedTower = _selectedTower;
    }

    public void BeginPlacement(Plot plot)
    {
        if (ActivePlacingPlot != null && ActivePlacingPlot != plot)
            ActivePlacingPlot.CancelPlacement();

        ActivePlacingPlot = plot;
    }

    public void CompletePlacement(Plot plot)
    {
        if (ActivePlacingPlot == plot)
            ActivePlacingPlot = null;
    }

    public bool CancelActivePlacement()
    {
        if (ActivePlacingPlot == null)
            return false;

        Plot plot = ActivePlacingPlot;
        ActivePlacingPlot = null;
        plot.CancelPlacement();
        return true;
    }
}
