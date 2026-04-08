using UnityEngine;
using System.Collections;
using TMPro;

public class Health : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private int hitPoint = 2;
    [SerializeField] private int currencyWorth = 50;
    private bool isDestroyed = false;

    [Header("Effects")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color hitColor = Color.white;
    [SerializeField] private float hitFlashDuration = 0.1f;
    [SerializeField] private GameObject hitSparkPrefab;
    [SerializeField] private GameObject floatingTextPrefab;

    [Header("Audio")]
    [SerializeField] private AudioSource hitSound;
    [SerializeField] private AudioSource deathSound;

    private Color originalColor;

    private void Start()
    {
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void TakeDamage(int dmg)
    {
        if (isDestroyed) return;

        hitPoint -= dmg;

        if (spriteRenderer != null)
            StartCoroutine(FlashCoroutine());
        if (hitSparkPrefab != null)
            ShowHitEffect();
        if (hitSound != null)
            hitSound.Play();

        if (hitPoint <= 0)
        {
        
            EnemySpawner.onEnemyDestroy.Invoke();
            LevelMananger.main.IncreaseCurrency(currencyWorth);
            ShowFloatingText(currencyWorth);

            
            EnemySpawner.AddStats(currencyWorth);

            if (deathSound != null)
                deathSound.Play();

            isDestroyed = true;
            Destroy(gameObject);
        }
    }

    private IEnumerator FlashCoroutine()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(hitFlashDuration);
        spriteRenderer.color = originalColor;
    }

    private void ShowHitEffect()
    {
        GameObject spark = Instantiate(hitSparkPrefab, transform.position, Quaternion.identity);
        Destroy(spark, 0.5f);
    }

    private void ShowFloatingText(int amount)
    {
        if (floatingTextPrefab == null) return;
        GameObject textObj = Instantiate(floatingTextPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
        var tmp = textObj.GetComponent<TextMeshProUGUI>();
        if (tmp != null) tmp.text = $"+{amount}";
        Destroy(textObj, 1f);
    }
}