using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;

    }
    public bool CheckSound;

    public SoundAudio[] soundArray;

    [System.Serializable]
    public class SoundAudio
    {
        public Sound sound;
        public AudioClip audioClip;
    }

    public enum Sound
    {
        playerMovement,
        dead,
        die_water,
        die_hit,
        coin,
        sad,
        bull,
        spinningRock,
        elephant,
        deer,
        jeep_horn,
        snake,
        ostrichs,
        jumpOnLog,
        bull_footstep,
        spinningRock_footstep,
        elephant_footstep,
        deer_footstep,
        jeep,
        snake_footstep,
        ostrichs__footstep,
        show_ui,
        milestone,
        button,
        bgm,
        ambient,
        traffic,

    }
    public void PlaySound(Sound newSound,AudioSource audioSource)
    {
        audioSource.PlayOneShot(GetAudioClip(newSound));
    }
    public AudioClip GetAudioClip(Sound newSound)
    {
        foreach (SoundAudio soundAudio in soundArray)
        {
            if (soundAudio.sound == newSound)
            {
                return soundAudio.audioClip;
            }
        }
        return null;
    }
    private void Update()
    {
        if (CheckSound)
        {
            SoungSettingOn();
        }
        else
        {
            SoungSettingOff();
        }
    }
    private void Start()
    {
         CheckSound = true;
    }
    public void SoungSettingOn()
    {
        AudioListener.volume = 1;
        CheckSound = true;
    }
    public void SoungSettingOff()
    {
        AudioListener.volume = 0;
        CheckSound = false;
    }
}
