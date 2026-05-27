using System.Collections;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TurretSlowmo : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform turretRotationPoint;
    [SerializeField] private LayerMask enemyMask;

    [Header("Attributes")]
    [SerializeField] private float targetingRange = 5f;
    [SerializeField] private float aps = 4f; // attacks per second
    [SerializeField] private float freezeTime = 1f;

    [Header("Targeting Filter")]
    [SerializeField] private bool canTargetGround = true;
    [SerializeField] private bool canTargetAir = false;


    private float timeUntilFire;
    void Start()
    {
        
    }
    private void Update()
    {
        timeUntilFire += Time.deltaTime;
        if (timeUntilFire >= 1f / aps)
        {
            Debug.Log("Freeze");
            FreezeEnemies();
            timeUntilFire = 0f;
        }
    }
    private void FreezeEnemies()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, targetingRange, (Vector2)transform.position, 0f, enemyMask);

        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit2D hit = hits[i];

                if (hit.transform.TryGetComponent<Health>(out var health))
                {
                    bool allowed = health.Kind == EnemyKind.Ground ? canTargetGround : canTargetAir;
                    if (!allowed) continue;
                }

                EnemyMovement em = hit.transform.GetComponent<EnemyMovement>();
                if (em == null) continue;

                em.UpdateSpeed(0.5f);
                StartCoroutine(ResetEnemySpeed(em));
            }
        }
    }
    private IEnumerator ResetEnemySpeed(EnemyMovement em)
    {
        yield return new WaitForSeconds(freezeTime);

        em.ResetSpeed();
    }
    private void OnDrawGizmosSelected()
    {
        Handles.color = Color.green;
        Handles.DrawWireDisc(transform.position, transform.forward, targetingRange);
    }
}
