using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    public void SetMasterVolume(float v)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.SetMasterVolume(v);
    }

    public void SetMusicVolume(float v)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.SetMusicVolume(v);
    }

    public void SetSFXVolume(float v)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.SetSFXVolume(v);
    }

    public void SaveSettings()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.SaveSettings();
    }
}
