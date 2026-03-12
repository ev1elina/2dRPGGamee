using UnityEngine;
using System;
using MoreMountains.Tools;

[Serializable]
public class PlayerSaveData
{
    public int health;
    public int potions;
}

public class SaveManager : MonoBehaviour
{
    public string saveFile = "player_save";

    public void Save(PlayerHealth player, Inventory inv)
    {
        PlayerSaveData data = new PlayerSaveData();
        data.health = player != null ? player.currentHealth : 0;
        data.potions = inv != null ? inv.potionCount : 0;
        MMSaveLoadManager.Save(data, saveFile + ".save", "");
    }

    public PlayerSaveData Load()
    {
        var obj = (PlayerSaveData)MMSaveLoadManager.Load(typeof(PlayerSaveData), saveFile + ".save", "");
        return obj;
    }
}
