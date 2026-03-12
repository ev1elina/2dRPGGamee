using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Slider slider;
    public Image fill;

    private float displayedHealth;
    private float targetHealth;
    private float lerpSpeed = 5f;

    void Awake()
    {
        if (slider == null)
            slider = GetComponentInChildren<Slider>();
        if (fill == null && slider != null && slider.fillRect != null)
            fill = slider.fillRect.GetComponent<Image>();
    }

    public void SetMaxHealth(int health)
    {
        if (slider == null) return;
        slider.maxValue = health;
        slider.value = health;
        displayedHealth = health;
        targetHealth = health;
        UpdateColor();
    }

    public void SetHealth(int health)
    {
        if (slider == null) return;
        targetHealth = health;
    }

    void Update()
    {
        if (slider == null) return;
        if (Mathf.Abs(displayedHealth - targetHealth) > 0.01f)
        {
            displayedHealth = Mathf.Lerp(displayedHealth, targetHealth, Time.deltaTime * lerpSpeed);
            slider.value = displayedHealth;
            UpdateColor();
        }
        else if (displayedHealth != targetHealth)
        {
            displayedHealth = targetHealth;
            slider.value = displayedHealth;
            UpdateColor();
        }
    }

    void UpdateColor()
    {
        float t = slider.normalizedValue;
        if (t > 0.5f)
            fill.color = Color.Lerp(new Color(0.7f, 0.2f, 0.8f), new Color(0.4f, 0.1f, 0.9f), (t - 0.5f) * 2f);
        else
            fill.color = Color.Lerp(new Color(0.9f, 0.1f, 0.1f), new Color(0.7f, 0.2f, 0.8f), t * 2f);
    }
}
