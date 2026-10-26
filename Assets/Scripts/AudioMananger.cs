using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("BGM Clips")]
    public AudioClip titleBgm;
    public AudioClip gameplayBgm;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        bgmSource.loop = true;
    }

    public void PlayTitleBGM()
    {
        if (bgmSource.clip == titleBgm && bgmSource.isPlaying) { return; }

        PlayBGM(titleBgm);
    }

    public void PlayGameplayBGM() => PlayBGM(gameplayBgm);

    private void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null || clip == null) { return; }

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) { return; }
        sfxSource.PlayOneShot(clip);
    }
}