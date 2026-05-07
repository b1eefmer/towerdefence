using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] private float moveSpeed = 2f;

    private Transform target;
    private int pathIndex = 0;
    private Transform[] activePath;

    private float baseSpeed;
    private bool stateRestored;

    private BaseHealth baseHealth; 

    void Start()
    {
        if (!stateRestored)
        {
            baseSpeed = moveSpeed;
            activePath = LevelMananger.main.path;
            if (activePath != null && activePath.Length > 0)
            {
                target = activePath[pathIndex];
            }
        }

        
        baseHealth = FindFirstObjectByType<BaseHealth>();
    }

    void Update()
    {
        if (target == null)
        {
            return;
        }

        if (Vector2.Distance(target.position, transform.position) <= 0.1f)
        {
            pathIndex++;

            if (activePath == null || pathIndex == activePath.Length)
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
                target = activePath[pathIndex];
            }
        }
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = (target.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    public void UpdateSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    public void ResetSpeed()
    {
        moveSpeed = baseSpeed;
    }

    public int GetPathIndex()
    {
        return pathIndex;
    }

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    public float GetBaseSpeed()
    {
        return baseSpeed;
    }

    public void RestoreState(int restoredPathIndex, float restoredMoveSpeed, float restoredBaseSpeed)
    {
        if (LevelMananger.main == null || LevelMananger.main.path.Length == 0)
        {
            return;
        }

        activePath = LevelMananger.main.path;
        pathIndex = Mathf.Clamp(restoredPathIndex, 0, LevelMananger.main.path.Length - 1);
        moveSpeed = Mathf.Max(0.01f, restoredMoveSpeed);
        baseSpeed = Mathf.Max(0.01f, restoredBaseSpeed);
        target = activePath[pathIndex];
        stateRestored = true;
    }

    public void SetHardcodedPath(Transform spawnPoint, Transform[] waypoints)
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            return;
        }

        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
        }
        else
        {
            transform.position = waypoints[0].position;
        }

        activePath = waypoints;
        pathIndex = 0;
        target = activePath[pathIndex];
        baseSpeed = moveSpeed;
        stateRestored = true;
    }
}
