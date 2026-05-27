using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] private float bulletSpeed = 5f;
    [SerializeField] private int bulletDamage = 1;
    [SerializeField] private float lifetime = 3f;

    private Vector2 direction = Vector2.up;
    private bool hasHit = false;

    public void Init(Vector2 bulletDirection, int projectileLayer)
    {
        direction = bulletDirection == Vector2.zero ? Vector2.up : bulletDirection.normalized;
        gameObject.layer = projectileLayer;
        ConfigureCollisionLayers(projectileLayer);
        transform.up = direction;
        Destroy(gameObject, lifetime);
    }

    private void ConfigureCollisionLayers(int projectileLayer)
    {
        Physics2D.IgnoreLayerCollision(projectileLayer, 0, true);

        int groundEnemyLayer = LayerMask.NameToLayer("GroundEnemy");
        int flyingEnemyLayer = LayerMask.NameToLayer("FlyingEnemy");
        int groundProjectileLayer = LayerMask.NameToLayer("GroundProjectile");
        int airProjectileLayer = LayerMask.NameToLayer("AirProjectile");

        if (projectileLayer == groundProjectileLayer)
        {
            if (groundEnemyLayer >= 0)
                Physics2D.IgnoreLayerCollision(projectileLayer, groundEnemyLayer, false);
            if (flyingEnemyLayer >= 0)
                Physics2D.IgnoreLayerCollision(projectileLayer, flyingEnemyLayer, true);
        }
        else if (projectileLayer == airProjectileLayer)
        {
            if (groundEnemyLayer >= 0)
                Physics2D.IgnoreLayerCollision(projectileLayer, groundEnemyLayer, true);
            if (flyingEnemyLayer >= 0)
                Physics2D.IgnoreLayerCollision(projectileLayer, flyingEnemyLayer, false);
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = direction * bulletSpeed;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (hasHit) return;

        hasHit = true;

        Health health = other.gameObject.GetComponent<Health>();
        if (health != null)
            health.TakeDamage(bulletDamage);

        Destroy(gameObject);
    }
}
