using System;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interact")]
    public float interactRadius = 2.2f;
    public LayerMask interactLayer;

    [SerializeField] 
    private float hackHoldDuration = 1.5f;

    private EnemyHackController _hackTarget;
    private float _hackHoldTimer;

    public event Action<EnemyHackController> HackSucceeded;

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
        if (_hackTarget == null)
        {
            TryLockHackTarget();
            if (_hackTarget == null)
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

        _hackTarget.SetAttemptProgress(normalizedProgress);

        if (_hackHoldTimer < hackHoldDuration)
        {
            return;
        }

        HackBeginResult result = _hackTarget.TryBeginHack(HackRequest.Default);
        if (result.Succeeded)
        {
            EnemyHackController hackedTarget = _hackTarget;
            ResetHackAttempt();
            HackSucceeded?.Invoke(hackedTarget);
            return;
        }

        ResetHackAttempt();
    }

    private bool TryLockHackTarget()
    {
        EnemyHackController target = FindNearestHackable();
        if (target == null)
        {
            return false;
        }

        _hackTarget = target;
        _hackHoldTimer = 0f;
        _hackTarget.SetAttemptProgress(0f);
        return true;
    }

    private EnemyHackController FindNearestHackable()
    {
        return FindNearestHackController(status => status.CanBegin);
    }

    private EnemyHackController FindNearestHackController(Func<HackStatusSnapshot, bool> statusPredicate)
    {
        EnemyHackController[] hackables = FindObjectsByType<EnemyHackController>(FindObjectsSortMode.None);
        if (hackables == null || hackables.Length == 0)
        {
            return null;
        }

        EnemyHackController nearestHackable = null;
        float nearestSqrDistance = float.MaxValue;
        Vector2 playerPosition = transform.position;
        float maxSqrDistance = interactRadius * interactRadius;

        foreach (EnemyHackController candidate in hackables)
        {
            if (candidate == null)
            {
                continue;
            }

            HackStatusSnapshot status = candidate.GetHackStatus();
            if (!statusPredicate(status))
            {
                continue;
            }

            float sqrDistance = ((Vector2)candidate.transform.position - playerPosition).sqrMagnitude;
            if (sqrDistance > maxSqrDistance || sqrDistance >= nearestSqrDistance)
            {
                continue;
            }

            nearestHackable = candidate;
            nearestSqrDistance = sqrDistance;
        }

        return nearestHackable;
    }

    private bool IsHackTargetStillValid()
    {
        if (_hackTarget == null)
        {
            return false;
        }

        HackStatusSnapshot status = _hackTarget.GetHackStatus();
        if (!status.CanBegin || status.IsActive)
        {
            return false;
        }

        float sqrDistance = ((Vector2)_hackTarget.transform.position - (Vector2)transform.position).sqrMagnitude;
        return sqrDistance <= interactRadius * interactRadius;
    }

    private void ResetHackAttempt()
    {
        if (_hackTarget != null)
        {
            _hackTarget.ClearAttemptProgress();
        }

        _hackTarget = null;
        _hackHoldTimer = 0f;
    }
}
