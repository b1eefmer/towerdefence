using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private int hitPoint = 2;
    [SerializeField] private bool dropsCurrency;
    [SerializeField] private int currencyDropValue = 10;
    [SerializeField] private CurrencyDrop currencyDropPrefab;
    private bool isDestroyed = false;

    [Header("Effects")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color hitColor = Color.white;
    [SerializeField] private float hitFlashDuration = 0.1f;
    [SerializeField] private GameObject hitSparkPrefab;

    [Header("Audio")]
    [SerializeField] private AudioSource hitSound;
    [SerializeField] private AudioSource deathSound;

    private Color originalColor;
    private Coroutine flashCoroutine;

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
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            flashCoroutine = StartCoroutine(FlashCoroutine());
        }
        if (hitSparkPrefab != null)
            ShowHitEffect();
        if (hitSound != null)
            hitSound.Play();

        if (hitPoint <= 0)
        {
            isDestroyed = true;
            EnemySpawner.onEnemyRemoved.Invoke();
            EnemySpawner.RegisterKill();

            if (dropsCurrency && currencyDropPrefab != null)
            {
                CurrencyDrop drop = Instantiate(
                    currencyDropPrefab,
                    transform.position,
                    Quaternion.identity);
                drop.SetValue(currencyDropValue);
            }

            if (deathSound != null)
                deathSound.Play();

            Destroy(gameObject);
        }
    }

    private IEnumerator FlashCoroutine()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(hitFlashDuration);
        spriteRenderer.color = originalColor;
        flashCoroutine = null;
    }

    private void ShowHitEffect()
    {
        GameObject spark = Instantiate(hitSparkPrefab, transform.position, Quaternion.identity);
        Destroy(spark, 0.5f);
    }

}
