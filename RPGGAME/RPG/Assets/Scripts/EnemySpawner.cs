using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnDelay = 0.5f; // delay between each enemy spawn
    [SerializeField] private bool spawnOnce = true;

    [Header("Spawn Effect")]
    [SerializeField] private float fadeInDuration = 0.6f;
    [SerializeField] private float scalePopDuration = 0.3f;

    private bool hasSpawned;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasSpawned && spawnOnce) return;
        if (!other.CompareTag("Player")) return;

        hasSpawned = true;
        StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        foreach (Transform point in spawnPoints)
        {
            if (point == null) continue;

            GameObject enemy = Instantiate(enemyPrefab, point.position, Quaternion.identity);
            StartCoroutine(SpawnEffect(enemy));

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    IEnumerator SpawnEffect(GameObject enemy)
    {
        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
        Collider2D col = enemy.GetComponent<Collider2D>();
        EnemyController controller = enemy.GetComponent<EnemyController>();
        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();

        // disable enemy logic during spawn
        if (controller != null) controller.enabled = false;
        if (col != null) col.enabled = false;
        if (rb != null) rb.simulated = false;

        // start invisible and small
        Color color = sr.color;
        color.a = 0f;
        sr.color = color;
        enemy.transform.localScale = Vector3.zero;

        // fade in + scale up
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;

            // smooth ease out
            float smooth = 1f - Mathf.Pow(1f - t, 3f);

            color.a = smooth;
            sr.color = color;
            enemy.transform.localScale = Vector3.one * smooth;

            yield return null;
        }

        // snap to final
        color.a = 1f;
        sr.color = color;
        enemy.transform.localScale = Vector3.one;

        // little overshoot pop
        float popElapsed = 0f;
        while (popElapsed < scalePopDuration)
        {
            popElapsed += Time.deltaTime;
            float t = popElapsed / scalePopDuration;
            float scale = 1f + 0.15f * Mathf.Sin(t * Mathf.PI);
            enemy.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        enemy.transform.localScale = Vector3.one;

        // enable enemy
        if (col != null) col.enabled = true;
        if (rb != null) rb.simulated = true;
        if (controller != null) controller.enabled = true;
    }
}
