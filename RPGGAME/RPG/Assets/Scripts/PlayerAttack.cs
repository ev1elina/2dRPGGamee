using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private int attackDamage = 2;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackOffset = 0.8f;
    [SerializeField] private LayerMask enemyLayer;

    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Called by animation event at the hit frame, OR call from movement script
    public void DealDamage()
    {
        // get facing direction from animator
        float x = anim.GetFloat("xinput");
        float y = anim.GetFloat("yinput");
        Vector2 dir = new Vector2(x, y).normalized;
        if (dir == Vector2.zero) dir = Vector2.down;

        Vector2 hitPos = (Vector2)transform.position + dir * attackOffset;

        // Debug — remove later
        Debug.Log($"DealDamage called! HitPos: {hitPos}, Range: {attackRange}, Layer: {enemyLayer.value}");

        Collider2D[] hits = Physics2D.OverlapCircleAll(hitPos, attackRange, enemyLayer);

        Debug.Log($"Hits found: {hits.Length}");

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);
                Debug.Log($"Damaged: {hit.name}");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (anim == null) anim = GetComponent<Animator>();
        float x = anim != null ? anim.GetFloat("xinput") : 0f;
        float y = anim != null ? anim.GetFloat("yinput") : -1f;
        Vector2 dir = new Vector2(x, y).normalized;
        if (dir == Vector2.zero) dir = Vector2.down;
        Vector2 hitPos = (Vector2)transform.position + dir * attackOffset;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(hitPos, attackRange);
    }
}
