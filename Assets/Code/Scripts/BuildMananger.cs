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

        selectedTower = _selectedTower;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
