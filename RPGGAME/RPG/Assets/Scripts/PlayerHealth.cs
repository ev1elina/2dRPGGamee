using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth;

    public Slider healthBar;
    public Image fillImage;

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(1);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth < 0)
            currentHealth = 0;

        healthBar.value = currentHealth;

        UpdateHealthColor();
    }

    void UpdateHealthColor()
    {
        float healthPercent = (float)currentHealth / maxHealth;

        if (healthPercent > 0.6f)
            fillImage.color = Color.green;
        else if (healthPercent > 0.3f)
            fillImage.color = Color.yellow;
        else
            fillImage.color = Color.red;
    }
}