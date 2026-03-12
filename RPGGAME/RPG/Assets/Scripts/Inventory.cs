using UnityEngine;
using System;

public class Inventory : MonoBehaviour
{
    public int potionCount = 0;

    public event Action<int> OnPotionCountChanged;

    public void AddPotion(int amount)
    {
        potionCount += amount;
        OnPotionCountChanged?.Invoke(potionCount);
    }

    public bool UsePotion(int amount, PlayerHealth player)
    {
        if (potionCount <= 0) return false;
        potionCount -= 1;
        OnPotionCountChanged?.Invoke(potionCount);
        if (player != null)
            player.Heal(amount);
        return true;
    }
}
