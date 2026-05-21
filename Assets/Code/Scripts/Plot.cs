using UnityEngine;

public class Plot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Color hoverColor;

    [Header("Audio")]
    [SerializeField] private AudioSource buySound;

    private GameObject towerObj;
    private Turret turret;
    private Color startColor;

    private void Start()
    {
        startColor = sr.color;
    }

    private void OnMouseEnter()
    {
        if (PauseMenuController.IsPaused) return;
        if (Turret.IsAwaitingDirection) return;

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
        if (Turret.IsAwaitingDirection) return;

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

        towerObj = Instantiate(towerToBuild.prefab, transform.position, Quaternion.identity);
        turret = towerObj.GetComponent<Turret>();

        if (turret != null)
        {
            turret.BeginPlacement(towerToBuild.cost, this);
            if (buySound != null) buySound.Play();
        }
        else
        {
            if (towerToBuild.cost <= LevelMananger.main.currency)
                LevelMananger.main.SpendCurrency(towerToBuild.cost);
            if (buySound != null) buySound.Play();
        }
    }

    /// <summary>
    /// Called by Turret when the player cancels placement. Frees the plot for another purchase.
    /// </summary>
    public void ClearTower()
    {
        towerObj = null;
        turret = null;
        sr.color = startColor;
    }
}
