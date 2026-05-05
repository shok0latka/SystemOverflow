using UnityEngine;

[DisallowMultipleComponent]
public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHp = 3;
    [SerializeField] private int currentHp = 3;

    private EnemyAI2D _owner;

    public int MaxHp => maxHp;
    public int CurrentHp => currentHp;
    public bool IsAlive => currentHp > 0;

    private void Awake()
    {
        _owner = GetComponent<EnemyAI2D>();
        maxHp = Mathf.Max(1, maxHp);
        currentHp = Mathf.Clamp(currentHp, 1, maxHp);
    }

    public void Configure(EnemyAI2D owner, int configuredMaxHp, bool resetCurrentHp)
    {
        _owner = owner != null ? owner : GetComponent<EnemyAI2D>();
        maxHp = Mathf.Max(1, configuredMaxHp);
        if (resetCurrentHp)
        {
            currentHp = maxHp;
            return;
        }

        currentHp = currentHp > 0
            ? Mathf.Clamp(currentHp, 1, maxHp)
            : 0;
    }

    public bool TakeDamage(int amount, EnemyAI2D attacker)
    {
        if (attacker != null && !AiLevelFeatureFlags.EnemiesCanAttackEnemies)
        {
            return false;
        }

        if (amount <= 0 || !IsAlive)
        {
            return false;
        }

        currentHp = Mathf.Max(0, currentHp - amount);
        if (currentHp <= 0)
        {
            Die();
            return true;
        }

        _owner?.HandleRobotAttacked(attacker);
        return true;
    }

    private void Die()
    {
        _owner?.HandleRobotDestroyed();
        Destroy(gameObject);
    }
}
