using UnityEngine;

public class movement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    
    private Rigidbody2D rb;
    private Animator anim;
    
    private float inputX;
    private float inputY;
    private float lastX = 0f;
    private float lastY = -1f; 
    private bool isAttacking = false;
    private bool inputLocked = false;
    private bool hasDealtDamage = false;
    private bool attackBuffered = false;
    private float attackTimer = 0f;
    private float attackDamageTime = 0.15f; // deal damage this many seconds into attack
    private float attackDuration = 0.4f; // total attack lock time
    private PlayerAttack playerAttack;
    private bool walkingSfxPlaying = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerAttack = GetComponent<PlayerAttack>();
    }

    void Update()
    {
        if (inputLocked) return;
       
        if (isAttacking)
        {
            attackTimer += Time.deltaTime;

            // deal damage at the right moment
            if (!hasDealtDamage && attackTimer >= attackDamageTime)
            {
                hasDealtDamage = true;
                if (playerAttack != null)
                    playerAttack.DealDamage();
            }

            // end attack after duration
            if (attackTimer >= attackDuration)
            {
                isAttacking = false;

                // if player buffered another attack, fire it immediately
                if (attackBuffered)
                {
                    attackBuffered = false;
                    Attack();
                    return;
                }
            }
        }
        
        
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (!isAttacking)
            {
                Attack();
            }
            else
            {
                // buffer the next attack
                attackBuffered = true;
            }
        }
        
        if (isAttacking) return;
        
        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");

        
        if (inputX != 0 || inputY != 0)
        {
            lastX = inputX;
            lastY = inputY;
        }

       
        anim.SetFloat("xinput", lastX);
        anim.SetFloat("yinput", lastY);
        
        
        bool isMoving = (inputX != 0 || inputY != 0);
        anim.SetBool("isMoving", isMoving);

        // play/stop looping walk SFX
        if (SoundManager.Instance != null)
        {
            if (isMoving && !isAttacking && !inputLocked)
            {
                if (!walkingSfxPlaying)
                {
                    SoundManager.Instance.PlayLoop("walk");
                    walkingSfxPlaying = true;
                }
            }
            else
            {
                if (walkingSfxPlaying)
                {
                    SoundManager.Instance.StopLoop("walk");
                    walkingSfxPlaying = false;
                }
            }
        }
    }
    
    void Attack()
    {
        isAttacking = true;
        hasDealtDamage = false;
        attackBuffered = false;
        attackTimer = 0f;

        // lock in the facing direction for the hitbox BEFORE the animation starts
        if (playerAttack != null)
            playerAttack.SetAttackDirection(lastX, lastY);

        // play attack sound (named clip in SoundManager). tries common keys.
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.Play("player_attack");
            SoundManager.Instance.Play("attack");
        }

        anim.SetTrigger("attack");
        anim.SetBool("isMoving", false);

        // stop movement instantly
        rb.linearVelocity = Vector2.zero;
    }
    
    
    public void AttackEnd()
    {
        isAttacking = false;
    }

    public void SetInputLocked(bool locked)
    {
        inputLocked = locked;
        if (locked)
        {
            inputX = 0;
            inputY = 0;
            anim.SetBool("isMoving", false);
        }
    }

    void FixedUpdate()
    {
        if (isAttacking || inputLocked) return;
        
        Vector2 movement = new Vector2(inputX, inputY).normalized;
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }
}
