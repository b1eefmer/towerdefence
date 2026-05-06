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

    private void Awake()
    {
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

        if (UIManager.main != null && UIManager.main.IsHoveringUI()) return;

        if (!canBuild)
        {
            Debug.Log("Cant build here!");
            return;
        }

        if (towerObj != null)
        {
            if (turret != null)
                turret.OpenUpgradeUI();
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
        turret = towerObj.GetComponent<Turret>();

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

        if (turret != null)
        {
            turret.SetLevel(towerLevel);
        }

        UpdateColor();
    }
}
