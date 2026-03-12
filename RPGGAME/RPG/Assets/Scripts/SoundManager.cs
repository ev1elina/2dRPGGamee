using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Volumes")]
    [Range(0f,1f)] public float masterVolume = 1f;
    [Range(0f,1f)] public float musicVolume = 1f;
    [Range(0f,1f)] public float sfxVolume = 1f;

    [Header("SFX Pool")]
    [SerializeField] private int sfxPoolSize = 12;

    [Header("Named Clips (optional)")]
    [SerializeField] private NamedClip[] namedClips;

    private AudioSource musicSource;
    private AudioSource[] sfxPool;
    private System.Collections.Generic.Dictionary<string, AudioSource> loopSources;
    private Dictionary<string, AudioClip> clipLookup;

    [System.Serializable]
    public class NamedClip { public string name; public AudioClip clip; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // music source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        // sfx pool
        sfxPool = new AudioSource[sfxPoolSize];
        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject go = new GameObject("SFXSource_" + i);
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            sfxPool[i] = src;
        }

        // named clip lookup
        clipLookup = new Dictionary<string, AudioClip>();
        if (namedClips != null)
        {
            foreach (var nc in namedClips)
            {
                if (nc != null && nc.clip != null && !string.IsNullOrEmpty(nc.name))
                    clipLookup[nc.name] = nc.clip;
            }
        }

        // looped sfx sources
        loopSources = new System.Collections.Generic.Dictionary<string, AudioSource>();
    }

    // --- Music ---
    public void PlayMusic(AudioClip clip, float volume = 1f, bool loop = true)
    {
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = masterVolume * musicVolume * Mathf.Clamp01(volume);
        musicSource.Play();
    }

    public void StopMusic(float fadeOut = 0f)
    {
        if (fadeOut <= 0f)
        {
            musicSource.Stop();
            musicSource.clip = null;
        }
        else
        {
            StartCoroutine(FadeAndStopMusic(fadeOut));
        }
    }

    System.Collections.IEnumerator FadeAndStopMusic(float dur)
    {
        float start = musicSource.volume;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(start, 0f, t / dur);
            yield return null;
        }
        musicSource.Stop();
        musicSource.clip = null;
        musicSource.volume = start;
    }

    // --- SFX ---
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource src = GetAvailableSFXSource();
        if (src == null)
        {
            // fallback
            AudioSource.PlayClipAtPoint(clip, Camera.main != null ? Camera.main.transform.position : Vector3.zero, masterVolume * sfxVolume * volume);
            return;
        }
        src.clip = clip;
        src.volume = masterVolume * sfxVolume * Mathf.Clamp01(volume);
        src.spatialBlend = 0f;
        src.Play();
    }

    public void PlaySFXAtPosition(AudioClip clip, Vector3 pos, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, pos, masterVolume * sfxVolume * Mathf.Clamp01(volume));
    }

    public void Play(string name, float volume = 1f)
    {
        if (string.IsNullOrEmpty(name)) return;
        if (clipLookup != null && clipLookup.TryGetValue(name, out AudioClip c))
            PlaySFX(c, volume);
    }

    AudioSource GetAvailableSFXSource()
    {
        for (int i = 0; i < sfxPool.Length; i++)
        {
            if (!sfxPool[i].isPlaying) return sfxPool[i];
        }
        // steal the first one if all busy
        return sfxPool.Length > 0 ? sfxPool[0] : null;
    }

    // --- Looping SFX ---
    public void PlayLoop(string name, float volume = 1f)
    {
        if (string.IsNullOrEmpty(name)) return;
        if (clipLookup == null || !clipLookup.TryGetValue(name, out AudioClip clip) || clip == null) return;

        if (loopSources.ContainsKey(name))
        {
            var existing = loopSources[name];
            if (existing != null && existing.isPlaying) return; // already playing
        }

        AudioSource src = GetAvailableSFXSource();
        if (src == null)
        {
            GameObject go = new GameObject("LoopSFX_" + name);
            go.transform.SetParent(transform);
            src = go.AddComponent<AudioSource>();
        }

        src.clip = clip;
        src.loop = true;
        src.volume = masterVolume * sfxVolume * Mathf.Clamp01(volume);
        src.spatialBlend = 0f;
        src.Play();

        loopSources[name] = src;
    }

    public void StopLoop(string name)
    {
        if (string.IsNullOrEmpty(name)) return;
        if (loopSources == null) return;
        if (loopSources.TryGetValue(name, out AudioSource src) && src != null)
        {
            src.Stop();
            src.loop = false;
            loopSources.Remove(name);
            // if the source was from the pool, clear its clip
            for (int i = 0; i < sfxPool.Length; i++)
            {
                if (sfxPool[i] == src)
                {
                    sfxPool[i].clip = null;
                    break;
                }
            }
        }
    }

    // --- Volume controls ---
    public void SetMasterVolume(float v)
    {
        masterVolume = Mathf.Clamp01(v);
        ApplyVolumeSettings();
    }

    public void SetMusicVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        ApplyVolumeSettings();
    }

    public void SetSFXVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        ApplyVolumeSettings();
    }

    void ApplyVolumeSettings()
    {
        if (musicSource != null)
            musicSource.volume = masterVolume * musicVolume;
        if (sfxPool != null)
        {
            foreach (var s in sfxPool)
                s.volume = masterVolume * sfxVolume;
        }
    }

    // Save/Load convenience
    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("masterVolume", masterVolume);
        PlayerPrefs.SetFloat("musicVolume", musicVolume);
        PlayerPrefs.SetFloat("sfxVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("masterVolume", masterVolume);
        musicVolume = PlayerPrefs.GetFloat("musicVolume", musicVolume);
        sfxVolume = PlayerPrefs.GetFloat("sfxVolume", sfxVolume);
        ApplyVolumeSettings();
    }
}
