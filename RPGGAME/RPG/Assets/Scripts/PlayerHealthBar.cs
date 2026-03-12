using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    public Slider slider;
    public Image fill;

    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;
        UpdateColor();
    }

    public void SetHealth(int health)
    {
        slider.value = health;
        UpdateColor();
    }

    void UpdateColor()
    {
        float t = slider.normalizedValue;
        if (t > 0.5f)
            fill.color = Color.Lerp(new Color(1f, 0.9f, 0.1f), new Color(0.2f, 0.9f, 0.2f), (t - 0.5f) * 2f);
        else
            fill.color = Color.Lerp(new Color(0.9f, 0.1f, 0.1f), new Color(1f, 0.9f, 0.1f), t * 2f);
    }
}
