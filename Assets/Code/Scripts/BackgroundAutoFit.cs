using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundAutoFit : MonoBehaviour
{
    private SpriteRenderer sr;
    
    [Header("Налаштування масштабу")]
    [Tooltip("Зменшити або збільшити картинку. 0.8 = -20% розміру")]
    [Range(0.1f, 2f)]
    public float scaleMultiplier = 0.8f;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (sr == null || sr.sprite == null) return;

        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic) return;

        // Розміри камери
        float screenHeight = cam.orthographicSize * 2f;
        float screenWidth = screenHeight * cam.aspect;

        // Оригінальні розміри спрайту без урахування поточного масштабу
        float spriteWidth = sr.sprite.rect.width / sr.sprite.pixelsPerUnit;
        float spriteHeight = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;

        // Завжди підганяємо масштаб так, щоб краї картинки ідеально торкалися країв екрану
        // І застосовуємо множник (scaleMultiplier), який зараз зменшує розмір на 20%
        float targetScaleX = (screenWidth / spriteWidth) * scaleMultiplier;
        float targetScaleY = (screenHeight / spriteHeight) * scaleMultiplier;
        
        transform.localScale = new Vector3(targetScaleX, targetScaleY, 1f);
        
        // Завжди тримаємо бекграунд ідеально по центру камери
        transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 10f);
    }
}
