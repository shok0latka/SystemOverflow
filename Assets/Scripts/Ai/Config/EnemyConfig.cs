using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "SystemOverflow/Enemy Config")]
public class EnemyConfig : ScriptableObject
{
    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3.2f;

    [Header("Perception")]
    public float detectRadius = 6f;
    public float loseSightTime = 1.2f;
    public float suspicionGainPerSecond = 0.9f;
    public float suspicionDecayPerSecond = 0.4f;
    [Range(0f, 1f)] public float suspicionThreshold = 0.6f;

    [Header("Behavior")]
    [Range(0f, 1f)] public float aggressiveness = 0.5f;
    public float searchDuration = 2.5f;

    [Header("Attack")]
    public float attackRadius = 1.2f;
    public float attackCooldown = 0.8f;
    public int attackDamage = 1;

    [Header("Hack")]
    public float baseHackDuration = 6f;
    [Range(0f, 0.95f)] public float hackResistance = 0.2f;
}
