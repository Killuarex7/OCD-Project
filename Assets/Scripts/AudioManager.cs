
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
    public AudioClip ObjectDisappear;
    public AudioClip ObjectFound;

    private void Start()
    {
        if (musicSource != null && BackgroundMusic != null)
        {
            musicSource.clip = BackgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayClick()
    {
        if (sfxSource != null && Click != null)
            sfxSource.PlayOneShot(Click);
    }

    public void PlayWin()
    {
        if (sfxSource != null && FinishingGame != null)
            sfxSource.PlayOneShot(FinishingGame);
    }

    public void PlayObjectDisappear()
    {
        if (sfxSource != null && ObjectDisappear != null)
            sfxSource.PlayOneShot(ObjectDisappear);
    }

    public void PlayObjectFound()
    {
        if (sfxSource != null && ObjectFound != null)
            sfxSource.PlayOneShot(ObjectFound);
    }
}
