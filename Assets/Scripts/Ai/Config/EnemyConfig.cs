using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "SystemOverflow/Enemy Config")]
public class EnemyConfig : ScriptableObject
{
    private const float MinimumSpeed = 0f;
    private const float MinimumDuration = 0f;
    private const float MinimumVisionRadius = 0.1f;
    private const float MinimumVisionConeAngleDegrees = 1f;
    private const float MaximumVisionConeAngleDegrees = 360f;
    private const float MinimumAttackRadius = 1f;
    private const float MinimumAttackCooldown = 0.05f;
    private const int MinimumAttackDamage = 0;
    private const int MinimumRank = 1;
    private const int MinimumMaxHp = 1;
    private const float MinimumInteractRadius = 0.05f;
    private const float MinimumHackDuration = 0.2f;
    private const float MinimumHackResistance = 0f;
    private const float MaximumHackResistance = 0.95f;
    private const float MinimumAlertRadius = 0f;
    private const float MinimumAlertSignalSpeed = 0.05f;
    private const float MinimumPathCellSize = 0.1f;
    private const float MinimumPathRefreshInterval = 0.05f;
    private const int MinimumPathMaxSearchNodes = 1024;

    [Header("Movement")]
    [Min(MinimumSpeed)]
    public float patrolSpeed = 2f;
    [Min(MinimumSpeed)]
    public float chaseSpeed = 3.2f;

    [Header("Perception")]
    [FormerlySerializedAs("detectRadius")]
    [Min(MinimumVisionRadius)]
    public float visionRadius = 6f;
    [Range(MinimumVisionConeAngleDegrees, MaximumVisionConeAngleDegrees)]
    public float visionConeAngleDegrees = 90f;
    [Min(MinimumDuration)]
    public float loseSightTime = 1.2f;
    [Min(MinimumDuration)]
    public float suspicionGainPerSecond = 0.9f;
    [Min(MinimumDuration)]
    public float suspicionDecayPerSecond = 0.4f;
    [Range(0f, 1f)] public float suspicionThreshold = 0.6f;

    [Header("Behavior")]
    [Range(0f, 1f)] public float aggressiveness = 0.5f;
    [Min(MinimumDuration)]
    public float searchPointOffset = 1.25f;
    [Min(MinimumDuration)]
    public float searchPointStuckDuration = 1f;

    [Header("Interaction")]
    [Min(MinimumInteractRadius)]
    public float interactRadius = 0.75f;

    [Header("Combat")]
    [Min(MinimumRank)]
    public int rank = 1;
    [Min(MinimumMaxHp)]
    public int maxHp = 3;
    [Min(MinimumAttackRadius)]
    public float attackRadius = 1.2f;
    [Min(MinimumAttackCooldown)]
    public float attackCooldown = 0.8f;
    [Min(MinimumAttackDamage)]
    public int attackDamage = 1;

    [Header("Hack")]
    [FormerlySerializedAs("baseHackDuration")]
    [Min(MinimumHackDuration)]
    public float hackDuration = 40f;
    [Range(MinimumHackResistance, MaximumHackResistance)]
    public float hackResistance = 0.2f;

    [Header("Alert")]
    [Min(MinimumAlertRadius)]
    public float alertRadius = 5f;
    [Min(MinimumAlertSignalSpeed)]
    public float alertSignalSpeed = 2f;
    [Range(0f, 1f)] public float alertSuspicion = 0.45f;

    [Header("Pathfinding")]
    [Min(MinimumPathCellSize)]
    public float pathCellSize = 0.5f;
    [Min(MinimumPathRefreshInterval)]
    public float pathRefreshInterval = 0.2f;
    [Min(MinimumPathMaxSearchNodes)]
    public int pathMaxSearchNodes = 1024;

    private void OnEnable()
    {
        NormalizeValues();
    }

    private void OnValidate()
    {
        NormalizeValues();
    }

    private void NormalizeValues()
    {
        patrolSpeed = Mathf.Max(MinimumSpeed, patrolSpeed);
        chaseSpeed = Mathf.Max(MinimumSpeed, chaseSpeed);

        visionRadius = Mathf.Max(MinimumVisionRadius, visionRadius);
        visionConeAngleDegrees = Mathf.Clamp(
            visionConeAngleDegrees,
            MinimumVisionConeAngleDegrees,
            MaximumVisionConeAngleDegrees);
        loseSightTime = Mathf.Max(MinimumDuration, loseSightTime);
        suspicionGainPerSecond = Mathf.Max(MinimumDuration, suspicionGainPerSecond);
        suspicionDecayPerSecond = Mathf.Max(MinimumDuration, suspicionDecayPerSecond);
        suspicionThreshold = Mathf.Clamp01(suspicionThreshold);

        aggressiveness = Mathf.Clamp01(aggressiveness);
        searchPointOffset = Mathf.Max(MinimumDuration, searchPointOffset);
        searchPointStuckDuration = Mathf.Max(MinimumDuration, searchPointStuckDuration);
        interactRadius = Mathf.Max(MinimumInteractRadius, interactRadius);

        rank = Mathf.Max(MinimumRank, rank);
        maxHp = Mathf.Max(MinimumMaxHp, maxHp);
        attackRadius = Mathf.Max(MinimumAttackRadius, attackRadius);
        attackCooldown = Mathf.Max(MinimumAttackCooldown, attackCooldown);
        attackDamage = Mathf.Max(MinimumAttackDamage, attackDamage);

        hackDuration = Mathf.Max(MinimumHackDuration, hackDuration);
        hackResistance = Mathf.Clamp(
            hackResistance,
            MinimumHackResistance,
            MaximumHackResistance);

        alertRadius = Mathf.Max(MinimumAlertRadius, alertRadius);
        alertSignalSpeed = Mathf.Max(MinimumAlertSignalSpeed, alertSignalSpeed);
        alertSuspicion = Mathf.Clamp01(alertSuspicion);

        pathCellSize = Mathf.Max(MinimumPathCellSize, pathCellSize);
        pathRefreshInterval = Mathf.Max(MinimumPathRefreshInterval, pathRefreshInterval);
        pathMaxSearchNodes = Mathf.Max(MinimumPathMaxSearchNodes, pathMaxSearchNodes);
    }
}
