using UnityEngine;
using UnityEngine.UI;
#if TMP_PRESENT
using TMPro;
#endif

public class InventoryUI : MonoBehaviour
{
    public Inventory inventory;
    public Text countText;
#if TMP_PRESENT
    public TextMeshProUGUI countTMP;
#endif

    void Start()
    {
        if (inventory == null)
            inventory = FindObjectOfType<Inventory>();
        if (inventory != null)
            inventory.OnPotionCountChanged += UpdateUI;
        UpdateUI(inventory != null ? inventory.potionCount : 0);
    }

    void UpdateUI(int count)
    {
        if (countText != null) countText.text = count.ToString();
#if TMP_PRESENT
        if (countTMP != null) countTMP.text = count.ToString();
#endif
    }

    void OnDestroy()
    {
        if (inventory != null) inventory.OnPotionCountChanged -= UpdateUI;
    }
}
