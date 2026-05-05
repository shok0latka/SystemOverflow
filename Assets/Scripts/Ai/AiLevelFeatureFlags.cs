using UnityEngine;

[DisallowMultipleComponent]
public sealed class AiLevelFeatureFlags : MonoBehaviour
{
    [SerializeField] private bool enemiesCanAttackEnemies = true;
    [SerializeField] private bool enemiesCanCommunicate = true;

    private static AiLevelFeatureFlags _current;

    public static bool EnemiesCanAttackEnemies => Current == null ||
        Current.enemiesCanAttackEnemies;

    public static bool EnemiesCanCommunicate => Current == null ||
        Current.enemiesCanCommunicate;

    private static AiLevelFeatureFlags Current
    {
        get
        {
            if (_current == null)
            {
                _current = FindObjectOfType<AiLevelFeatureFlags>();
            }

            return _current;
        }
    }

    private void OnEnable()
    {
        _current = this;
    }

    private void OnDisable()
    {
        if (_current == this)
        {
            _current = null;
        }
    }
}
