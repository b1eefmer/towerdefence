using UnityEditor;
using UnityEngine;

public static class PhysicsLayerSetup
{
    private const string GroundEnemy = "GroundEnemy";
    private const string FlyingEnemy = "FlyingEnemy";
    private const string GroundProjectile = "GroundProjectile";
    private const string AirProjectile = "AirProjectile";

    [MenuItem("Tools/Tower Defence/Configure Projectile Collision Matrix")]
    private static void ConfigureProjectileCollisionMatrix()
    {
        int groundEnemyLayer = LayerMask.NameToLayer(GroundEnemy);
        int flyingEnemyLayer = LayerMask.NameToLayer(FlyingEnemy);
        int groundProjectileLayer = LayerMask.NameToLayer(GroundProjectile);
        int airProjectileLayer = LayerMask.NameToLayer(AirProjectile);

        if (groundEnemyLayer < 0 ||
            flyingEnemyLayer < 0 ||
            groundProjectileLayer < 0 ||
            airProjectileLayer < 0)
        {
            Debug.LogError("Required tower defence physics layers are missing.");
            return;
        }

        Physics2D.IgnoreLayerCollision(groundProjectileLayer, groundEnemyLayer, false);
        Physics2D.IgnoreLayerCollision(groundProjectileLayer, flyingEnemyLayer, true);
        Physics2D.IgnoreLayerCollision(airProjectileLayer, groundEnemyLayer, true);
        Physics2D.IgnoreLayerCollision(airProjectileLayer, flyingEnemyLayer, false);

        AssetDatabase.SaveAssets();
        Debug.Log("Configured projectile collision matrix for ground and flying enemies.");
    }
}
