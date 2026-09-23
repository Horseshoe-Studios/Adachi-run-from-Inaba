using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    private const string PARAM_MASTER = "masterVolume";
    private const string PARAM_MUSIC = "musicVolume";
    private const string PARAM_SFX = "sfxVolume";

    private const string PREF_MASTER = "VolumenMaster";
    private const string PREF_MUSIC = "VolumenMusic";
    private const string PREF_SFX = "VolumenSFX";

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    // ==========================================
    // CLIPS DE MÚSICA Y SFX
    // ==========================================
    [Header("Música")]
    public AudioClip musicaMenu;
    public AudioClip musicaVelvet;
    public AudioClip musicaJuego;

    [Header("SFX")]
    public AudioClip botonMenu;
    public AudioClip resultadoGacha;
    public AudioClip botonPlay;
    public AudioClip escudoActivar;
    public AudioClip monedaRecoger;
    public AudioClip personajeMover;
    public AudioClip recibirDano;
    public AudioClip perder;

    [Header("SFX - Gacha y Colección")]
    public AudioClip tirarGacha;
    public AudioClip equiparPersona;
    public AudioClip resetearJuego;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            if (musicSource != null)
            {
                musicSource.loop = true;
            }

            // 1. Configuración de AutoRotation bloqueando estrictamente los giros horizontales
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToPortrait = true;

            // 2. Forzar orientación nativa directa en el sistema operativo Android (JNI)
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        // 7 = ActivityInfo.SCREEN_ORIENTATION_SENSOR_PORTRAIT
                        currentActivity.Call("setRequestedOrientation", 7);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("No se pudo forzar orientación nativa: " + e.Message);
            }
#endif

            // 3. Desbloqueo de FPS a la tasa nativa de la pantalla
            QualitySettings.vSyncCount = 0;
            int tasaRefresco = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);
            Application.targetFrameRate = tasaRefresco > 30 ? tasaRefresco : 60;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        // Reafirma las restricciones de orientación si la app se minimiza y vuelve a primer plano
        if (hasFocus)
        {
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToPortrait = true;

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        currentActivity.Call("setRequestedOrientation", 7);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("No se pudo recuperar orientación nativa: " + e.Message);
            }
#endif
        }
    }

    private IEnumerator Start()
    {
        yield return null;
        CargarVolumenesGuardados();
    }

    #region Conversión y Control de Volumen

    private float LinearToDecibels(float volume)
    {
        if (volume <= 0.0001f) return -80f;
        return (volume * 40f) - 40f;
    }

    public void SetMasterVolume(float volume)
    {
        PlayerPrefs.SetFloat(PREF_MASTER, volume);
        PlayerPrefs.Save();
        if (audioMixer != null) audioMixer.SetFloat(PARAM_MASTER, LinearToDecibels(volume));
    }

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat(PREF_MUSIC, volume);
        PlayerPrefs.Save();
        if (audioMixer != null) audioMixer.SetFloat(PARAM_MUSIC, LinearToDecibels(volume));
    }

    public void SetSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat(PREF_SFX, volume);
        PlayerPrefs.Save();
        if (audioMixer != null) audioMixer.SetFloat(PARAM_SFX, LinearToDecibels(volume));
    }

    public void CargarVolumenesGuardados()
    {
        SetMasterVolume(PlayerPrefs.GetFloat(PREF_MASTER, 1f));
        SetMusicVolume(PlayerPrefs.GetFloat(PREF_MUSIC, 1f));
        SetSFXVolume(PlayerPrefs.GetFloat(PREF_SFX, 1f));
    }

    public float GetMasterVolume() => PlayerPrefs.GetFloat(PREF_MASTER, 1f);
    public float GetMusicVolume() => PlayerPrefs.GetFloat(PREF_MUSIC, 1f);
    public float GetSFXVolume() => PlayerPrefs.GetFloat(PREF_SFX, 1f);

    #endregion

    #region Reproducción Base

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayMusic(AudioClip musicClip, bool loop = true)
    {
        if (musicSource != null && musicClip != null)
        {
            if (musicSource.clip == musicClip && musicSource.isPlaying)
            {
                musicSource.loop = loop;
                return;
            }

            musicSource.clip = musicClip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    #endregion

    #region Métodos Directos para el Juego

    public void PlayMusicaMenu() => PlayMusic(musicaMenu, true);
    public void PlayMusicaVelvet() => PlayMusic(musicaVelvet, true);
    public void PlayMusicaJuego() => PlayMusic(musicaJuego, true);

    public void PlayBotonMenu() => PlaySFX(botonMenu);
    public void PlayResultadoGacha() => PlaySFX(resultadoGacha);
    public void PlayBotonPlay() => PlaySFX(botonPlay);
    public void PlayEscudoActivar() => PlaySFX(escudoActivar);
    public void PlayMonedaRecoger() => PlaySFX(monedaRecoger);
    public void PlayPersonajeMover() => PlaySFX(personajeMover);
    public void PlayRecibirDano() => PlaySFX(recibirDano);
    public void PlayPerder() => PlaySFX(perder);

    public void PlayTirarGacha() => PlaySFX(tirarGacha);
    public void PlayEquiparPersona() => PlaySFX(equiparPersona);
    public void PlayResetearJuego() => PlaySFX(resetearJuego);

    #endregion
}