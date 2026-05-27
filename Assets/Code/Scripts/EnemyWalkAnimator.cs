using UnityEngine;

public class EnemyWalkAnimator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float framesPerSecond = 8f;
    [SerializeField] private bool animateOnlyWhenMoving = true;

    private int currentFrame;
    private float timer;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (frames != null && frames.Length > 0 && spriteRenderer != null)
            spriteRenderer.sprite = frames[0];
    }

    private void Update()
    {
        if (spriteRenderer == null || frames == null || frames.Length == 0 || framesPerSecond <= 0f)
            return;

        if (animateOnlyWhenMoving && rb != null && rb.linearVelocity.sqrMagnitude < 0.01f)
            return;

        timer += Time.deltaTime;
        float frameDuration = 1f / framesPerSecond;

        while (timer >= frameDuration)
        {
            timer -= frameDuration;
            currentFrame = (currentFrame + 1) % frames.Length;
            spriteRenderer.sprite = frames[currentFrame];
        }
    }
}
