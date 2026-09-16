using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    private void Awake()
    {
        // Singleton sencillo para acceder al AudioManager desde cualquier script
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Conversión de Volumen

    // Fórmula lineal a decibelios: (Volume * 40) - 40, mapeado a -80 dB si es 0
    private float LinearToDecibels(float volume)
    {
        if (volume <= 0f) return -80f;
        return (volume * 40f) - 40f;
    }

    // Cambia el volumen del canal Master (recibe valor entre 0 y 1)
    public void SetMasterVolume(float volume)
    {
        if (audioMixer == null) return;
        audioMixer.SetFloat("MasterVolume", LinearToDecibels(volume));
    }

    // Cambia el volumen del canal de Música (recibe valor entre 0 y 1)
    public void SetMusicVolume(float volume)
    {
        if (audioMixer == null) return;
        audioMixer.SetFloat("MusicVolume", LinearToDecibels(volume));
    }

    // Cambia el volumen del canal de Efectos (recibe valor entre 0 y 1)
    public void SetSFXVolume(float volume)
    {
        if (audioMixer == null) return;
        audioMixer.SetFloat("SFXVolume", LinearToDecibels(volume));
    }

    #endregion

    #region Reproducción de Sonidos

    // Reproduce un efecto de sonido único
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // Reproduce o cambia la música de fondo
    public void PlayMusic(AudioClip musicClip, bool loop = true)
    {
        if (musicSource != null && musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }

    #endregion
}