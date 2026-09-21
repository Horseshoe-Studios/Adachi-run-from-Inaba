using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VelvetScript : MonoBehaviour
{
    private const string PREF_TOKENS = "TokensGuardados";

    [Header("Conexión con MenuManager")]
    [SerializeField] private MenuManager menuManager;

    [Header("Menú / Ventana")]
    [SerializeField] private GameObject menuGacha;

    [Header("Elementos de UI a activar")]
    [SerializeField] private GameObject botonGacha;
    [SerializeField] private GameObject tokensImage;

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
    [SerializeField] private int tokensPorDefecto = 0; // Tokens iniciales si nunca ha jugado
    private int tokens = 0;
    [SerializeField] private TextMeshProUGUI textoTokens;

    private Coroutine secuenciaCoroutine;
    private bool secuenciaIniciada = false;

    void Start()
    {
        // Cargar tokens guardados; si es la primera vez, usa tokensPorDefecto
        tokens = PlayerPrefs.GetInt(PREF_TOKENS, tokensPorDefecto);
        ActualizarTextoTokens();

        if (botonGacha != null) botonGacha.SetActive(false);
        if (tokensImage != null) tokensImage.SetActive(false);
        if (textoTokens != null) textoTokens.gameObject.SetActive(false);
    }

    void Update()
    {
        if (menuGacha != null)
        {
            if (menuGacha.activeSelf && !secuenciaIniciada)
            {
                // Refrescar tokens por si vienes de la escena de juego habiendo recogido monedas
                tokens = PlayerPrefs.GetInt(PREF_TOKENS, tokens);
                ActualizarTextoTokens();

                secuenciaIniciada = true;
                IniciarSecuencia();
            }
            else if (!menuGacha.activeSelf && secuenciaIniciada)
            {
                secuenciaIniciada = false;
                ResetearAnimaciones();
            }
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
        if (animatorIgor != null) animatorIgor.SetBool(boolIgor, true);
        yield return new WaitForSeconds(duracionIgor);

        if (animatorTexto != null) animatorTexto.SetBool(boolTexto, true);
        yield return new WaitForSeconds(duracionTexto);

        if (animatorGacha != null) animatorGacha.SetBool(boolGacha, true);
        yield return new WaitForSeconds(duracionGacha);

        yield return StartCoroutine(ActivarInterfazRoutine());

        secuenciaCoroutine = null;
    }

    private IEnumerator ActivarInterfazRoutine()
    {
        yield return new WaitForSeconds(1.0f);

        if (botonGacha != null) botonGacha.SetActive(true);
        if (tokensImage != null) tokensImage.SetActive(true);
        if (textoTokens != null) textoTokens.gameObject.SetActive(true);
    }

    // --- LÓGICA DE TIRO ---

    public void TirarGacha()
    {
        // Asegurar que leemos el valor más actualizado
        tokens = PlayerPrefs.GetInt(PREF_TOKENS, tokens);

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
        // Guardar el nuevo valor en disco
        PlayerPrefs.SetInt(PREF_TOKENS, tokens);
        PlayerPrefs.Save();

        ActualizarTextoTokens();
        Debug.Log("Tiro realizado con éxito. Tokens restantes: " + tokens);

        if (menuManager != null)
        {
            menuManager.tirar_gacha();
        }
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

        if (botonGacha != null) botonGacha.SetActive(false);
        if (tokensImage != null) tokensImage.SetActive(false);
        if (textoTokens != null) textoTokens.gameObject.SetActive(false);
    }

    // ==========================================
    // MÉTODOS ESTÁTICOS ACCESIBLES DESDE CUALQUIER ESCENA
    // ==========================================

    // Tu compañero puede llamar a esto desde la escena de jugar al recoger monedas
    public static void AnadirTokens(int cantidad)
    {
        int guardados = PlayerPrefs.GetInt(PREF_TOKENS, 0);
        guardados += cantidad;
        PlayerPrefs.SetInt(PREF_TOKENS, guardados);
        PlayerPrefs.Save();
        Debug.Log("Monedas sumadas: " + cantidad + ". Total actual: " + guardados);
    }

    // Por si necesitas consultar cuántas monedas tiene el jugador desde la escena de juego
    public static int ObtenerTokens()
    {
        return PlayerPrefs.GetInt(PREF_TOKENS, 0);
    }
}