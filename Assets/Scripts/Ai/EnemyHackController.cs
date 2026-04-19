using UnityEngine;

[DisallowMultipleComponent]
public class EnemyHackController : MonoBehaviour
{
    private EnemyAI2D _owner;
    private HackedEnemyDriveIntent _driveIntent;
    private bool _interactRequested;
    private bool _isAcceptingCommands;

    public bool IsAcceptingCommands => _isAcceptingCommands;

    internal HackedEnemyDriveIntent CurrentDriveIntent => _driveIntent;

    public bool TryBeginHack(float baseDuration)
    {
        EnemyAI2D resolvedOwner = ResolveOwner();
        if (resolvedOwner == null)
        {
            return false;
        }

        return resolvedOwner.TryBeginHackInternal(baseDuration);
    }

    public void SetDriveIntent(HackedEnemyDriveIntent intent)
    {
        if (!_isAcceptingCommands)
        {
            return;
        }

        _driveIntent = HackedEnemyDriveIntent.Clamp(intent);
    }

    public void ClearDriveIntent()
    {
        _driveIntent = default;
    }

    public bool RequestInteract()
    {
        if (!_isAcceptingCommands)
        {
            return false;
        }

        _interactRequested = true;
        return true;
    }

    internal bool ConsumeInteractRequest()
    {
        if (!_interactRequested)
        {
            return false;
        }

        _interactRequested = false;
        return true;
    }

    internal void BeginHackControl()
    {
        _isAcceptingCommands = true;
        ClearRuntimeState();
    }

    internal void EndHackControl()
    {
        _isAcceptingCommands = false;
        ClearRuntimeState();
    }

    private void ClearRuntimeState()
    {
        ClearDriveIntent();
        _interactRequested = false;
    }

    internal void BindOwner(EnemyAI2D enemyOwner)
    {
        _owner = enemyOwner;
    }

    private EnemyAI2D ResolveOwner()
    {
        if (_owner != null)
        {
            return _owner;
        }

        _owner = GetComponent<EnemyAI2D>();
        return _owner;
    }
}
