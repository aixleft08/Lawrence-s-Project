using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource mainSource;
    public AudioSource musicSource;
    
    public AudioClip hitSound;
    public AudioClip cheerSound;
    public AudioClip selectUI;
    public AudioClip pressUI;

    public AudioClip menuMusic;
    public AudioClip gameplayMusic;

    public static SoundManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        musicSource.loop = true;
        PlayMenuMusic();
    }

    public void PlayHitSound()
    {
        mainSource.PlayOneShot(hitSound);
    }

    public void PlayCheer()
    {
        mainSource.PlayOneShot(cheerSound);
    }
    public void PlaySelectUI()
    {
        mainSource.PlayOneShot(selectUI);
    }
    public void PlayPressUI()
    {
        mainSource.PlayOneShot(pressUI);
    }
    public void PlayMenuMusic()
    {
        musicSource.Stop();
        musicSource.clip = menuMusic;
        musicSource.Play();
    }
    public void PlayGameplayMusic()
    {
        musicSource.Stop();
        musicSource.clip = gameplayMusic;
        musicSource.Play();
    }
}
