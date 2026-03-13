
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

            StartCoroutine(AnimateAndCleanup(img, sprite, tex, duration));

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

                float baseAlpha = Mathf.Clamp01(1f - (dist / maxR));

                float n = Mathf.PerlinNoise((x + Random.value * 1000f) / (w * noiseScale), (y + Random.value * 1000f) / (h * noiseScale));
                float alpha = baseAlpha * Mathf.Pow(n, 0.8f);

                if (Random.value < 0.002f) alpha = 1f;

                Color c = new Color(1f, 0f, 0f, alpha);
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();
        return tex;
    }
}
