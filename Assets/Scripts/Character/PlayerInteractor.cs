using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interact")]
    public float interactRadius = 1.2f;
    public LayerMask interactLayer;

    [Header("Hack")]
    public float hackRadius = 1.8f;
    public LayerMask enemyLayer;
    public float hackDurationSeconds = 6f;

    public bool TryHackNearestEnemy()
    {
        EnemyAI2D nearestEnemy = FindNearestEnemyInRadius(hackRadius);
        if (nearestEnemy == null)
        {
            return false;
        }

        return nearestEnemy.TryHack(hackDurationSeconds);
    }

    public void TryInteract()
    {
        Collider2D hit = Physics2D.OverlapCircle(
            transform.position,
            interactRadius,
            interactLayer
        );

        if (hit == null)
        {
            return;
        }

        Interactable interactable = hit.GetComponent<Interactable>();
        if (interactable != null)
        {
            interactable.Interact();
        }
    }

    private EnemyAI2D FindNearestEnemyInRadius(float radius)
    {
        Collider2D[] colliders = enemyLayer.value == 0
            ? Physics2D.OverlapCircleAll(transform.position, radius)
            : Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);

        EnemyAI2D nearest = null;
        float nearestSqrDistance = float.MaxValue;

        foreach (Collider2D collider in colliders)
        {
            EnemyAI2D enemy = collider.GetComponentInParent<EnemyAI2D>();
            if (enemy == null)
            {
                continue;
            }

            float sqrDistance = ((Vector2)enemy.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearest = enemy;
            }
        }

        if (nearest != null)
        {
            return nearest;
        }

        EnemyAI2D[] allEnemies = FindObjectsByType<EnemyAI2D>(FindObjectsSortMode.None);
        float radiusSqr = radius * radius;

        foreach (EnemyAI2D enemy in allEnemies)
        {
            float sqrDistance = ((Vector2)enemy.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (sqrDistance > radiusSqr)
            {
                continue;
            }

            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearest = enemy;
            }
        }

        return nearest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hackRadius);
    }
}
