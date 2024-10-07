using UnityEngine.Audio;
using UnityEngine;
using System;
using Random = UnityEngine.Random;


[ExecuteInEditMode]
public class SoundBank : MonoBehaviour
{
    public SoundEffect[] soundEffects;
    public AudioMixerGroup sfxMixerGroup;
    public AudioSource soundTesterSource;
    public float soundCooldown = 0.12f;

    [HideInInspector]
    public SoundEffect
        // Gameplay SFX
        TakeDamage,
        PlayerDeath,
        BeamWeapon,
        EnemyDie,
        LevelComplete,
        SpeakingSound,
        MenuClick;

    void Start()
    {
        LoadSounds();
    }

    public void LoadSounds()
    {
        soundEffects = Resources.LoadAll<SoundEffect>("SoundEffects");
        
        ClearAudioSources();

        TakeDamage = GetSoundByName(nameof(TakeDamage));
        PlayerDeath = GetSoundByName(nameof(PlayerDeath));
        BeamWeapon = GetSoundByName(nameof(BeamWeapon));
        EnemyDie = GetSoundByName(nameof(EnemyDie));
        MenuClick = GetSoundByName(nameof(MenuClick));
        SpeakingSound = GetSoundByName(nameof(SpeakingSound));
        LevelComplete = GetSoundByName(nameof(LevelComplete));

        soundTesterSource = gameObject.AddComponent<AudioSource>();
        soundTesterSource.playOnAwake = false;
        soundTesterSource.outputAudioMixerGroup = sfxMixerGroup;

        foreach (var s in soundEffects)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.playOnAwake = false;
            s.source.outputAudioMixerGroup = sfxMixerGroup;
            s.canPlay = true;
        }
    }

    public void ClearAudioSources()
    {
        var audioSources = GetComponentsInChildren<AudioSource>();
        foreach (var audioSource in audioSources)
        {
            DestroyImmediate(audioSource);
        }
    }

    public void PlaySound(SoundEffect sound, float pitchRandomizationAmount = 0)
    {
        if (sound == null)
        {
            //print("SoundBank: PlaySound():  Sound was null");
            return;
        }

        if (AudioManager.Instance.audioMuted || !sound.canPlay) return;

        if (!sound.clip)
        {
            Debug.LogError($"Sound {sound.name} has no audio clip assigned.");
            return;
        }

        if (pitchRandomizationAmount > 0)
        {
            float pitch = Random.Range(1 - sound.pitchRandomizationAmount, 1 + sound.pitchRandomizationAmount);
            sound.source.pitch = pitch;
        }

        sound.source.PlayOneShot(sound.clip);
        StartCoroutine(AudioManager.Instance.SoundCooldown(sound));
        sound.source.pitch = 1.0f;
    }


    SoundEffect GetSoundByName(string soundName)
    {
        return Array.Find(soundEffects, s => s.name == soundName);
    }

}
