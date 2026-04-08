using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBase : MonoBehaviour
{
    public GameObject Health1;
    public GameObject Health2;
    public GameObject Health3;
    public GameObject Health4;

    private int health;

    void Start()
    {
      
        health = 4;
    }

    void Update()
    {
       
        if (Input.GetKeyDown(KeyCode.A))
        {
            health--;
            UpdateHealthDisplay();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            health++;
            UpdateHealthDisplay();
        }
    }

   
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health < 0) health = 0; 
        UpdateHealthDisplay();
    }

    private void UpdateHealthDisplay()
    {
    
        switch (health)
        {
            case 0:
                Health1.SetActive(false);
                Health2.SetActive(false);
                Health3.SetActive(false);
                Health4.SetActive(false);
                break;
            case 1:
                Health1.SetActive(true);
                Health2.SetActive(false);
                Health3.SetActive(false);
                Health4.SetActive(false);
                break;
            case 2:
                Health1.SetActive(true);
                Health2.SetActive(true);
                Health3.SetActive(false);
                Health4.SetActive(false);
                break;
            case 3:
                Health1.SetActive(true);
                Health2.SetActive(true);
                Health3.SetActive(true);
                Health4.SetActive(false);
                break;
            case 4:
                Health1.SetActive(true);
                Health2.SetActive(true);
                Health3.SetActive(true);
                Health4.SetActive(true);
                break;
        }
    }
}