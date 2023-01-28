using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource source;
    
    public AudioClip hitSound;
    public AudioClip cheerSound;

    public static SoundManager Instance;

    void Awake()
    {
        Instance = this;
    }

    public void PlayHitSound()
    {
        source.PlayOneShot(hitSound);
    }

    public void PlayCheer()
    {
        source.PlayOneShot(cheerSound);
    }
}
