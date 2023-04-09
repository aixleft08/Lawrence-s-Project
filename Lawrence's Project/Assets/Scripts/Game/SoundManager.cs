using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public Slider musicSlider;
    public Slider sfxSlider;

    public static SoundManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        musicSlider.value = musicSource.volume;
        sfxSlider.value = mainSource.volume;

        musicSource.loop = true;
        PlayMenuMusic();
    }

    void Update()
    {
        musicSource.volume = musicSlider.value;
        mainSource.volume = sfxSlider.value;
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
