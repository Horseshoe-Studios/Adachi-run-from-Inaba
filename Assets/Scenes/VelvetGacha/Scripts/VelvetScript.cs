using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VelvetScript : MonoBehaviour
{
    [Header("Menú / Ventana")]
    [SerializeField] private GameObject menuGacha;

    [Header("Elementos de UI a activar")]
    [SerializeField] private GameObject botonGacha;
    [SerializeField] private GameObject tokensImage; // Imagen del token

    [Header("Animators")]
    [SerializeField] private Animator animatorIgor;
    [SerializeField] private Animator animatorTexto;
    [SerializeField] private Animator animatorGacha;

    [Header("Parámetros Bool de los Animators")]
    [SerializeField] private string boolIgor = "animacionIgor";
    [SerializeField] private string boolTexto = "animacionTexto";
    [SerializeField] private string boolGacha = "animacionGacha";

    [Header("Duraciones entre animaciones (segundos)")]
    [SerializeField] private float duracionIgor = 2.0f;
    [SerializeField] private float duracionTexto = 1.5f;
    [SerializeField] private float duracionGacha = 2.5f;

    [Header("Tokens")]
    [SerializeField] private int tokens = 0;
    [SerializeField] private TextMeshProUGUI textoTokens;

    private Coroutine secuenciaCoroutine;

    void Start()
    {
        ActualizarTextoTokens();

        // Ocultar botón, imagen y texto al iniciar
        if (botonGacha != null)
        {
            botonGacha.SetActive(false);
        }

        if (tokensImage != null)
        {
            tokensImage.SetActive(false);
        }

        if (textoTokens != null)
        {
            textoTokens.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (menuGacha != null && menuGacha.activeSelf && secuenciaCoroutine == null)
        {
            IniciarSecuencia();
        }
    }

    public void IniciarSecuencia()
    {
        if (secuenciaCoroutine != null)
        {
            StopCoroutine(secuenciaCoroutine);
        }
        secuenciaCoroutine = StartCoroutine(SecuenciaGachaRoutine());
    }

    private IEnumerator SecuenciaGachaRoutine()
    {
        // 1. Animación de Igor
        if (animatorIgor != null)
        {
            animatorIgor.SetBool(boolIgor, true);
        }

        yield return new WaitForSeconds(duracionIgor);

        // 2. Animación de texto
        if (animatorTexto != null)
        {
            animatorTexto.SetBool(boolTexto, true);
        }

        yield return new WaitForSeconds(duracionTexto);

        // 3. Animación del gacha
        if (animatorGacha != null)
        {
            animatorGacha.SetBool(boolGacha, true);
        }

        yield return new WaitForSeconds(duracionGacha);

        // 4. Espera 1 segundo y activa botón, texto e imagen al mismo tiempo
        yield return StartCoroutine(ActivarInterfazRoutine());

        secuenciaCoroutine = null;
    }

    private IEnumerator ActivarInterfazRoutine()
    {
        yield return new WaitForSeconds(1.0f);

        if (botonGacha != null)
        {
            botonGacha.SetActive(true);
        }

        if (tokensImage != null)
        {
            tokensImage.SetActive(true);
        }

        if (textoTokens != null)
        {
            textoTokens.gameObject.SetActive(true);
        }
    }

    // --- LÓGICA DE TIRO ---

    public void TirarGacha()
    {
        if (tokens > 0)
        {
            TirarCorrecto();
        }
        else
        {
            NoTokens();
        }
    }

    private void TirarCorrecto()
    {
        tokens--;
        ActualizarTextoTokens();
        Debug.Log("Tiro realizado con éxito. Tokens restantes: " + tokens);
    }

    private void NoTokens()
    {
        Debug.LogWarning("No tienes tokens suficientes para tirar.");
    }

    private void ActualizarTextoTokens()
    {
        if (textoTokens != null)
        {
            textoTokens.text = tokens.ToString();
        }
    }

    private void OnValidate()
    {
        ActualizarTextoTokens();
    }

    public void ResetearAnimaciones()
    {
        if (secuenciaCoroutine != null)
        {
            StopCoroutine(secuenciaCoroutine);
            secuenciaCoroutine = null;
        }

        if (animatorIgor != null) animatorIgor.SetBool(boolIgor, false);
        if (animatorTexto != null) animatorTexto.SetBool(boolTexto, false);
        if (animatorGacha != null) animatorGacha.SetBool(boolGacha, false);

        if (botonGacha != null)
        {
            botonGacha.SetActive(false);
        }

        if (tokensImage != null)
        {
            tokensImage.SetActive(false);
        }

        if (textoTokens != null)
        {
            textoTokens.gameObject.SetActive(false);
        }
    }
}