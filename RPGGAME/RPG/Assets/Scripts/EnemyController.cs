using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    public enum EnemyState { Idle, Patrol, Chase, Attack, Hurt, Death, Returning, Lurk, Blink }

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
    [SerializeField] private float minDistanceToPlayer = 1.1f;
    [SerializeField] private float separationRadius = 1.2f;
    [SerializeField] private float separationForce = 3f;

    [Header("Patrol")]
    [SerializeField] private float patrolRadius = 4f;
    [SerializeField] private float patrolSpeed = 1.8f;
    [SerializeField] private float patrolPauseMin = 0.3f;
    [SerializeField] private float patrolPauseMax = 1.2f;

    [Header("Vampire Abilities")]
    [SerializeField] private float blinkChance = 0.05f;       // chance to blink instead of normal patrol
    [SerializeField] private float lurkChance = 0.07f;        // chance to lurk
    [SerializeField] private float blinkDistance = 2f;
    [SerializeField] private float blinkDuration = 0.4f;
    [SerializeField] private float lurkDuration = 2f;         // how long to lurk
    [SerializeField] private float idleGhostAlpha = 0.7f;     // transparency when patrolling
    [SerializeField] private float ghostPulseSpeed = 1.5f;    // how fast alpha pulses

    private EnemyState currentState = EnemyState.Idle;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private EnemyHealth enemyHealth;

    private Vector2 startPosition;
    private float lastAttackTime = -99f;
    private bool isAttacking;
    private bool isDead;
    private Coroutine activeRoutine;

    // patrol
    private Vector2 patrolTarget;
    private float patrolPauseTimer;
    private bool hasPatrolTarget;

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

        // prevent enemy and player colliders from pushing each other
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (playerLayer >= 0 && enemyLayer >= 0)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        // start patrolling
        patrolPauseTimer = Random.Range(0.5f, 2f);
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
            case EnemyState.Patrol:
                HandlePatrol(distToPlayer);
                break;
            case EnemyState.Chase:
                HandleChase(distToPlayer);
                break;
            case EnemyState.Attack:
                HandleAttack(distToPlayer);
                break;
            case EnemyState.Hurt:
            case EnemyState.Lurk:
            case EnemyState.Blink:
                // handled by coroutines — but still check for player detection
                if (distToPlayer <= detectionRadius && currentState != EnemyState.Hurt)
                {
                    if (activeRoutine != null)
                    {
                        StopCoroutine(activeRoutine);
                        activeRoutine = null;
                    }
                    Color c2 = spriteRenderer.color;
                    c2.a = 1f;
                    spriteRenderer.color = c2;
                    ChangeState(EnemyState.Chase);
                }
                break;
        }

        // ghostly alpha pulse when not aggro'd
        UpdateGhostEffect();
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
            case EnemyState.Patrol:
                if (hasPatrolTarget)
                {
                    float distToTarget = Vector2.Distance(transform.position, patrolTarget);
                    if (distToTarget > 0.2f)
                        MoveTowards(patrolTarget, patrolSpeed);
                    else
                    {
                        rb.linearVelocity = Vector2.zero;
                        hasPatrolTarget = false;
                        patrolPauseTimer = Random.Range(patrolPauseMin, patrolPauseMax);
                        ChangeState(EnemyState.Idle);
                    }
                }
                break;
            case EnemyState.Chase:
                if (distToPlayer > minDistanceToPlayer)
                {
                    // slow down smoothly as enemy approaches player
                    float approachFactor = Mathf.Clamp01((distToPlayer - minDistanceToPlayer) / 1.5f);
                    float speed = Mathf.Lerp(moveSpeed * 0.2f, moveSpeed, approachFactor);
                    MoveTowards(player.position, speed);
                }
                else if (distToPlayer < minDistanceToPlayer * 0.7f)
                {
                    // too close — soft push back
                    Vector2 away = ((Vector2)transform.position - (Vector2)player.position).normalized;
                    rb.linearVelocity = away * moveSpeed * 0.5f;
                }
                else
                {
                    rb.linearVelocity = Vector2.zero;
                }
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
            Color c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
            ChangeState(EnemyState.Chase);
            return;
        }

        if (currentState == EnemyState.Returning) return;

        // if far from home, walk back
        float distToHome = Vector2.Distance(transform.position, startPosition);
        if (distToHome > patrolRadius * 1.5f)
        {
            ChangeState(EnemyState.Returning);
            FaceTarget(startPosition);
            return;
        }

        // count down pause, then pick a new patrol action
        patrolPauseTimer -= Time.deltaTime;
        if (patrolPauseTimer <= 0f)
        {
            float roll = Random.value;
            if (roll < blinkChance)
            {
                activeRoutine = StartCoroutine(BlinkRoutine());
            }
            else if (roll < blinkChance + lurkChance)
            {
                activeRoutine = StartCoroutine(LurkRoutine());
            }
            else
            {
                // normal patrol walk — this is the most common
                PickNewPatrolPoint();
                FaceTarget(patrolTarget);
                ChangeState(EnemyState.Patrol);
            }
        }
    }

    void HandlePatrol(float distToPlayer)
    {
        // detected player while patrolling?
        if (distToPlayer <= detectionRadius)
        {
            Color c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
            hasPatrolTarget = false;
            ChangeState(EnemyState.Chase);
            return;
        }

        // keep facing the patrol target while walking
        if (hasPatrolTarget)
            FaceTarget(patrolTarget);
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
            activeRoutine = StartCoroutine(AttackRoutine());
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
        activeRoutine = null;
    }

    // --- HURT / DEATH (called by EnemyHealth) ---

    public void OnHurt()
    {
        if (isDead) return;

        // cancel any running attack/hurt routine cleanly
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }
        isAttacking = false;

        activeRoutine = StartCoroutine(HurtRoutine());
    }

    IEnumerator HurtRoutine()
    {
        ChangeState(EnemyState.Hurt);

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

        activeRoutine = null;

        if (!isDead)
        {
            // go straight to Attack if player is still nearby, skip Chase loop
            float dist = player != null ? Vector2.Distance(transform.position, player.position) : 999f;
            if (dist <= attackRadius * 1.5f)
                ChangeState(EnemyState.Attack);
            else
                ChangeState(EnemyState.Chase);
        }
    }

    public void OnDeath()
    {
        isDead = true;
        isAttacking = false;
        StopAllCoroutines();
        currentState = EnemyState.Death;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        // disable colliders
        foreach (Collider2D col in GetComponents<Collider2D>())
            col.enabled = false;

        // make fully visible for death
        spriteRenderer.color = Color.white;

        // fade health bar smoothly (add CanvasGroup if missing)
        Canvas healthCanvas = GetComponentInChildren<Canvas>();
        if (healthCanvas != null)
        {
            CanvasGroup cg = healthCanvas.GetComponent<CanvasGroup>();
            if (cg == null) cg = healthCanvas.gameObject.AddComponent<CanvasGroup>();
            StartCoroutine(FadeCanvasGroup(cg, 0.6f));
        }

        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        // force animator out of any blend tree state
        animator.SetBool("isMoving", false);
        animator.ResetTrigger("attack");
        animator.ResetTrigger("hurt");
        
        // small delay to let animator settle, then trigger death
        yield return null;
        animator.SetTrigger("die");

        // wait for death animation to start
        yield return null;
        yield return null;

        // wait for death animation to finish
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float waitTime = stateInfo.length > 0 ? stateInfo.length : 1.5f;
        yield return new WaitForSeconds(waitTime);

        // fade out then destroy
        float fadeDur = 0.5f;
        float elapsed = 0f;
        Color col = spriteRenderer.color;
        while (elapsed < fadeDur)
        {
            elapsed += Time.deltaTime;
            col.a = 1f - (elapsed / fadeDur);
            spriteRenderer.color = col;
            yield return null;
        }

        Destroy(gameObject);
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float duration)
    {
        float start = cg.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, 0f, elapsed / duration);
            yield return null;
        }
        cg.alpha = 0f;
    }

    // --- MOVEMENT HELPERS ---

    void MoveTowards(Vector2 target, float speed)
    {
        Vector2 direction = ((Vector2)target - rb.position).normalized;
        Vector2 separation = GetSeparationForce();
        Vector2 desired = direction * speed + separation;
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, desired, Time.fixedDeltaTime * 10f);
    }

    Vector2 GetSeparationForce()
    {
        Vector2 force = Vector2.zero;
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, separationRadius, LayerMask.GetMask("Enemy"));
        foreach (Collider2D col in nearby)
        {
            if (col.gameObject == gameObject) continue;
            Vector2 away = (Vector2)transform.position - (Vector2)col.transform.position;
            float dist = away.magnitude;
            if (dist > 0f && dist < separationRadius)
                force += away.normalized * (separationRadius - dist) / separationRadius;
        }
        return force * separationForce;
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
        Vector2 offset = Random.insideUnitCircle * patrolRadius;
        patrolTarget = startPosition + offset;
        hasPatrolTarget = true;
    }

    // --- VAMPIRE ABILITIES ---

    IEnumerator BlinkRoutine()
    {
        ChangeState(EnemyState.Blink);
        rb.linearVelocity = Vector2.zero;

        // fade out
        float elapsed = 0f;
        float halfDur = blinkDuration * 0.5f;
        Color col = spriteRenderer.color;
        while (elapsed < halfDur)
        {
            elapsed += Time.deltaTime;
            col.a = Mathf.Lerp(idleGhostAlpha, 0f, elapsed / halfDur);
            spriteRenderer.color = col;
            yield return null;
        }

        // teleport to new spot near home
        Vector2 blinkTarget = startPosition + Random.insideUnitCircle * blinkDistance;
        transform.position = (Vector3)blinkTarget;

        // fade back in
        elapsed = 0f;
        while (elapsed < halfDur)
        {
            elapsed += Time.deltaTime;
            col.a = Mathf.Lerp(0f, idleGhostAlpha, elapsed / halfDur);
            spriteRenderer.color = col;
            yield return null;
        }

        col.a = idleGhostAlpha;
        spriteRenderer.color = col;

        activeRoutine = null;
        patrolPauseTimer = Random.Range(patrolPauseMin, patrolPauseMax);
        ChangeState(EnemyState.Idle);
    }

    IEnumerator LurkRoutine()
    {
        ChangeState(EnemyState.Lurk);
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("isMoving", false);

        // look in random directions menacingly
        float elapsed = 0f;
        float nextLookTime = 0f;
        while (elapsed < lurkDuration)
        {
            elapsed += Time.deltaTime;

            // check for player during lurk
            if (player != null)
            {
                float dist = Vector2.Distance(transform.position, player.position);
                if (dist <= detectionRadius)
                {
                    Color c = spriteRenderer.color;
                    c.a = 1f;
                    spriteRenderer.color = c;
                    activeRoutine = null;
                    ChangeState(EnemyState.Chase);
                    yield break;
                }
            }

            if (elapsed >= nextLookTime)
            {
                // snap to a random 4-direction
                int dir = Random.Range(0, 4);
                switch (dir)
                {
                    case 0: facingX = 0f;  facingY = 1f;  break; // up
                    case 1: facingX = 0f;  facingY = -1f; break; // down
                    case 2: facingX = -1f; facingY = 0f;  break; // left
                    case 3: facingX = 1f;  facingY = 0f;  break; // right
                }
                animator.SetFloat("xinput", facingX);
                animator.SetFloat("yinput", facingY);
                nextLookTime = elapsed + Random.Range(0.4f, 0.8f);
            }

            yield return null;
        }

        activeRoutine = null;
        patrolPauseTimer = Random.Range(patrolPauseMin, patrolPauseMax);
        ChangeState(EnemyState.Idle);
    }

    void UpdateGhostEffect()
    {
        bool isAggro = currentState == EnemyState.Chase || currentState == EnemyState.Attack;
        Color col = spriteRenderer.color;

        if (isAggro)
        {
            // fully visible when fighting
            col.a = 1f;
        }
        else if (currentState != EnemyState.Blink && currentState != EnemyState.Death && currentState != EnemyState.Hurt)
        {
            // ghostly pulse when passive
            float pulse = Mathf.Sin(Time.time * ghostPulseSpeed) * 0.1f;
            col.a = idleGhostAlpha + pulse;
        }

        spriteRenderer.color = col;
    }

    // --- STATE MANAGEMENT ---

    void ChangeState(EnemyState newState)
    {
        currentState = newState;

        bool walking = newState == EnemyState.Chase || newState == EnemyState.Returning || newState == EnemyState.Patrol;
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
