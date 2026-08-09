using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private int poolSize = 10;

    private List<AudioSource> sfxPool;
    private Coroutine musicRoutine;

    private void Awake()
    {
        ServiceLocator.Register<AudioManager>(this);

        sfxPool = new List<AudioSource>();
        for (int i = 0; i < poolSize; i++)
        {
            sfxPool.Add(gameObject.AddComponent<AudioSource>());
        }
    }

    public void PlaySFX(AudioData data)
    {
        for (int i = 0; i < poolSize; i++)
        {
            if (!sfxPool[i].isPlaying)
            {
                sfxPool[i].PlayOneShot(data.soundClip, data.volume);
                return;
            }
        }
    }

    public void PlayMusic(AudioData data, float fadeTime = 0.5f)
    {
        if (musicSource.clip == data.soundClip)
            return;

        if (musicRoutine != null)
            StopCoroutine(musicRoutine);

        musicRoutine = StartCoroutine(PlayMusicRoutine(data.soundClip, data.volume, fadeTime));
    }

    private IEnumerator PlayMusicRoutine(AudioClip clip, float volume, float fadeTime)
    {
        yield return Fade(0, fadeTime);

        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.Play();

        yield return Fade(volume, fadeTime);
    }

    private IEnumerator Fade(float target, float duration)
    {
        float start = musicSource.volume;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(start, target, time / duration);
            yield return null;
        }

        musicSource.volume = target;
    }
}

[System.Serializable]
public class AudioData
{
    public AudioClip soundClip;
    [Range(0, 1)] public float volume = 0.5f;
}
