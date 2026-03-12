using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Image damageImage; // optional delayed damage bar
    [SerializeField] private float damageSpeed = 2f;

    private float targetFill = 1f;
    private float damageFill = 1f;

    public void SetHealth(float normalizedHealth)
    {
        targetFill = Mathf.Clamp01(normalizedHealth);
        if (fillImage != null)
            fillImage.fillAmount = targetFill;
    }

    void Update()
    {
        // delayed damage bar catches up smoothly
        if (damageImage != null)
        {
            if (damageFill > targetFill)
            {
                damageFill -= damageSpeed * Time.deltaTime;
                if (damageFill < targetFill)
                    damageFill = targetFill;
                damageImage.fillAmount = damageFill;
            }
            else
            {
                damageFill = targetFill;
                damageImage.fillAmount = damageFill;
            }
        }
    }
}
