using TMPro;
using UnityEngine;

public class Menu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TextMeshProUGUI currencyUI;
    [SerializeField] Animator anim;
    private bool isMenuOpen = true;
    public void ToogleMenu()
    {
        isMenuOpen = !isMenuOpen;
        anim.SetBool("MenuOpen", isMenuOpen);
    }
    private void OnGUI()
    {
        currencyUI.text = LevelMananger.main.currency.ToString();
    }
    //public void SetSelected()
    //{

    //}
    void Start()
    {
        
    }
    
}
