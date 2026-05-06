using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] private float moveSpeed = 2f;

    private Transform target;
    private int pathIndex = 0;

    private float baseSpeed;
    private bool stateRestored;

    private BaseHealth baseHealth; 

    void Start()
    {
        if (!stateRestored)
        {
            baseSpeed = moveSpeed;
            target = LevelMananger.main.path[pathIndex];
        }

        
        baseHealth = FindFirstObjectByType<BaseHealth>();
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

        pathIndex = Mathf.Clamp(restoredPathIndex, 0, LevelMananger.main.path.Length - 1);
        moveSpeed = Mathf.Max(0.01f, restoredMoveSpeed);
        baseSpeed = Mathf.Max(0.01f, restoredBaseSpeed);
        target = LevelMananger.main.path[pathIndex];
        stateRestored = true;
    }
}
