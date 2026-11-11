using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip BackgroundMusic;
    public AudioClip Click;
    public AudioClip FinishingGame;

    void Start()
    {
        // Start background music
        if (musicSource != null)
        {
            musicSource.clip = BackgroundMusic;
            musicSource.loop = true;

            if (!musicSource.isPlaying)
                musicSource.Play();
        }
    }

    public void PlayClick()
    {
        if (sfxSource != null && Click != null)
        {
            sfxSource.PlayOneShot(Click);
        }
    }

    public void PlayWin()
    {
        if (sfxSource != null && FinishingGame != null)
        {
            sfxSource.PlayOneShot(FinishingGame);
        }
    }
}