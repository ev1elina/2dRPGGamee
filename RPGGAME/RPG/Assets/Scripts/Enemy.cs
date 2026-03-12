using UnityEngine;
using System.Collections;
public class Enemy : MonoBehaviour
{
    public Transform player;

    public float speed = 2f;
    public float detectionRadius = 6f;
    public float attackRadius = 1.5f;

    public float attackCooldown = 1.5f;
    public int damage = 1;

    float lastAttackTime;

    Animator animator;
    SpriteRenderer sprite;

    void Start()
    {
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        Flip();

       
        if (distance < detectionRadius && distance > attackRadius)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                speed * Time.deltaTime
            );

            animator.SetBool("isWalking", true);
        }

        
        else if (distance <= attackRadius)
        {
            animator.SetBool("isWalking", false);

            if (Time.time > lastAttackTime + attackCooldown)
            {
                Attack();
            }
        }

        
        else
        {
            animator.SetBool("isWalking", false);
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;

        animator.SetTrigger("attack");

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
    }

    void Flip()
    {
        if (player.position.x < transform.position.x)
            sprite.flipX = true;
        else
            sprite.flipX = false;
    }

IEnumerator AttackRoutine()
{
    lastAttackTime = Time.time;

    animator.SetTrigger("attack");

    yield return new WaitForSeconds(0.4f); 

    float distance = Vector2.Distance(transform.position, player.position);

    if (distance <= attackRadius)
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
    }
}
}