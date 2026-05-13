using UnityEngine;

public class Plot : MonoBehaviour
{
    [Header("Build Settings")]
    [SerializeField] private bool canBuild = true;

    [Header("References")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Color hoverColor;
    [SerializeField] private Color blockedColor = Color.gray;

    [Header("Audio")]
    [SerializeField] private AudioSource buySound;

    private GameObject towerObj;
    private Turret turret;
    private Color startColor;
    private bool initialCanBuild;
    private static Plot selectedTowerPlot;

    private void Awake()
    {
        initialCanBuild = canBuild;
        CacheStartColor();
    }

    private void Start()
    {
        CacheStartColor();
        UpdateColor();
    }

    private void CacheStartColor()
    {
        if (sr != null && startColor.a == 0f && startColor.r == 0f && startColor.g == 0f && startColor.b == 0f)
        {
            startColor = sr.color;
        }
    }

    private void UpdateColor()
    {
        if (sr == null)
        {
            return;
        }

        CacheStartColor();

        if (!canBuild || towerObj != null)
        {
            sr.color = blockedColor;
        }
        else
        {
            sr.color = startColor;
        }
    }
    private void OnMouseEnter()
    {
        if (!canBuild) return;
        sr.color = hoverColor;
    }

    private void OnMouseExit()
    {
        UpdateColor();
    }

    private void OnMouseDown()
    {
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
        {
            return;
        }

        if (UIManager.main != null && (UIManager.main.IsHoveringUI() || UIManager.main.IsPointerOverBlockingUI()))
        {
            return;
        }

        if (towerObj != null)
        {
            SetSelectedTowerPlot(this);

            if (turret != null)
            {
                turret.OpenUpgradeUI();
            }
            return;
        }

        if (!canBuild)
        {
            Debug.Log("Cant build here!");
            return;
        }

        SetSelectedTowerPlot(null);

        Tower towerToBuild = BuildMananger.main.GetSelectedTower();
        if (towerToBuild.cost > LevelMananger.main.currency)
        {
            Debug.Log("You can't afford this tower");
            return;
        }

        LevelMananger.main.SpendCurrency(towerToBuild.cost);
        if (buySound != null) buySound.Play();

        towerObj = Instantiate(towerToBuild.prefab, transform.position, Quaternion.identity);
        turret = towerObj.GetComponent<Turret>();
        canBuild = false;

        UpdateColor();
    }

    public bool HasTower()
    {
        return towerObj != null;
    }

    public GameObject GetTowerObject()
    {
        return towerObj;
    }

    public void ClearTower()
    {
        if (towerObj != null)
        {
            Destroy(towerObj);
            towerObj = null;
            turret = null;
        }

        canBuild = initialCanBuild;

        if (selectedTowerPlot == this)
        {
            selectedTowerPlot = null;
        }

        UpdateColor();
    }

    public void RestoreTower(GameObject towerPrefab, int towerLevel)
    {
        if (towerPrefab == null)
        {
            return;
        }

        ClearTower();

        towerObj = Instantiate(towerPrefab, transform.position, Quaternion.identity);
        turret = towerObj.GetComponent<Turret>();
        canBuild = false;

        if (turret != null)
        {
            turret.SetLevel(towerLevel);
        }

        UpdateColor();
    }

    private static void SetSelectedTowerPlot(Plot plot)
    {
        if (selectedTowerPlot != null && selectedTowerPlot != plot && selectedTowerPlot.turret != null)
        {
            selectedTowerPlot.turret.CloseUpgradeUI();
        }

        selectedTowerPlot = plot;
    }
}
