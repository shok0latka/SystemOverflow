using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interact")]
    public float interactRadius = 2.2f;
    public LayerMask interactLayer;

    [SerializeField] 
    private float hackHoldDuration = 1.5f;

    private EnemyAI2D _hackTargetEnemy;
    private float _hackHoldTimer;

    public void HandleUseInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            BeginUseAttempt();
        }

        if (Input.GetKey(KeyCode.E))
        {
            ContinueUseAttempt();
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            ResetHackAttempt();
        }
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

        Interactable interactable = hit.GetComponentInParent<Interactable>();
        if (interactable != null)
        {
            interactable.Interact();
        }
    }

    private void OnDisable()
    {
        ResetHackAttempt();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }

    private void BeginUseAttempt()
    {
        if (TryLockHackTarget())
        {
            return;
        }

        TryInteract();
    }

    private void ContinueUseAttempt()
    {
        if (_hackTargetEnemy == null)
        {
            TryLockHackTarget();
            if (_hackTargetEnemy == null)
            {
                return;
            }
        }

        if (!IsHackTargetStillValid())
        {
            ResetHackAttempt();
            return;
        }

        _hackHoldTimer += Time.deltaTime;
        float normalizedProgress = hackHoldDuration <= 0f
            ? 1f
            : Mathf.Clamp01(_hackHoldTimer / hackHoldDuration);

        _hackTargetEnemy.ShowHackProgress(normalizedProgress);

        if (_hackHoldTimer < hackHoldDuration)
        {
            return;
        }

        if (_hackTargetEnemy.TryBeginHack(0f))
        {
            ResetHackAttempt();
            return;
        }

        ResetHackAttempt();
    }

    private bool TryLockHackTarget()
    {
        EnemyAI2D targetEnemy = FindNearestEnemy();
        if (targetEnemy == null)
        {
            return false;
        }

        _hackTargetEnemy = targetEnemy;
        _hackHoldTimer = 0f;
        _hackTargetEnemy.ShowHackProgress(0f);
        return true;
    }

    private EnemyAI2D FindNearestEnemy()
    {
        EnemyAI2D[] enemies = FindObjectsByType<EnemyAI2D>(FindObjectsSortMode.None);
        if (enemies == null || enemies.Length == 0)
        {
            return null;
        }

        EnemyAI2D nearestEnemy = null;
        float nearestSqrDistance = float.MaxValue;
        Vector2 playerPosition = transform.position;
        float maxSqrDistance = interactRadius * interactRadius;

        foreach (EnemyAI2D candidate in enemies)
        {
            if (candidate == null || !candidate.CanBeHacked)
            {
                continue;
            }

            float sqrDistance = ((Vector2)candidate.transform.position - playerPosition).sqrMagnitude;
            if (sqrDistance > maxSqrDistance || sqrDistance >= nearestSqrDistance)
            {
                continue;
            }

            nearestEnemy = candidate;
            nearestSqrDistance = sqrDistance;
        }

        return nearestEnemy;
    }

    private bool IsHackTargetStillValid()
    {
        if (_hackTargetEnemy == null)
        {
            return false;
        }

        if (!_hackTargetEnemy.CanBeHacked || _hackTargetEnemy.IsHackActive)
        {
            return false;
        }

        float sqrDistance = ((Vector2)_hackTargetEnemy.transform.position - (Vector2)transform.position).sqrMagnitude;
        return sqrDistance <= interactRadius * interactRadius;
    }

    private void ResetHackAttempt()
    {
        if (_hackTargetEnemy != null)
        {
            _hackTargetEnemy.HideHackProgress();
        }

        _hackTargetEnemy = null;
        _hackHoldTimer = 0f;
    }
}
