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
    [SerializeField, Range(0.1f, 1f)] private float placementPreviewAlpha = 0.65f;
    [SerializeField] private Color placementRangeColor = new Color(0f, 0.85f, 1f, 0.85f);
    [SerializeField] private float placementRangeLineWidth = 0.06f;
    [SerializeField] private float placementRangeWidthScale = 0.5f;

    private GameObject towerObj;
    private PlacedTower placedTower;
    private Color startColor;
    private PlotState state = PlotState.Empty;
    private Tower pendingTower;
    private Vector2 chosenDirection = Vector2.up;
    private GameObject placementPreviewObject;
    private PlacedTower placementPreviewTower;
    private GameObject placementRangeObject;
    private LineRenderer placementRangeRenderer;
    private Material placementRangeMaterial;

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
        ShowPlacementPreview();
    }

    public void ConfirmPlacement()
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
            VolumeSettings.PlaySfx(buySound);

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
        HidePlacementPreview();
    }

    private void SetDirection(Vector2 direction)
    {
        chosenDirection = direction;
        UpdatePlacementPreview();
    }

    private void ShowPlacementPreview()
    {
        HidePlacementPreview();

        placementPreviewObject = Instantiate(pendingTower.prefab, transform.position, Quaternion.identity);
        placementPreviewObject.name = $"{pendingTower.name} Preview";
        placementPreviewTower = placementPreviewObject.GetComponent<PlacedTower>();

        if (placementPreviewTower != null)
            placementPreviewTower.SetPreviewDirection(chosenDirection);

        foreach (MonoBehaviour behaviour in placementPreviewObject.GetComponentsInChildren<MonoBehaviour>(true))
            behaviour.enabled = false;

        foreach (Collider2D collider in placementPreviewObject.GetComponentsInChildren<Collider2D>(true))
            collider.enabled = false;

        foreach (AudioSource audioSource in placementPreviewObject.GetComponentsInChildren<AudioSource>(true))
            audioSource.enabled = false;

        foreach (Canvas canvas in placementPreviewObject.GetComponentsInChildren<Canvas>(true))
            canvas.gameObject.SetActive(false);

        foreach (SpriteRenderer sprite in placementPreviewObject.GetComponentsInChildren<SpriteRenderer>(true))
        {
            Color color = sprite.color;
            color.a *= placementPreviewAlpha;
            sprite.color = color;
            sprite.sortingOrder += 20;
        }

        ShowPlacementRange();
    }

    private void UpdatePlacementPreview()
    {
        if (placementPreviewTower != null)
            placementPreviewTower.SetPreviewDirection(chosenDirection);

        UpdatePlacementRange();
    }

    private void HidePlacementPreview()
    {
        if (placementPreviewObject != null)
            Destroy(placementPreviewObject);

        placementPreviewObject = null;
        placementPreviewTower = null;
        HidePlacementRange();
    }

    private void OnDestroy()
    {
        HidePlacementPreview();

        if (placementRangeMaterial != null)
            Destroy(placementRangeMaterial);
    }

    private void ShowPlacementRange()
    {
        if (placementPreviewTower == null ||
            !placementPreviewTower.TryGetTargetingBox(out float range, out float width) ||
            range <= 0f ||
            width <= 0f)
        {
            HidePlacementRange();
            return;
        }

        if (placementRangeObject == null)
        {
            placementRangeObject = new GameObject("PlacementTargetingBox");
            placementRangeRenderer = placementRangeObject.AddComponent<LineRenderer>();
            placementRangeMaterial = new Material(Shader.Find("Sprites/Default"));
            placementRangeRenderer.material = placementRangeMaterial;
            placementRangeRenderer.useWorldSpace = true;
            placementRangeRenderer.loop = true;
            placementRangeRenderer.positionCount = 4;
            placementRangeRenderer.startWidth = placementRangeLineWidth;
            placementRangeRenderer.endWidth = placementRangeLineWidth;
            placementRangeRenderer.startColor = placementRangeColor;
            placementRangeRenderer.endColor = placementRangeColor;
            placementRangeRenderer.sortingLayerID = sr.sortingLayerID;
            placementRangeRenderer.sortingOrder = sr.sortingOrder + 30;
        }

        placementRangeObject.SetActive(true);
        UpdatePlacementRange(range, width);
    }

    private void UpdatePlacementRange()
    {
        if (placementRangeRenderer == null ||
            placementPreviewTower == null ||
            !placementPreviewTower.TryGetTargetingBox(out float range, out float width))
            return;

        UpdatePlacementRange(range, width);
    }

    private void UpdatePlacementRange(float range, float width)
    {
        Vector2 direction = chosenDirection == Vector2.zero ? Vector2.up : chosenDirection.normalized;
        width *= placementRangeWidthScale;
        Vector2 side = new Vector2(-direction.y, direction.x);
        Vector2 origin = transform.position;
        Vector2 startLeft = origin + side * (width * 0.5f);
        Vector2 startRight = origin - side * (width * 0.5f);
        Vector2 endLeft = direction * range + startLeft;
        Vector2 endRight = direction * range + startRight;

        placementRangeRenderer.SetPosition(0, startLeft);
        placementRangeRenderer.SetPosition(1, endLeft);
        placementRangeRenderer.SetPosition(2, endRight);
        placementRangeRenderer.SetPosition(3, startRight);
    }

    private void HidePlacementRange()
    {
        if (placementRangeObject != null)
            placementRangeObject.SetActive(false);
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
