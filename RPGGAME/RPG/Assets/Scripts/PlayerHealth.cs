using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth;

    private Animator anim;
    private movement playerMovement;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private bool isDead;
    private bool isInvincible;

    [SerializeField] private float invincibilityTime = 0.5f;
    [SerializeField] private float knockbackForce = 3f;

    void Start()
    {
        currentHealth = maxHealth;

        anim = GetComponent<Animator>();
        playerMovement = GetComponent<movement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            StartCoroutine(HurtRoutine());
        }
    }

    IEnumerator HurtRoutine()
    {
        isInvincible = true;

        if (playerMovement != null)
            playerMovement.SetInputLocked(true);

        anim.SetTrigger("hurt");

        // knockback away from nearest enemy
        GameObject nearestEnemy = FindNearestEnemy();
        if (nearestEnemy != null && rb != null)
        {
            Vector2 knockDir = ((Vector2)transform.position - (Vector2)nearestEnemy.transform.position).normalized;
            rb.linearVelocity = knockDir * knockbackForce;
        }

        // flash effect
        spriteRenderer.color = new Color(1f, 0.3f, 0.3f);
        yield return new WaitForSeconds(0.2f);
        if (rb != null) rb.linearVelocity = Vector2.zero;
        spriteRenderer.color = Color.white;

        if (playerMovement != null)
            playerMovement.SetInputLocked(false);

        // brief invincibility after hurt (prevents spam damage)
        yield return new WaitForSeconds(invincibilityTime);
        isInvincible = false;
    }

    GameObject FindNearestEnemy()
    {
        float closest = float.MaxValue;
        GameObject result = null;
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < closest)
            {
                closest = dist;
                result = enemy;
            }
        }
        return result;
    }

    void Die()
    {
        isDead = true;

        if (playerMovement != null)
            playerMovement.SetInputLocked(true);

        anim.SetTrigger("die");
        anim.SetBool("isMoving", false);

        if (rb != null) rb.linearVelocity = Vector2.zero;

        foreach (Collider2D col in GetComponents<Collider2D>())
            col.enabled = false;
    }

    public bool IsDead()
    {
        return isDead;
    }
}