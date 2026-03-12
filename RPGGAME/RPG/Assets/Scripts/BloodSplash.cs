using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BloodSplash : MonoBehaviour
{
    public RectTransform splatParent;
    public int minBlobs = 3;
    public int maxBlobs = 6;
    public int minSize = 128;
    public int maxSize = 256;
    public float lifetime = 1.2f;

    void Start()
    {
        if (splatParent == null)
        {
            var df = FindAnyObjectByType<DamageFlash>();
            if (df != null && df.flashImage != null)
            {
                var canvas = df.flashImage.canvas;
                if (canvas != null)
                    splatParent = canvas.transform as RectTransform;
            }
        }
    }

    public void Splash()
    {
        if (splatParent == null) return;
        StartCoroutine(SplashRoutine());
    }

    IEnumerator SplashRoutine()
    {
        int count = Random.Range(minBlobs, maxBlobs + 1);
        for (int i = 0; i < count; i++)
        {
            CreateSplat();
            yield return new WaitForSeconds(0.03f);
        }
    }

    void CreateSplat()
    {
        int size = Random.Range(minSize, maxSize + 1);
        Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color clear = new Color(0, 0, 0, 0);
        Color[] cols = new Color[size * size];
        for (int p = 0; p < cols.Length; p++) cols[p] = clear;

        int blobs = Random.Range(3, 6);
        for (int b = 0; b < blobs; b++)
        {
            float cx = Random.Range(0.2f, 0.8f) * size;
            float cy = Random.Range(0.2f, 0.8f) * size;
            float r = Random.Range(size * 0.12f, size * 0.4f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cx; float dy = y - cy;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d < r)
                    {
                        float t = 1f - (d / r);
                        float alpha = t * Random.Range(0.35f, 0.9f);
                        int idx = y * size + x;
                        Color prev = cols[idx];
                        float outA = prev.a + alpha * (1 - prev.a);
                        Color blood = new Color(0.6f, 0f, 0f, outA);
                        cols[idx] = blood;
                    }
                }
            }
        }

        tex.SetPixels(cols);
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);

        GameObject go = new GameObject("BloodSplat", typeof(RectTransform));
        go.transform.SetParent(splatParent, false);
        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.raycastTarget = false;
        img.color = Color.white;

        RectTransform rt = go.GetComponent<RectTransform>();
        Vector2 parentSize = splatParent.rect.size;
        Vector2 anchored = new Vector2(Random.Range(-parentSize.x / 2f, parentSize.x / 2f), Random.Range(-parentSize.y / 2f, parentSize.y / 2f));
        rt.anchoredPosition = anchored;
        float scale = Random.Range(0.4f, 1.2f);
        rt.sizeDelta = new Vector2(size * scale, size * scale);
        rt.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        StartCoroutine(FadeAndDestroy(img, tex, sprite, lifetime));
    }

    IEnumerator FadeAndDestroy(Image img, Texture2D tex, Sprite sprite, float life)
    {
        float elapsed = 0f;
        Color start = img.color;
        while (elapsed < life)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / life);
            img.color = Color.Lerp(start, new Color(start.r, start.g, start.b, 0f), t);
            yield return null;
        }
        if (img != null && img.gameObject != null) Destroy(img.gameObject);
        if (sprite != null) Destroy(sprite);
        if (tex != null) Destroy(tex);
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BloodSplash : MonoBehaviour
{
    public static BloodSplash Instance { get; private set; }

    [Tooltip("Default number of splats to create when Splash() is called")]
    public int defaultSplats = 5;
    [Tooltip("How long the splats remain visible before fading")]
    public float duration = 1.0f;
    [Tooltip("Sorting order for the blood canvas so it renders above UI")]
    public int canvasSortingOrder = 1000;

    Canvas bloodCanvas;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    Canvas GetOrCreateCanvas()
    {
        if (bloodCanvas != null) return bloodCanvas;
        bloodCanvas = FindObjectOfType<Canvas>();
        // Prefer an existing canvas; otherwise create a dedicated blood canvas
        if (bloodCanvas == null)
        {
            GameObject go = new GameObject("BloodCanvas");
            bloodCanvas = go.AddComponent<Canvas>();
            bloodCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
        }
        // Ensure it renders above other UI
        bloodCanvas.sortingOrder = Mathf.Max(bloodCanvas.sortingOrder, canvasSortingOrder);
        return bloodCanvas;
    }

    public void Splash()
    {
        Splash(defaultSplats);
    }

    public void Splash(int count)
    {
        if (count <= 0) return;
        StartCoroutine(SpawnSplats(count));
    }

    IEnumerator SpawnSplats(int count)
    {
        Canvas cv = GetOrCreateCanvas();

        for (int i = 0; i < count; i++)
        {
            // procedural texture size (keep small to avoid memory churn)
            int size = Random.Range(128, 384);
            Texture2D tex = GenerateSplatTexture(size, size);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);

            GameObject go = new GameObject("blood_splat");
            go.transform.SetParent(cv.transform, false);
            Image img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.color = new Color(0.85f, 0.05f, 0.05f, 1f);

            RectTransform rt = img.rectTransform;
            float scale = Random.Range(0.25f, 1.0f);
            rt.sizeDelta = new Vector2(tex.width * scale, tex.height * scale);
            rt.anchoredPosition = new Vector2(Random.Range(-Screen.width / 2, Screen.width / 2), Random.Range(-Screen.height / 2, Screen.height / 2));
            rt.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

            // animate: small pop then fade out
            StartCoroutine(AnimateAndCleanup(img, sprite, tex, duration));

            // seed small delay between splats for nicer distribution
            yield return new WaitForSeconds(0.02f);
        }
    }

    IEnumerator AnimateAndCleanup(Image img, Sprite sprite, Texture2D tex, float life)
    {
        float t = 0f;
        RectTransform rt = img.rectTransform;
        Vector3 startScale = rt.localScale * 0.5f;
        Vector3 endScale = Vector3.one * 1.3f;
        img.canvasRenderer.SetAlpha(1f);
        rt.localScale = startScale;

        while (t < life)
        {
            t += Time.deltaTime;
            float p = t / life;
            rt.localScale = Vector3.Lerp(startScale, endScale, Mathf.SmoothStep(0f, 1f, p));
            float a = Mathf.Lerp(1f, 0f, p);
            img.color = new Color(img.color.r, img.color.g, img.color.b, a);
            yield return null;
        }

        Destroy(img.gameObject);
        if (sprite != null) Destroy(sprite);
        if (tex != null) Destroy(tex);
    }

    Texture2D GenerateSplatTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2(w / 2f, h / 2f);
        float maxR = Mathf.Min(w, h) * 0.45f;
        float noiseScale = Random.Range(0.6f, 1.6f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = x - center.x;
                float dy = y - center.y;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                // base circular falloff
                float baseAlpha = Mathf.Clamp01(1f - (dist / maxR));

                // add perlin noise to make ragged edges
                float n = Mathf.PerlinNoise((x + Random.value * 1000f) / (w * noiseScale), (y + Random.value * 1000f) / (h * noiseScale));
                float alpha = baseAlpha * Mathf.Pow(n, 0.8f);

                // sprinkle some tiny droplets
                if (Random.value < 0.002f) alpha = 1f;

                Color c = new Color(1f, 0f, 0f, alpha);
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();
        return tex;
    }
}
