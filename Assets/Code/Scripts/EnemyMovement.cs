using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Attributes")]
    [SerializeField] private float moveSpeed = 2f;

    private Transform[] path;
    private Transform target;
    private int waypointIndex;
    private int assignedPath;

    private float baseSpeed;
    private Color originalColor;

    private BaseHealth baseHealth;

    void Start()
    {
        baseSpeed = moveSpeed;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        path = LevelMananger.main.GetPath(assignedPath);
        if (path == null || path.Length == 0)
        {
            Debug.LogError($"Enemy has no valid path at index {assignedPath}.");
            enabled = false;
            return;
        }

        SetTarget(path[waypointIndex]);
        baseHealth = FindFirstObjectByType<BaseHealth>();
    }

    void Update()
    {
        if (Vector2.Distance(target.position, transform.position) <= 0.1f)
        {
            waypointIndex++;

            if (waypointIndex == path.Length)
            {
          
                if (baseHealth != null)
                {
                    baseHealth.TakeDamage(1);
                }

                EnemySpawner.onEnemyRemoved.Invoke();
                Destroy(gameObject);
                return;
            }
            else
            {
                SetTarget(path[waypointIndex]);
            }
        }
    }

    private void FixedUpdate()
    {
        if (target == null) return;

        Vector2 direction = (target.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    public void SetPath(int pathIndex)
    {
        assignedPath = pathIndex;
    }

    private void SetTarget(Transform nextTarget)
    {
        target = nextTarget;

        Vector2 direction = (target.position - transform.position).normalized;
        if (direction == Vector2.zero)
            return;

        float angle = Mathf.Atan2(direction.y, Mathf.Abs(direction.x)) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (spriteRenderer != null)
            spriteRenderer.flipX = direction.x < -0.01f;
    }

    public void UpdateSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    public void ResetSpeed()
    {
        moveSpeed = baseSpeed;
    }

    public void SetSlowTint(Color tint)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = tint;
    }

    public void ClearSlowTint()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }
}
