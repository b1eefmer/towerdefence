using UnityEngine;

public enum AudioChannel
{
    Music,
    Sfx
}

public static class VolumeSettings
{
    private const string MusicKey = "MusicVolume";
    private const string SfxKey = "SfxVolume";
    private const float MinVolume = 0.0001f;

    private static bool loaded;
    private static float musicVolume = 1f;
    private static float sfxVolume = 1f;

    public static float MusicVolume
    {
        get
        {
            EnsureLoaded();
            return musicVolume;
        }
    }

    public static float SfxVolume
    {
        get
        {
            EnsureLoaded();
            return sfxVolume;
        }
    }

    public static void EnsureLoaded()
    {
        if (loaded)
            return;

        musicVolume = Mathf.Clamp(PlayerPrefs.GetFloat(MusicKey, 1f), MinVolume, 1f);
        sfxVolume = Mathf.Clamp(PlayerPrefs.GetFloat(SfxKey, 1f), MinVolume, 1f);
        loaded = true;
    }

    public static void SetMusicVolume(float value)
    {
        EnsureLoaded();
        musicVolume = Mathf.Clamp(value, MinVolume, 1f);
        PlayerPrefs.SetFloat(MusicKey, musicVolume);
        PlayerPrefs.Save();
        ApplyAllSceneVolumes();
    }

    public static void SetSfxVolume(float value)
    {
        EnsureLoaded();
        sfxVolume = Mathf.Clamp(value, MinVolume, 1f);
        PlayerPrefs.SetFloat(SfxKey, sfxVolume);
        PlayerPrefs.Save();
        ApplyAllSceneVolumes();
    }

    public static void PlayMusic(AudioSource source)
    {
        if (source == null)
            return;

        Apply(source, AudioChannel.Music);
        source.Play();
    }

    public static void PlaySfx(AudioSource source)
    {
        if (source == null)
            return;

        Apply(source, AudioChannel.Sfx);
        source.Play();
    }

    public static void Apply(AudioSource source, AudioChannel channel)
    {
        if (source == null)
            return;

        EnsureLoaded();

        AudioSourceVolumeCache cache = source.GetComponent<AudioSourceVolumeCache>();
        if (cache == null)
            cache = source.gameObject.AddComponent<AudioSourceVolumeCache>();

        cache.Initialize(source, channel);
        source.volume = cache.BaseVolume * GetVolume(channel);
    }

    public static void ApplyAllSceneVolumes()
    {
        EnsureLoaded();

        AudioSource[] sources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource source in sources)
        {
            AudioSourceVolumeCache cache = source.GetComponent<AudioSourceVolumeCache>();
            AudioChannel channel = cache != null && cache.IsInitialized
                ? cache.Channel
                : GuessChannel(source);

            Apply(source, channel);
        }
    }

    private static float GetVolume(AudioChannel channel)
    {
        return channel == AudioChannel.Music ? musicVolume : sfxVolume;
    }

    private static AudioChannel GuessChannel(AudioSource source)
    {
        if (source.GetComponentInParent<MusicManager>() != null)
            return AudioChannel.Music;

        string objectName = source.gameObject.name.ToLowerInvariant();
        string clipName = source.clip != null ? source.clip.name.ToLowerInvariant() : string.Empty;
        if (objectName.Contains("music") || clipName.Contains("music") || objectName.Contains("background"))
            return AudioChannel.Music;

        return AudioChannel.Sfx;
    }
}

public class AudioSourceVolumeCache : MonoBehaviour
{
    public float BaseVolume { get; private set; }
    public AudioChannel Channel { get; private set; }
    public bool IsInitialized { get; private set; }

    public void Initialize(AudioSource source, AudioChannel channel)
    {
        if (!IsInitialized)
        {
            BaseVolume = source.volume;
            IsInitialized = true;
        }

        Channel = channel;
    }
}
