using UnityEngine;
using UnityEngine.EventSystems;

public class Plot : MonoBehaviour
{
    private enum PlotState
    {
        Empty,
        Placing,
        Built
    }

    [Header("Identity")]
    [SerializeField] private int plotId;
    public int PlotId => plotId;

    [Header("References")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Color hoverColor;

    [Header("Audio")]
    [SerializeField] private AudioSource buySound;

    [Header("Placement Preview")]
    [SerializeField] private Color placementArrowColor = Color.yellow;
    [SerializeField] private float placementArrowLength = 0.65f;
    [SerializeField] private float placementArrowWidth = 0.06f;

    private GameObject towerObj;
    private PlacedTower placedTower;
    private Color startColor;
    private PlotState state = PlotState.Empty;
    private Tower pendingTower;
    private Vector2 chosenDirection = Vector2.up;
    private GameObject placementArrowObject;
    private LineRenderer placementArrow;
    private Material placementArrowMaterial;

    private void Start()
    {
        startColor = sr.color;
    }

    private void Update()
    {
        if (state != PlotState.Placing || PauseMenuController.IsPaused)
            return;

        if (Input.GetKeyDown(KeyCode.W))
            SetDirection(Vector2.up);
        else if (Input.GetKeyDown(KeyCode.S))
            SetDirection(Vector2.down);
        else if (Input.GetKeyDown(KeyCode.A))
            SetDirection(Vector2.left);
        else if (Input.GetKeyDown(KeyCode.D))
            SetDirection(Vector2.right);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            ConfirmPlacement();
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

        if (state == PlotState.Built || towerObj != null)
        {
            BuildMananger.main.CancelActivePlacement();

            if (placedTower != null)
                placedTower.OpenUpgradeUI();
            return;
        }

        Tower towerToBuild = BuildMananger.main.GetSelectedTower();

        if (towerToBuild.type == TowerType.Slow)
        {
            BuildMananger.main.CancelActivePlacement();
            PlaceImmediately(towerToBuild);
            return;
        }

        EnterPlacementMode(towerToBuild);
    }

    private void PlaceImmediately(Tower towerToBuild)
    {
        if (towerToBuild.cost > LevelMananger.main.currency)
        {
            Debug.Log("You can't afford this tower");
            return;
        }

        BuildTower(towerToBuild, Vector2.zero);
    }

    private void EnterPlacementMode(Tower towerToBuild)
    {
        BuildMananger.main.BeginPlacement(this);
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        pendingTower = towerToBuild;
        chosenDirection = Vector2.up;
        state = PlotState.Placing;
        ShowPlacementArrow();
    }

    private void ConfirmPlacement()
    {
        if (pendingTower == null)
            return;

        if (pendingTower.cost > LevelMananger.main.currency)
        {
            Debug.Log("You can't afford this tower");
            return;
        }

        Tower towerToBuild = pendingTower;
        Vector2 direction = chosenDirection;
        ClearPlacementMode();
        BuildMananger.main.CompletePlacement(this);
        BuildTower(towerToBuild, direction);
    }

    private void BuildTower(Tower towerToBuild, Vector2 direction)
    {
        LevelMananger.main.SpendCurrency(towerToBuild.cost);
        if (buySound != null)
            buySound.Play();

        towerObj = Instantiate(towerToBuild.prefab, transform.position, Quaternion.identity);
        placedTower = towerObj.GetComponent<PlacedTower>();
        if (placedTower != null)
            placedTower.Initialize(direction, this, towerToBuild.cost);

        state = PlotState.Built;
    }

    public void CancelPlacement()
    {
        if (state != PlotState.Placing)
            return;

        ClearPlacementMode();
    }

    private void ClearPlacementMode()
    {
        pendingTower = null;
        state = PlotState.Empty;
        HidePlacementArrow();
    }

    private void SetDirection(Vector2 direction)
    {
        chosenDirection = direction;
        UpdatePlacementArrow();
    }

    private void ShowPlacementArrow()
    {
        if (placementArrowObject == null)
        {
            placementArrowObject = new GameObject("PlacementDirection");
            placementArrowObject.transform.SetParent(transform, false);
            placementArrow = placementArrowObject.AddComponent<LineRenderer>();
            placementArrowMaterial = new Material(Shader.Find("Sprites/Default"));
            placementArrow.material = placementArrowMaterial;
            placementArrow.useWorldSpace = false;
            placementArrow.positionCount = 5;
            placementArrow.startWidth = placementArrowWidth;
            placementArrow.endWidth = placementArrowWidth;
            placementArrow.startColor = placementArrowColor;
            placementArrow.endColor = placementArrowColor;
            placementArrow.sortingLayerID = sr.sortingLayerID;
            placementArrow.sortingOrder = sr.sortingOrder + 20;
        }

        placementArrowObject.SetActive(true);
        UpdatePlacementArrow();
    }

    private void UpdatePlacementArrow()
    {
        if (placementArrow == null)
            return;

        Vector2 tip = chosenDirection * placementArrowLength;
        Vector2 side = new Vector2(-chosenDirection.y, chosenDirection.x) * (placementArrowLength * 0.2f);
        Vector2 arrowBase = tip - chosenDirection * (placementArrowLength * 0.25f);

        placementArrow.SetPosition(0, Vector3.zero);
        placementArrow.SetPosition(1, tip);
        placementArrow.SetPosition(2, arrowBase + side);
        placementArrow.SetPosition(3, tip);
        placementArrow.SetPosition(4, arrowBase - side);
    }

    private void HidePlacementArrow()
    {
        if (placementArrowObject != null)
            placementArrowObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (placementArrowMaterial != null)
            Destroy(placementArrowMaterial);
    }

    public void OnTowerSold()
    {
        towerObj = null;
        placedTower = null;
        state = PlotState.Empty;
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
        state = PlotState.Built;
    }
}
