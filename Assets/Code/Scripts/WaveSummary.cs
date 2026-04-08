using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveSummary : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI killsText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Button continueButton;

    private int killsInWave;
    private int goldEarnedInWave;
    private float waveDuration;

    private void Start()
    {
        gameObject.SetActive(false);
        if (continueButton != null)
            continueButton.onClick.AddListener(HideSummary);
    }

    public void ShowSummary(int kills, int gold, float duration)
    {
        killsInWave = kills;
        goldEarnedInWave = gold;
        waveDuration = duration;

        killsText.text = $"Kills: {kills}";
        goldText.text = $"Gold: {gold}";
        timeText.text = $"Time: {duration:F1} s";

        gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    private void HideSummary()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
}