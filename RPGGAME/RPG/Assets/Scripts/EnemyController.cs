using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    public enum EnemyState { Idle, Chase, Attack, Hurt, Death, Returning }

    [Header("References")]
    public Transform player;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float returnSpeed = 1.5f;

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float attackRadius = 1.3f;
    [SerializeField] private float losePlayerRadius = 7f;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackDelay = 0.35f;

    [Header("Hurt")]
    [SerializeField] private float knockbackForce = 4f;

    [Header("Spacing")]
    [SerializeField] private float minDistanceToPlayer = 0.8f;

    private EnemyState currentState = EnemyState.Idle;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private EnemyHealth enemyHealth;

    private Vector2 startPosition;
    private float lastAttackTime = -99f;
    private bool isAttacking;
    private bool isDead;

    // 4-direction facing
    private float facingX = 0f;
    private float facingY = -1f; // default facing down

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyHealth = GetComponent<EnemyHealth>();

        startPosition = transform.position;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    void Update()
    {
        if (isDead) return;
        if (player == null) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Idle:
            case EnemyState.Returning:
                HandleIdle(distToPlayer);
                break;
            case EnemyState.Chase:
                HandleChase(distToPlayer);
                break;
            case EnemyState.Attack:
                HandleAttack(distToPlayer);
                break;
            case EnemyState.Hurt:
                // handled by coroutine
                break;
        }
    }

    void FixedUpdate()
    {
        if (isDead || isAttacking) return;

        float distToPlayer = player != null ? Vector2.Distance(transform.position, player.position) : 999f;

        switch (currentState)
        {
            case EnemyState.Returning:
                float distToHome = Vector2.Distance(transform.position, startPosition);
                if (distToHome > 0.3f)
                    MoveTowards(startPosition, returnSpeed);
                else
                {
                    rb.linearVelocity = Vector2.zero;
                    ChangeState(EnemyState.Idle);
                }
                break;
            case EnemyState.Chase:
                // stop before stacking on top of the player
                if (distToPlayer > minDistanceToPlayer)
                    MoveTowards(player.position, moveSpeed);
                else
                    rb.linearVelocity = Vector2.zero;
                break;
            default:
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }

    // --- STATE HANDLERS ---

    void HandleIdle(float distToPlayer)
    {
        // detected player?
        if (distToPlayer <= detectionRadius)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        // if far from home, walk back
        float distToHome = Vector2.Distance(transform.position, startPosition);
        if (currentState != EnemyState.Returning && distToHome > 0.5f)
        {
            ChangeState(EnemyState.Returning);
            FaceTarget(startPosition);
        }
    }

    void HandleChase(float distToPlayer)
    {
        // lost player? go back to idle
        if (distToPlayer > losePlayerRadius)
        {
            ChangeState(EnemyState.Returning);
            return;
        }

        // in attack range?
        if (distToPlayer <= attackRadius)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        FaceTarget(player.position);
    }

    void HandleAttack(float distToPlayer)
    {
        FaceTarget(player.position);

        // player moved out of range? chase again
        if (distToPlayer > attackRadius * 1.5f && !isAttacking)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        if (!isAttacking && Time.time >= lastAttackTime + attackCooldown)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    // --- ATTACK ---

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        animator.SetTrigger("attack");

        // wait for the hit moment in the animation
        yield return new WaitForSeconds(attackDelay);

        // check if still in range when damage applies
        if (!isDead && player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= attackRadius * 1.8f)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);
                }
            }
        }

        // wait for rest of attack anim
        yield return new WaitForSeconds(0.3f);

        lastAttackTime = Time.time;
        isAttacking = false;
    }

    // --- HURT / DEATH (called by EnemyHealth) ---

    public void OnHurt()
    {
        if (isDead) return;
        StartCoroutine(HurtRoutine());
    }

    IEnumerator HurtRoutine()
    {
        ChangeState(EnemyState.Hurt);
        isAttacking = false;

        animator.SetTrigger("hurt");

        // knockback away from player
        if (player != null)
        {
            Vector2 knockDir = ((Vector2)transform.position - (Vector2)player.position).normalized;
            rb.linearVelocity = knockDir * knockbackForce;
        }

        // flash red
        spriteRenderer.color = new Color(1f, 0.3f, 0.3f);
        yield return new WaitForSeconds(0.2f);
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;

        if (!isDead)
            ChangeState(EnemyState.Chase);
    }

    public void OnDeath()
    {
        isDead = true;
        isAttacking = false;
        StopAllCoroutines();
        ChangeState(EnemyState.Death);

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        animator.SetTrigger("die");

        // disable colliders
        foreach (Collider2D col in GetComponents<Collider2D>())
            col.enabled = false;

        Destroy(gameObject, 2f);
    }

    // --- MOVEMENT HELPERS ---

    void MoveTowards(Vector2 target, float speed)
    {
        Vector2 direction = ((Vector2)target - rb.position).normalized;
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, direction * speed, Time.fixedDeltaTime * 10f);
    }

    void FaceTarget(Vector2 target)
    {
        Vector2 dir = ((Vector2)target - (Vector2)transform.position).normalized;

        // snap to 4 directions based on which axis is dominant
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            facingX = dir.x > 0 ? 1f : -1f;
            facingY = 0f;
        }
        else
        {
            facingX = 0f;
            facingY = dir.y > 0 ? 1f : -1f;
        }

        animator.SetFloat("xinput", facingX);
        animator.SetFloat("yinput", facingY);
    }

    void PickNewPatrolPoint()
    {
        // unused — kept for potential future use
    }

    // --- STATE MANAGEMENT ---

    void ChangeState(EnemyState newState)
    {
        currentState = newState;

        bool walking = newState == EnemyState.Chase || newState == EnemyState.Returning;
        animator.SetBool("isMoving", walking);
    }

    // --- GIZMOS ---

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
