using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class MenuManager : MonoBehaviour
{
    [Header("Menús")]
    public GameObject mainMenu;
    public GameObject velvet;
    public GameObject config;
    public GameObject gachaMenu;

    [Header("Transición Velvet")]
    public GameObject panelFundido1;
    public Animator animatorPanel1;

    public GameObject objetoVideo;
    public VideoPlayer videoPlayer;

    public GameObject panelFundido2;
    public Animator animatorPanel2;

    [Header("Parámetros Booleanos (Animators)")]
    public string boolFundidoAzul1 = "fundidoAzul1";
    public string boolFundidoAzul2 = "fundidoAzul2";

    [Header("Tiempos de Espera (Inspector)")]
    public float duracionFundido1 = 1.0f;
    public float duracionVideo = 4.0f;
    public float anticipacionPanel2 = 0.8f;
    public float duracionFundido2 = 1.2f;

    private Coroutine transicionVelvetCoroutine;

    public enum Menu
    {
        Main,
        Velvet,
        Config,
        Gacha
    }
    public Menu currentMenu;

    void Start()
    {
        if (panelFundido1 != null) panelFundido1.SetActive(false);
        if (objetoVideo != null) objetoVideo.SetActive(false);
        if (panelFundido2 != null) panelFundido2.SetActive(false);

        CambiarMenu(Menu.Main);
    }

    public void CambiarMenu(Menu menu)
    {
        currentMenu = menu;

        mainMenu.SetActive(menu == Menu.Main);
        velvet.SetActive(menu == Menu.Velvet);
        config.SetActive(menu == Menu.Config);
        gachaMenu.SetActive(menu == Menu.Gacha);
    }

    public void IrAMain()
    {
        CambiarMenu(Menu.Main);
    }

    public void IrAVelvet()
    {
        if (transicionVelvetCoroutine != null)
        {
            StopCoroutine(transicionVelvetCoroutine);
        }
        transicionVelvetCoroutine = StartCoroutine(TransicionVelvetRoutine());
    }

    private IEnumerator TransicionVelvetRoutine()
    {
        // 1. PRIMER PANEL: Fundido a azul
        if (panelFundido1 != null) panelFundido1.SetActive(true);
        yield return null; // Margen de un fotograma para despertar el Animator

        if (animatorPanel1 != null) animatorPanel1.SetBool(boolFundidoAzul1, true);

        yield return new WaitForSeconds(duracionFundido1);

        // 2. VIDEO DE TRANSICIÓN: Se enciende y prepara antes de apagar el panel
        if (objetoVideo != null) objetoVideo.SetActive(true);

        if (videoPlayer != null)
        {
            videoPlayer.enabled = true;
            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared)
            {
                yield return null; // Espera a que cargue el vídeo en memoria
            }
            videoPlayer.Play();
        }

        if (animatorPanel1 != null) animatorPanel1.SetBool(boolFundidoAzul1, false);
        if (panelFundido1 != null) panelFundido1.SetActive(false);

        float tiempoHastaPanel2 = Mathf.Max(0f, duracionVideo - anticipacionPanel2);
        yield return new WaitForSeconds(tiempoHastaPanel2);

        // 3. SEGUNDO PANEL: Entra antes de que corte el vídeo
        if (panelFundido2 != null) panelFundido2.SetActive(true);
        yield return null;

        if (animatorPanel2 != null) animatorPanel2.SetBool(boolFundidoAzul2, true);

        yield return new WaitForSeconds(anticipacionPanel2);

        // Apagar el vídeo
        if (videoPlayer != null) videoPlayer.Stop();
        if (objetoVideo != null) objetoVideo.SetActive(false);

        // Cambiar la pantalla a Velvet mientras el panel 2 tapa la pantalla
        CambiarMenu(Menu.Velvet);

        yield return new WaitForSeconds(duracionFundido2);

        if (animatorPanel2 != null) animatorPanel2.SetBool(boolFundidoAzul2, false);
        if (panelFundido2 != null) panelFundido2.SetActive(false);

        transicionVelvetCoroutine = null;
    }

    public void IrAConfig()
    {
        CambiarMenu(Menu.Config);
    }

    public void IrAGacha()
    {
        CambiarMenu(Menu.Gacha);
    }

    // MÉTODOS PARA SALIR DEL GACHA
    public void SalirDeGachaAVelvet()
    {
        CambiarMenu(Menu.Velvet);
    }

    public void SalirDeGachaAMain()
    {
        CambiarMenu(Menu.Main);
    }
}