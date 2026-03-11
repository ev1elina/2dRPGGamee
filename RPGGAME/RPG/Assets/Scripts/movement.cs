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
    private PlayerAttack playerAttack;

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
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

            // deal damage at 40% through the attack animation
            if (!hasDealtDamage && stateInfo.IsTag("Attack") && stateInfo.normalizedTime >= 0.4f)
            {
                hasDealtDamage = true;
                if (playerAttack != null)
                    playerAttack.DealDamage();
            }

            if (!stateInfo.IsTag("Attack") && stateInfo.normalizedTime >= 0)
            {
                isAttacking = false;
            }
        }
        
        
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (!isAttacking)
            {
                Attack();
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
    }
    
    void Attack()
    {
        isAttacking = true;
        hasDealtDamage = false;
        anim.SetTrigger("attack");
        anim.SetBool("isMoving", false);
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
