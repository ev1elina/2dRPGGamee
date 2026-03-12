using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DamageFlash : MonoBehaviour
{
    public Image flashImage;
    [SerializeField] private float flashDuration = 0.3f;
    [SerializeField] private float minAlpha = 0.15f;  // first hit = light red
    [SerializeField] private float maxAlpha = 0.7f;    // low health = intense red

    private Coroutine flashRoutine;
    public BloodSplash bloodSplash;

    public void Flash(float healthPercent)
    {
        // less health = stronger flash
        float alpha = Mathf.Lerp(maxAlpha, minAlpha, healthPercent);
        Color flashColor = new Color(0.8f, 0f, 0f, alpha);

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine(flashColor));

        if (bloodSplash == null)
            bloodSplash = FindAnyObjectByType<BloodSplash>();
        if (bloodSplash != null)
            bloodSplash.Splash();
    }

    IEnumerator FlashRoutine(Color flashColor)
    {
        flashImage.color = flashColor;
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;
            flashImage.color = Color.Lerp(flashColor, Color.clear, t);
            yield return null;
        }
        flashImage.color = Color.clear;
        flashRoutine = null;
    }
}
