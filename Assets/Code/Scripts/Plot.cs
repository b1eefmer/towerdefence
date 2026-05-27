using UnityEngine;

public class Plot : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private int plotId;
    public int PlotId => plotId;

    [Header("References")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Color hoverColor;

    [Header("Audio")]
    [SerializeField] private AudioSource buySound;

    private GameObject towerObj;
    private PlacedTower placedTower;
    private Color startColor;

    private void Start()
    {
        startColor = sr.color;
    }

    private void OnMouseEnter()
    {
        if (PauseMenuController.IsPaused) return;

        sr.color = hoverColor;
    }

    private void OnMouseExit()
    {
        sr.color = startColor;
    }

    private void OnMouseDown()
    {
        if (PauseMenuController.IsPaused) return;
        
        if (UIManager.main != null && UIManager.main.IsHoveringUI()) return;

        
        if (towerObj != null)
        {
            if (placedTower != null)
                placedTower.OpenUpgradeUI();
            return;
        }

        Tower towerToBuild = BuildMananger.main.GetSelectedTower();
        if (towerToBuild.cost > LevelMananger.main.currency)
        {
            Debug.Log("You can't afford this tower");
            return;
        }

        LevelMananger.main.SpendCurrency(towerToBuild.cost);
        if (buySound != null) buySound.Play();

        towerObj = Instantiate(towerToBuild.prefab, transform.position, Quaternion.identity);
        placedTower = towerObj.GetComponent<PlacedTower>();
        if (placedTower != null)
            placedTower.Initialize(Vector2.zero, this, towerToBuild.cost);
    }

    public void OnTowerSold()
    {
        towerObj = null;
        placedTower = null;
    }

    public void RestoreTower(TowerType type, int savedLevel, int savedTotalSpent, Vector2 direction)
    {
        GameObject prefab = BuildMananger.main.GetPrefabByType(type);
        if (prefab == null)
        {
            Debug.LogError($"No prefab configured for tower type {type}.");
            return;
        }

        towerObj = Instantiate(prefab, transform.position, Quaternion.identity);
        placedTower = towerObj.GetComponent<PlacedTower>();
        if (placedTower == null)
        {
            Debug.LogError($"Prefab for tower type {type} does not contain PlacedTower.");
            Destroy(towerObj);
            towerObj = null;
            return;
        }

        placedTower.Initialize(direction, this, savedTotalSpent);
        placedTower.RestoreState(savedLevel, savedTotalSpent);
    }
}
