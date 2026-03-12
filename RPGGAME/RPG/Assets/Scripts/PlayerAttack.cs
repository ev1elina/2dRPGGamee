using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private int attackDamage = 2;
    [SerializeField] private float attackRange = 0.7f;
    [SerializeField] private float attackOffset = 0.8f;
    [SerializeField] private LayerMask enemyLayer;

    private Animator anim;
    private Vector2 attackDirection = Vector2.down;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    /// <summary>
    /// Call this right when the attack starts to lock in the facing direction.
    /// </summary>
    public void SetAttackDirection(float x, float y)
    {
        Vector2 dir = new Vector2(x, y).normalized;
        if (dir == Vector2.zero) dir = Vector2.down;
        attackDirection = dir;
    }

    // Called by animation event at the hit frame, OR call from movement script
    public void DealDamage()
    {
        Vector2 hitPos = (Vector2)transform.position + attackDirection * attackOffset;

        // Use direct transform distance instead of OverlapCircle to avoid collider issues
        EnemyHealth[] allEnemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        foreach (EnemyHealth enemy in allEnemies)
        {
            if (enemy == null) continue;
            float dist = Vector2.Distance(hitPos, (Vector2)enemy.transform.position);
            if (dist <= attackRange)
            {
                enemy.TakeDamage(attackDamage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector2 hitPos = (Vector2)transform.position + attackDirection * attackOffset;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(hitPos, attackRange);
    }
}
