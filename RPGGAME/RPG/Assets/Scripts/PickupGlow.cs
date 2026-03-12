using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PickupGlow : MonoBehaviour
{
    [Header("Pulse")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField, Range(0f, 0.5f)] private float pulseScale = 0.12f;

    [Header("Color")]
    [SerializeField] private Color glowColor = new Color(1f, 0.9f, 0.3f);
    [SerializeField, Range(0f, 1f)] private float colorIntensity = 0.45f;

    private Vector3 baseScale;
    private SpriteRenderer sr;
    private Color baseColor;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        if (sr != null) baseColor = sr.color;
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f; // 0..1

        // scale pulse
        float scaleFactor = 1f + Mathf.Lerp(-pulseScale, pulseScale, t);
        transform.localScale = baseScale * scaleFactor;

        // color tint pulse
        if (sr != null)
        {
            Color target = Color.Lerp(baseColor, glowColor, t * colorIntensity);
            sr.color = target;
        }
    }

    void OnDisable()
    {
        // restore
        if (sr != null) sr.color = baseColor;
        transform.localScale = baseScale;
    }
}
