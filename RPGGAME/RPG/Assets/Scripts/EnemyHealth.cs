using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;

    private int currentHealth;
    private EnemyController controller;
    private EnemyHealthBar healthBar;

    void Start()
    {
        currentHealth = maxHealth;
        controller = GetComponent<EnemyController>();
        healthBar = GetComponentInChildren<EnemyHealthBar>();

        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;

        // play hurt SFX
        if (SoundManager.Instance != null)
            SoundManager.Instance.Play("enemy_hurt");

        if (healthBar != null)
            healthBar.SetHealth(currentHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            if (SoundManager.Instance != null)
                SoundManager.Instance.Play("enemy_die");
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
