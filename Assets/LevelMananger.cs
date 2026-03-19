using UnityEngine;

public class LevelMananger : MonoBehaviour
{
    public static LevelMananger main;

    public Transform startPoint;
    public Transform[] path;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        main = this;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
