using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "SystemOverflow/Enemy Config")]
public class EnemyConfig : ScriptableObject
{
    [Header("Movement")]
    [Min(0f)]
    public float patrolSpeed = 2f;
    [Min(0f)]
    public float chaseSpeed = 3.2f;

    [Header("Perception")]
    [FormerlySerializedAs("detectRadius")]
    [Min(0.1f)]
    public float visionRadius = 6f;
    [Range(1f, 360f)]
    public float visionConeAngleDegrees = 90f;
    [Min(0f)]
    public float facePlayerTurnSpeedDegreesPerSecond = 540f;
    [Min(0f)]
    public float loseSightTime = 1.2f;
    [Min(0f)]
    public float suspicionGainPerSecond = 0.9f;
    [Min(0f)]
    public float suspicionDecayPerSecond = 0.4f;
    [Range(0f, 1f)] public float suspicionThreshold = 0.6f;

    [Header("Behavior")]
    [Range(0f, 1f)] public float aggressiveness = 0.5f;
    [Min(0f)]
    public float searchPointOffset = 1.25f;
    [Min(0f)]
    public float searchPointStuckDuration = 1f;

    [Header("Interaction")]
    [Min(0.05f)]
    public float interactRadius = 0.75f;

    [Header("Combat")]
    [Min(1)]
    public int rank = 1;
    [Min(1)]
    public int maxHp = 3;
    [Min(1f)]
    public float attackRadius = 1.2f;
    [Min(0.05f)]
    public float attackCooldown = 0.8f;
    [Min(0)]
    public int attackDamage = 1;

    [Header("Hack")]
    [FormerlySerializedAs("baseHackDuration")]
    [Min(0.2f)]
    public float hackDuration = 40f;
    [Range(0f, 0.95f)]
    public float hackResistance = 0.2f;

    [Header("Alert")]
    [Min(0f)]
    public float alertRadius = 5f;
    [Min(0.05f)]
    public float alertSignalSpeed = 2f;
    [Range(0f, 1f)] public float alertSuspicion = 0.45f;

    [Header("Pathfinding")]
    [Min(0.1f)]
    public float pathCellSize = 0.5f;
    [Min(0.05f)]
    public float pathRefreshInterval = 0.2f;
    [Min(1024)]
    public int pathMaxSearchNodes = 1024;
}
