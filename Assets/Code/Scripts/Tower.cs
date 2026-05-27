using System;
using UnityEngine;

public enum TowerType
{
    Normal,
    Slow,
    AntiAir
}

[Serializable]
public class Tower
{
    public string name;
    public int cost;
    public GameObject prefab;
    public TowerType type;

    public Tower (string _name, int _cost, GameObject _prefab)
    {
        name = _name;
        cost = _cost;
        prefab = _prefab;
        type = TowerType.Normal;
    }
}
