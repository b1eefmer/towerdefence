using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] private float moveSpeed = 2f;

    private Transform[] path;
    private Transform target;
    private int waypointIndex;
    private int assignedPath;

    private float baseSpeed;

    private BaseHealth baseHealth; 

    void Start()
    {
        baseSpeed = moveSpeed;
        path = LevelMananger.main.GetPath(assignedPath);
        if (path == null || path.Length == 0)
        {
            Debug.LogError($"Enemy has no valid path at index {assignedPath}.");
            enabled = false;
            return;
        }

        target = path[waypointIndex];
        baseHealth = FindObjectOfType<BaseHealth>();
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
                target = path[waypointIndex];
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

    public void UpdateSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    public void ResetSpeed()
    {
        moveSpeed = baseSpeed;
    }
}
