using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;

    private int currentHealth;
    private EnemyController controller;

    void Start()
    {
        currentHealth = maxHealth;
        controller = GetComponent<EnemyController>();
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            controller.OnDeath();
        }
        else
        {
            controller.OnHurt();
        }
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }
}
