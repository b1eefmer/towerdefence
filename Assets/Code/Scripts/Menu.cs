using TMPro;
using UnityEngine;

public class Menu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] TextMeshProUGUI currencyUI;
    [SerializeField] Animator anim;
    [SerializeField] float toggleDebounceSeconds = 0.2f;
    private bool isMenuOpen = true;
    private int lastToggleFrame = -1;
    private float lastToggleTime = float.NegativeInfinity;

    private void Awake()
    {
        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }

        if (anim != null)
        {
            isMenuOpen = anim.GetBool("MenuOpen");
        }

        if (currencyUI == null)
        {
            GameObject currencyObject = GameObject.Find("GoldText (1)");
            if (currencyObject != null)
            {
                currencyUI = currencyObject.GetComponent<TextMeshProUGUI>();
            }
        }
    }

    public void ToogleMenu()
    {
        if (anim == null)
        {
            Debug.LogWarning("Menu animator is missing.");
            return;
        }

        if (lastToggleFrame == Time.frameCount)
        {
            return;
        }

        if (Time.unscaledTime - lastToggleTime < toggleDebounceSeconds)
        {
            return;
        }

        lastToggleFrame = Time.frameCount;
        lastToggleTime = Time.unscaledTime;

        isMenuOpen = !anim.GetBool("MenuOpen");
        anim.SetBool("MenuOpen", isMenuOpen);
    }

    public void ToggleMenu()
    {
        ToogleMenu();
    }

    private void Update()
    {
        if (currencyUI == null || LevelMananger.main == null)
        {
            return;
        }

        currencyUI.text = LevelMananger.main.currency.ToString();
    }
    //public void SetSelected()
    //{

    //}
}
