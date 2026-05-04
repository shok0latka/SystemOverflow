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
    private const float MinimumInteractRadius = 0.05f;
    private const float MinimumHackDuration = 0.2f;
    private const float MinimumHackResistance = 0f;
    private const float MaximumHackResistance = 0.95f;
    private const float MinimumAlertRadius = 0f;
    private const float MinimumAlertSignalSpeed = 0.05f;

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3.2f;

    [Header("Perception")]
    [FormerlySerializedAs("detectRadius")]
    public float visionRadius = 6f;
    [Range(1f, 360f)] public float visionConeAngleDegrees = 90f;
    public float loseSightTime = 1.2f;
    public float suspicionGainPerSecond = 0.9f;
    public float suspicionDecayPerSecond = 0.4f;
    [Range(0f, 1f)] public float suspicionThreshold = 0.6f;

    [Header("Behavior")]
    [Range(0f, 1f)] public float aggressiveness = 0.5f;
    public float searchDuration = 2.5f;

    [Header("Interaction")]
    public float interactRadius = 0.75f;

    [Header("Attack")]
    public float attackRadius = 1.2f;
    public float attackCooldown = 0.8f;
    public int attackDamage = 1;

    [Header("Hack")]
    [FormerlySerializedAs("baseHackDuration")]
    public float hackDuration = 40f;
    [Range(0f, 0.95f)] public float hackResistance = 0.2f;

    [Header("Alert")]
    public float alertRadius = 5f;
    public float alertSignalSpeed = 2f;
    [Range(0f, 1f)] public float alertSuspicion = 0.45f;

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
        searchDuration = Mathf.Max(MinimumDuration, searchDuration);
        interactRadius = Mathf.Max(MinimumInteractRadius, interactRadius);

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
    }
}
