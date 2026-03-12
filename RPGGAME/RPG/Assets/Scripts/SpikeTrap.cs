using UnityEngine;
using UnityEngine.Tilemaps;

public class SpikeTrap : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float damageCooldown = 1f;

    private float lastDamageTime = -99f;
    private Tilemap tilemap;

    void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time < lastDamageTime + damageCooldown) return;

        // If we have a Tilemap, ensure there's actually a tile under the player
        if (tilemap != null)
        {
            Vector3Int cell = tilemap.WorldToCell(other.transform.position);
            TileBase tile = tilemap.GetTile(cell);
            if (tile == null) return; // not standing on a spike tile
        }

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
            lastDamageTime = Time.time;
        }
    }
}
