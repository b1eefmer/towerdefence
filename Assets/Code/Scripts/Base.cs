using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BaseHealth : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private int maxLives = 3;
    private int currentLives;
    private bool isGameOver = false;

    [Header("UI")]
    [SerializeField] private Image[] hearts;

    [Header("Effects")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource damageSound;

    [Header("Audio")]
    [SerializeField] private AudioSource baseHitSound;
    [SerializeField] private AudioSource gameOverSound;
    [SerializeField] private MusicManager musicManager;

    [Header("Game Over")]
    [SerializeField] private GameOverUI gameOverUI;

    private void Start()
    {
        currentLives = maxLives;
        UpdateHearts();
        isGameOver = false;
    }

    public void TakeDamage(int amount)
    {
        if (isGameOver) return;

        currentLives -= amount;
        currentLives = Mathf.Clamp(currentLives, 0, maxLives);
        UpdateHearts();

        if (animator != null) animator.SetTrigger("Damage");
        if (damageSound != null) VolumeSettings.PlaySfx(damageSound);
        if (baseHitSound != null) VolumeSettings.PlaySfx(baseHitSound);

        if (currentLives <= 0)
        {
            GameOver();
        }
    }

    public void SetLives(int lives)
    {
        currentLives = Mathf.Clamp(lives, 0, maxLives);
        UpdateHearts();
    }

    public int GetLives()
    {
        return currentLives;
    }

    private void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
            hearts[i].gameObject.SetActive(i < currentLives);
    }

    private void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("GAME OVER");

        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        int totalKills = 0;
        int totalGold = 0;
        float gameTime = 0f;
        if (spawner != null)
        {
            
            totalKills = spawner.GetTotalKills();
            totalGold = spawner.GetTotalGold();
            gameTime = spawner.GetGameTime();
        }

        if (musicManager != null)
            musicManager.StopMusic();

        if (gameOverSound != null)
            VolumeSettings.PlaySfx(gameOverSound);

        if (gameOverUI != null)
            gameOverUI.ShowGameOver(totalKills, totalGold, gameTime);
        else
            Time.timeScale = 0f;
    }
}
