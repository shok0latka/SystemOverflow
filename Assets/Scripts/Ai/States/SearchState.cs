using UnityEngine;

public class SearchState : EnemyStateBase
{
    private const int SearchPointCount = 4;
    private const float SearchPointArrivalDistance = 0.2f;
    private const float LastKnownRebuildDistance = 0.05f;

    private static readonly Vector2[] ClockwiseDiagonalOffsets =
    {
        new(1f, 1f),
        new(1f, -1f),
        new(-1f, -1f),
        new(-1f, 1f)
    };

    private readonly Vector2[] _searchPoints = new Vector2[SearchPointCount];

    private Vector2 _routeCenter;
    private int _searchPointIndex;
    private bool _hasActiveRoute;

    public SearchState(EnemyContext context, EnemyStateMachine stateMachine)
        : base(context, stateMachine)
    {
    }

    public override EnemyState StateType => EnemyState.Search;

    public override void Enter()
    {
        BuildSearchRoute();
    }

    public override void Exit()
    {
        Context.ClearActiveSearchTarget();
        Context.ClearPath();
    }

    public override void TickUpdate(float deltaTime)
    {
        if (Context.CanSeePlayer)
        {
            Context.ResetReturnTimer();
            StateMachine.TransitionTo(EnemyState.Chase);
            return;
        }

        if (!_hasActiveRoute || HasLastKnownPlayerPositionChanged())
        {
            BuildSearchRoute();
        }

        if (Context.IsNear(GetCurrentSearchPoint(), SearchPointArrivalDistance))
        {
            AdvanceSearchPoint();
        }
    }

    public override void TickFixed(float fixedDeltaTime)
    {
        if (!_hasActiveRoute)
        {
            Context.StopMovement();
            return;
        }

        Vector2 target = GetCurrentSearchPoint();
        if (Context.IsNear(target, SearchPointArrivalDistance))
        {
            AdvanceSearchPoint();
            return;
        }

        if (Context.MoveAlongPathTo(target, Config.patrolSpeed, fixedDeltaTime))
        {
            Context.ResetReturnTimer();
            return;
        }

        Context.ReturnTimer += fixedDeltaTime;
        if (Context.ReturnTimer >= Config.searchPointStuckDuration)
        {
            AdvanceSearchPoint();
        }
    }

    private void BuildSearchRoute()
    {
        _routeCenter = Context.LastKnownPlayerPosition;
        _searchPointIndex = 0;
        _hasActiveRoute = true;

        Context.ClearPath();
        Context.ResetReturnTimer();

        float offset = Mathf.Max(0f, Config.searchPointOffset);
        int startOffsetIndex = FindNearestOffsetIndex(_routeCenter, Context.Position, offset);
        for (int index = 0; index < SearchPointCount; index++)
        {
            Vector2 offsetDirection = ClockwiseDiagonalOffsets[
                (startOffsetIndex + index) % SearchPointCount];
            _searchPoints[index] = _routeCenter + offsetDirection * offset;
        }

        ActivateCurrentSearchPoint();
    }

    private int FindNearestOffsetIndex(Vector2 center, Vector2 origin, float offset)
    {
        int nearestIndex = 0;
        float nearestSqrDistance = float.MaxValue;

        for (int index = 0; index < SearchPointCount; index++)
        {
            Vector2 candidate = center + ClockwiseDiagonalOffsets[index] * offset;
            float sqrDistance = (candidate - origin).sqrMagnitude;
            if (sqrDistance >= nearestSqrDistance)
            {
                continue;
            }

            nearestSqrDistance = sqrDistance;
            nearestIndex = index;
        }

        return nearestIndex;
    }

    private bool HasLastKnownPlayerPositionChanged()
    {
        float rebuildSqrDistance = LastKnownRebuildDistance * LastKnownRebuildDistance;
        return (_routeCenter - Context.LastKnownPlayerPosition).sqrMagnitude > rebuildSqrDistance;
    }

    private Vector2 GetCurrentSearchPoint()
    {
        return _searchPoints[Mathf.Clamp(_searchPointIndex, 0, SearchPointCount - 1)];
    }

    private void ActivateCurrentSearchPoint()
    {
        Vector2 searchPoint = GetCurrentSearchPoint();
        Context.SetActiveSearchTarget(searchPoint);
        Context.UpdateViewDirectionTowards(searchPoint);
    }

    private void AdvanceSearchPoint()
    {
        _searchPointIndex++;
        Context.ClearPath();
        Context.ResetReturnTimer();

        if (_searchPointIndex >= SearchPointCount)
        {
            CompleteSearch();
            return;
        }

        ActivateCurrentSearchPoint();
    }

    private void CompleteSearch()
    {
        _hasActiveRoute = false;
        Context.ClearActiveSearchTarget();
        Context.Suspicion.Reset();
        Context.TimeSinceSeenPlayer = 0f;
        Context.ResetReturnTimer();
        StateMachine.TransitionTo(EnemyState.Patrol);
    }
}
