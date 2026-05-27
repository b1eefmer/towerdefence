using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] private float bulletSpeed = 5f;
    [SerializeField] private int bulletDamage = 1;
    [Tooltip("Auto-destroy after this many seconds so straight-flying bullets do not live forever.")]
    [SerializeField] private float lifetime = 3f;

    private Transform target;
    private bool hasHit = false;

    private bool damageGround = true;
    private bool damageAir = true;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetTarget(Transform _target)
    {
        target = _target;
    }

    public void Launch(Vector2 direction)
    {
        target = null;
        rb.linearVelocity = direction.normalized * bulletSpeed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    /// <summary>
    /// Configures which enemy kinds this bullet can damage. Called by the turret right after Launch().
    /// </summary>
    public void SetDamageFilter(bool ground, bool air)
    {
        damageGround = ground;
        damageAir = air;
    }

    private void FixedUpdate()
    {
        if (target == null) return;
        Vector2 direction = (target.position - transform.position).normalized;
        rb.linearVelocity = direction * bulletSpeed;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (hasHit) return;
        if (!other.gameObject.TryGetComponent<Health>(out var health)) return;

        if (!CanDamage(health.Kind)) return;

        hasHit = true;
        health.TakeDamage(bulletDamage);
        Destroy(gameObject);
    }

    private bool CanDamage(EnemyKind kind)
    {
        return kind == EnemyKind.Ground ? damageGround : damageAir;
    }
}
