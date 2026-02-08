using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHp = 3;
    [SerializeField] private int currentHp = 3;
    [SerializeField] private bool reloadSceneOnDeath = true;

    public int MaxHp => maxHp;
    public int CurrentHp => currentHp;

    private void Awake()
    {
        maxHp = Mathf.Max(1, maxHp);
        currentHp = Mathf.Clamp(currentHp, 1, maxHp);
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || currentHp <= 0)
        {
            return;
        }

        currentHp = Mathf.Max(0, currentHp - amount);

        if (currentHp == 0)
        {
            Die();
        }
    }

    public void SetCurrentHp(int value)
    {
        if (value <= 0)
        {
            currentHp = maxHp;
            return;
        }

        currentHp = Mathf.Clamp(value, 1, maxHp);
    }

    private void Die()
    {
        if (!reloadSceneOnDeath)
        {
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
