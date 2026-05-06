using UnityEngine;

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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
