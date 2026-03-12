using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    [SerializeField] private int healAmount = 1;
    [SerializeField] private bool destroyOnPickup = true;

    [Header("VFX / SFX")]
    [SerializeField] private ParticleSystem pickupParticlesPrefab = null;
    [SerializeField] private AudioClip pickupSfx = null;

    void Awake()
    {
        // Ensure a glow script is present so potions always pulse
        if (GetComponent<PickupGlow>() == null)
            gameObject.AddComponent<PickupGlow>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth player = other.GetComponent<PlayerHealth>();
        Inventory inv = other.GetComponent<Inventory>();

        // if player has an inventory, add potions to it; otherwise heal immediately
        if (inv != null)
        {
            inv.AddPotion(1);
        }
        else if (player != null)
        {
            player.Heal(healAmount);
        }

        // play SFX via SoundManager if available, fallback to local clip
        if (SoundManager.Instance != null)
            SoundManager.Instance.Play("pickup", 1f);
        else if (pickupSfx != null)
            AudioSource.PlayClipAtPoint(pickupSfx, transform.position);

        // play VFX and attach to player so it follows
        SpawnPickupVFX(other.transform);

        if (destroyOnPickup)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
        
    }

    void SpawnPickupVFX(Transform followTarget)
    {
        if (pickupParticlesPrefab != null)
        {
            ParticleSystem ps = Instantiate(pickupParticlesPrefab, followTarget != null ? followTarget.position : transform.position, Quaternion.identity);
            if (followTarget != null)
            {
                ps.transform.SetParent(followTarget, worldPositionStays: true);
                var mainMod = ps.main;
                mainMod.simulationSpace = ParticleSystemSimulationSpace.Local;
            }
            ps.Play();
            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax + 0.1f);
            return;
        }

        // create a simple one-shot particle system at runtime
        GameObject go = new GameObject("PickupVFX");
        go.transform.position = followTarget != null ? followTarget.position : transform.position;
        if (followTarget != null) go.transform.SetParent(followTarget, worldPositionStays: true);
        var psComp = go.AddComponent<ParticleSystem>();
        var main = psComp.main;
        main.duration = 0.6f;
        main.startLifetime = 0.5f;
        main.startSpeed = 1.2f;
        main.startSize = 0.12f;
        main.loop = false;
        main.playOnAwake = false;
        main.maxParticles = 40;
        if (followTarget != null)
        {
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
        }

        var em = psComp.emission;
        em.rateOverTime = 0;
        em.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 10, 16, 1, 0f) });

        var shape = psComp.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.1f;

        var col = psComp.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(0.6f, 1f, 0.6f), 0f), new GradientColorKey(new Color(0.2f, 0.8f, 0.2f), 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = grad;

        psComp.Play();
        Destroy(go, main.duration + main.startLifetime.constantMax + 0.2f);
    }
}
