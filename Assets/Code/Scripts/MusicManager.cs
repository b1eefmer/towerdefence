using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;
    private AudioSource audioSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                    audioSource = gameObject.AddComponent<AudioSource>();

                VolumeSettings.Apply(audioSource, AudioChannel.Music);
            }
            else
            {
                Destroy(gameObject);
        }
    }

    public void StopMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    public void PlayMusic()
    {
        if (audioSource != null && !audioSource.isPlaying)
            VolumeSettings.PlayMusic(audioSource);
    }

    public void RestartMusic()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            VolumeSettings.PlayMusic(audioSource);
        }
    }
}
