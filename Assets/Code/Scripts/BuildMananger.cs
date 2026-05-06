using UnityEngine;

using UnityEngine.UI;

public class BuildMananger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static BuildMananger main;
    [Header("References")]
    //[SerializeField] private GameObject[] towerPrefabs;
    [SerializeField] private Tower[] towers;

    private int selectedTower = 0;
    private void Awake()
    {
        main = this;
    }
    public Tower GetSelectedTower () 
    {  
        return towers[selectedTower];
    }
    public void SetSelectedTower(int _selectedTower)
    {
        selectedTower = _selectedTower;
    }

    public GameObject GetTowerPrefab(int towerIndex)
    {
        if (towerIndex < 0 || towerIndex >= towers.Length)
        {
            return null;
        }

        return towers[towerIndex].prefab;
    }

    public int GetTowerIndexByInstance(GameObject towerInstance)
    {
        if (towerInstance == null)
        {
            return -1;
        }

        return GetTowerIndexByPrefabName(towerInstance.name);
    }

    public int GetTowerIndexByPrefabName(string prefabName)
    {
        string normalizedName = NormalizeName(prefabName);

        for (int i = 0; i < towers.Length; i++)
        {
            GameObject prefab = towers[i].prefab;
            if (prefab != null && NormalizeName(prefab.name) == normalizedName)
            {
                return i;
            }
        }

        return -1;
    }

    private string NormalizeName(string objectName)
    {
        return objectName.Replace("(Clone)", "").Trim();
    }
    void Start()
    {
        BindTowerSelectionButtonsRuntime();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void BindTowerSelectionButtonsRuntime()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (Button button in buttons)
        {
            if (button == null)
            {
                continue;
            }

            if (!TryGetTowerIndexForButton(button.name, out int towerIndex))
            {
                continue;
            }

            if (HasValidTowerSelectionBinding(button, towerIndex))
            {
                continue;
            }

            int capturedTowerIndex = towerIndex;
            button.onClick.AddListener(() => SetSelectedTower(capturedTowerIndex));
        }
    }

    private bool TryGetTowerIndexForButton(string buttonName, out int towerIndex)
    {
        switch (buttonName)
        {
            case "Basic Turret":
                towerIndex = 0;
                return true;
            case "Sniper Turret":
                towerIndex = 1;
                return true;
            case "Slowmo Turret":
                towerIndex = 2;
                return true;
            default:
                towerIndex = -1;
                return false;
        }
    }

    private bool HasValidTowerSelectionBinding(Button button, int expectedTowerIndex)
    {
        int persistentEventCount = button.onClick.GetPersistentEventCount();
        for (int i = 0; i < persistentEventCount; i++)
        {
            if (button.onClick.GetPersistentMethodName(i) != "SetSelectedTower")
            {
                continue;
            }

            Object target = button.onClick.GetPersistentTarget(i);
            if (target is BuildMananger)
            {
                return true;
            }
        }

        return false;
    }
}
