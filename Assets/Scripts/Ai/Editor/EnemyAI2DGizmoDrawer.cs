using UnityEditor;
using UnityEngine;

internal static class EnemyAI2DGizmoDrawer
{
    private const string EnemyConfigFieldName = "enemyConfig";
    private const string PatrolPointsFieldName = "patrolPoints";
    private const string AttackRadiusFieldName = "attackRadius";
    private const float PointRadius = 0.15f;
    private const float FirstPointScale = 1.35f;
    private const float MinimumVisibleRadius = 0.01f;

    private static readonly Color _attackRadiusColor = new Color(1f, 0.25f, 0.25f, 0.95f);
    private static readonly Color _patrolPointColor = new Color(0.2f, 0.85f, 1f, 0.95f);
    private static readonly Color _patrolPathColor = new Color(0.2f, 1f, 0.45f, 0.95f);
    private static readonly Color _firstPatrolPointColor = new Color(1f, 0.95f, 0.25f, 0.95f);

    [DrawGizmo(GizmoType.Selected)]
    private static void DrawSelectedGizmos(EnemyAI2D enemy, GizmoType gizmoType)
    {
        if (enemy == null)
        {
            return;
        }

        SerializedObject enemyObject = new SerializedObject(enemy);
        DrawAttackRadius(enemy.transform, enemyObject);
        DrawPatrolPath(enemyObject);
    }

    private static void DrawAttackRadius(Transform enemyTransform, SerializedObject enemyObject)
    {
        SerializedProperty configProperty = enemyObject.FindProperty(EnemyConfigFieldName);
        Object configReference = configProperty != null ? configProperty.objectReferenceValue : null;
        if (configReference == null)
        {
            return;
        }

        SerializedObject configObject = new SerializedObject(configReference);
        SerializedProperty attackRadiusProperty = configObject.FindProperty(AttackRadiusFieldName);
        if (attackRadiusProperty == null)
        {
            return;
        }

        float attackRadius = Mathf.Max(0f, attackRadiusProperty.floatValue);
        if (attackRadius <= MinimumVisibleRadius)
        {
            return;
        }

        Gizmos.color = _attackRadiusColor;
        Gizmos.DrawWireSphere(enemyTransform.position, attackRadius);
    }

    private static void DrawPatrolPath(SerializedObject enemyObject)
    {
        SerializedProperty patrolPointsProperty = enemyObject.FindProperty(PatrolPointsFieldName);
        if (patrolPointsProperty == null || !patrolPointsProperty.isArray || patrolPointsProperty.arraySize == 0)
        {
            return;
        }

        Transform firstPoint = null;
        Transform previousPoint = null;

        for (int index = 0; index < patrolPointsProperty.arraySize; index++)
        {
            SerializedProperty pointProperty = patrolPointsProperty.GetArrayElementAtIndex(index);
            Transform patrolPoint = pointProperty != null ? pointProperty.objectReferenceValue as Transform : null;
            if (patrolPoint == null)
            {
                continue;
            }

            if (firstPoint == null)
            {
                firstPoint = patrolPoint;
            }

            Gizmos.color = _patrolPointColor;
            Gizmos.DrawWireSphere(patrolPoint.position, PointRadius);

            if (previousPoint != null)
            {
                Gizmos.color = _patrolPathColor;
                Gizmos.DrawLine(previousPoint.position, patrolPoint.position);
            }

            previousPoint = patrolPoint;
        }

        if (firstPoint == null)
        {
            return;
        }

        Gizmos.color = _firstPatrolPointColor;
        Gizmos.DrawWireSphere(firstPoint.position, PointRadius * FirstPointScale);

        if (previousPoint != null && previousPoint != firstPoint)
        {
            Gizmos.color = _patrolPathColor;
            Gizmos.DrawLine(previousPoint.position, firstPoint.position);
        }
    }
}
