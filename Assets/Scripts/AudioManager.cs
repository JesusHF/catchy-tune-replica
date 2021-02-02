using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour
{
    public AudioSource songAudioSource;
    public AudioSource sfxAudioSource;

    public Sound[] songs;
    public Sound[] sfxSounds;

    #region Singleton
    public static AudioManager instance { get; private set; }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    public void PlaySong(string name, bool loop = false)
    {
        var music = Array.Find(songs, song => song.name == name);
        Assert.IsNotNull(music, "Song '" + name + "' not found");

        songAudioSource.clip = music.clip;
        songAudioSource.loop = loop;
        songAudioSource.Play();
    }

    public void PlaySongWithCallback(string name, Action callback = null)
    {
        StartCoroutine(StartSongWithCallback(name, callback));
    }

    public void PlaySfx(string name, float volume = 1f)
    {
        var sfx = Array.Find(sfxSounds, sound => sound.name == name);
        Assert.IsNotNull(sfx, "SFX '" + name + "' not found");

        sfxAudioSource.PlayOneShot(sfx.clip, volume);
    }

    public void StopCurrentSfx()
    {
        sfxAudioSource.Stop();
    }

    public void FadeCurrentSong(float duration, float targetVolume = 0f)
    {
        StartCoroutine(StartFade(duration, targetVolume));
    }

    private IEnumerator StartFade(float duration, float targetVolume)
    {
        float currentTime = 0;
        float start = songAudioSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            songAudioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }

        songAudioSource.Stop();
        songAudioSource.volume = 1f;
        yield break;
    }

    private IEnumerator StartSongWithCallback(string name, Action callback)
    {
        var music = Array.Find(songs, song => song.name == name);
        Assert.IsNotNull(music, "Song '" + name + "' not found");

        songAudioSource.clip = music.clip;
        songAudioSource.loop = false;
        songAudioSource.Play();

        yield return new WaitForSeconds(music.clip.length);
        callback?.Invoke();
    }
}