using System.Collections;
using TMPro;
using UnityEngine;

public class CurrencyDrop : MonoBehaviour
{
    [Header("Value")]
    [SerializeField] private int value = 10;

    [Header("Lifetime")]
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private float warningDuration = 2f;
    [SerializeField] private float blinkInterval = 0.15f;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject floatingTextPrefab;

    private bool collected;

    private void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        StartCoroutine(LifetimeRoutine());
    }

    public void SetValue(int amount)
    {
        value = amount;
    }

    private void OnMouseDown()
    {
        if (collected || PauseMenuController.IsPaused || Time.timeScale == 0f)
            return;

        collected = true;
        LevelMananger.main.IncreaseCurrency(value);
        EnemySpawner.RegisterCollectedGold(value);
        ShowFloatingText(value);
        Destroy(gameObject);
    }

    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, lifetime - warningDuration));

        float remainingTime = Mathf.Min(lifetime, warningDuration);
        while (!collected && remainingTime > 0f)
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = !spriteRenderer.enabled;

            float interval = Mathf.Min(blinkInterval, remainingTime);
            yield return new WaitForSeconds(interval);
            remainingTime -= interval;
        }

        if (!collected)
            Destroy(gameObject);
    }

    private void ShowFloatingText(int amount)
    {
        if (floatingTextPrefab == null)
            return;

        GameObject textObject = Instantiate(
            floatingTextPrefab,
            transform.position + Vector3.up,
            Quaternion.identity);
        TMP_Text text = textObject.GetComponent<TMP_Text>();
        if (text != null)
            text.text = $"+{amount}";

        Destroy(textObject, 1f);
    }
}
