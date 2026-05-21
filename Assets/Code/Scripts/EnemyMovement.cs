using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Attributes")]
    [SerializeField] private float moveSpeed = 2f;
    [Tooltip("Sprite art faces right by default. Enable if the art faces left.")]
    [SerializeField] private bool spriteFacesLeftByDefault = false;
    [Tooltip("Minimum horizontal speed required before the sprite flips. Prevents jitter on near-vertical paths.")]
    [SerializeField] private float flipDeadzone = 0.05f;

    private Transform target;
    private int pathIndex = 0;

    private float baseSpeed;

    private BaseHealth baseHealth;

    void Start()
    {
        baseSpeed = moveSpeed;
        target = LevelMananger.main.path[pathIndex];

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        baseHealth = FindObjectOfType<BaseHealth>();
    }

    void Update()
    {
        if (Vector2.Distance(target.position, transform.position) <= 0.1f)
        {
            pathIndex++;

            if (pathIndex == LevelMananger.main.path.Length)
            {
          
                if (baseHealth != null)
                {
                    baseHealth.TakeDamage(1);
                }

                EnemySpawner.onEnemyDestroy.Invoke();
                Destroy(gameObject);
                return;
            }
            else
            {
                target = LevelMananger.main.path[pathIndex];
            }
        }
    }

    private void FixedUpdate()
    {
        Vector2 direction = (target.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        UpdateFacing(direction.x);
    }

    private void UpdateFacing(float horizontalDir)
    {
        if (spriteRenderer == null) return;
        if (Mathf.Abs(horizontalDir) < flipDeadzone) return;

        bool movingLeft = horizontalDir < 0f;
        spriteRenderer.flipX = movingLeft ^ spriteFacesLeftByDefault;
    }

    public void UpdateSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    public void ResetSpeed()
    {
        moveSpeed = baseSpeed;
    }
}