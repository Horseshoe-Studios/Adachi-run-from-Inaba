using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Menús")]
    public GameObject mainMenu;
    public GameObject velvet;
    public GameObject config;
    public GameObject gachaMenu;
    public GameObject menuVerPersonas;

    [Header("Transición Velvet Room")]
    public GameObject panelFundido1;
    public Animator animatorPanel1;

    public GameObject objetoVideo;
    public VideoPlayer videoPlayer;

    public GameObject panelFundido2;
    public Animator animatorPanel2;

    [Header("Parámetros Booleanos (Animators)")]
    public string boolFundidoAzul1 = "fundidoAzul1";
    public string boolFundidoAzul2 = "fundidoAzul2";

    [Header("Tiempos Transición Velvet")]
    public float duracionFundido1 = 1.0f;
    public float duracionVideo = 4.0f;
    public float anticipacionPanel2 = 0.8f;
    public float duracionFundido2 = 1.2f;

    // ==========================================
    // SECCIÓN: SISTEMA GACHA (GUILLOTINA)
    // ==========================================
    [Header("Gacha - Vídeo Guillotina")]
    public GameObject objetoVideoGuillotina;
    public VideoPlayer videoPlayerGuillotina;
    public float duracionVideoGuillotina = 2.5f;
    public Button botonSaltarAnimacion;

    [Header("Gacha - Ventana de Resultado")]
    public GameObject panelResultado;
    public Image imagenResultado;
    public TextMeshProUGUI textoNombrePersona;
    public Button botonVolverAlGacha;

    [Header("Gacha - Datos de las 10 Personas")]
    public Sprite[] spritesPersonas = new Sprite[10];
    public string[] nombresPersonas = new string[10];
    public bool[] personasDesbloqueadas = new bool[10];

    [Header("Gacha - Secreto (Igor)")]
    public Sprite spriteIgor;
    public string nombreIgor = "Igor";
    public bool igorDesbloqueado = false;

    // ==========================================
    // SECCIÓN: MENÚ VER PERSONAS Y EQUIPAR
    // ==========================================
    [Header("Personaje por Defecto (Adachi)")]
    public Sprite spriteAdachi;                     // Sprite original de Adachi
    public string nombreAdachi = "Adachi";
    public Button botonResetAdachi;                 // Botón para resetear a Adachi

    [Header("Menú Ver Personas - UI General")]
    public TextMeshProUGUI textoEquipado;          // Texto superior: "Equipped: [Nombre]"
    public string prefijoEquipado = "Equipped: ";
    public Sprite spriteBloqueado;
    public Sprite spriteSecreto;

    [Header("Menú Ver Personas - 10 Personas")]
    public Image[] slotsPersonas = new Image[10];
    public TextMeshProUGUI[] textosNombresPersonas = new TextMeshProUGUI[10];
    public Button[] botonesPersonas = new Button[10];

    [Header("Menú Ver Personas - Slot Secreto (Igor)")]
    public Image slotIgor;
    public TextMeshProUGUI textoNombreIgor;
    public Button botonIgor;

    // Variables estáticas: accesibles desde la escena del juego
    public static Sprite spriteSeleccionado;
    public static string nombreSeleccionado = "Adachi";
    public static int indiceEquipado = -1; // -1 = Adachi (default), 0-9 = Personas, 99 = Igor

    private Coroutine transicionVelvetCoroutine;
    private Coroutine gachaCoroutine;
    private int ultimaTirada = 0;

    public enum Menu
    {
        Main,
        Velvet,
        Config,
        Gacha,
        VerPersonas
    }
    public Menu currentMenu;

    void Start()
    {
        if (panelFundido1 != null) panelFundido1.SetActive(false);
        if (objetoVideo != null) objetoVideo.SetActive(false);
        if (panelFundido2 != null) panelFundido2.SetActive(false);

        if (objetoVideoGuillotina != null) objetoVideoGuillotina.SetActive(false);
        if (panelResultado != null) panelResultado.SetActive(false);
        if (botonSaltarAnimacion != null) botonSaltarAnimacion.gameObject.SetActive(false);

        if (botonVolverAlGacha != null)
        {
            botonVolverAlGacha.onClick.RemoveAllListeners();
            botonVolverAlGacha.onClick.AddListener(VolverAGacha);
        }

        // Si no hay nada equipado todavía, arrancar con Adachi
        if (spriteSeleccionado == null && spriteAdachi != null)
        {
            spriteSeleccionado = spriteAdachi;
            nombreSeleccionado = nombreAdachi;
            indiceEquipado = -1;
        }

        ConfigurarClicksDeBotones();
        ActualizarTextoEquipado();
        CambiarMenu(Menu.Main);
    }

    private void ConfigurarClicksDeBotones()
    {
        for (int i = 0; i < botonesPersonas.Length; i++)
        {
            if (botonesPersonas[i] != null)
            {
                int indice = i;
                botonesPersonas[i].onClick.RemoveAllListeners();
                botonesPersonas[i].onClick.AddListener(() => EquiparPersona(indice));
            }
        }

        if (botonIgor != null)
        {
            botonIgor.onClick.RemoveAllListeners();
            botonIgor.onClick.AddListener(EquiparIgor);
        }

        if (botonResetAdachi != null)
        {
            botonResetAdachi.onClick.RemoveAllListeners();
            botonResetAdachi.onClick.AddListener(ResetAAdachi);
        }
    }

    public void CambiarMenu(Menu menu)
    {
        currentMenu = menu;

        if (mainMenu != null) mainMenu.SetActive(menu == Menu.Main);
        if (velvet != null) velvet.SetActive(menu == Menu.Velvet);
        if (config != null) config.SetActive(menu == Menu.Config);
        if (gachaMenu != null) gachaMenu.SetActive(menu == Menu.Gacha);
        if (menuVerPersonas != null) menuVerPersonas.SetActive(menu == Menu.VerPersonas);
    }

    // ==========================================
    // NAVEGACIÓN
    // ==========================================
    public void IrAMain() => CambiarMenu(Menu.Main);
    public void IrAConfig() => CambiarMenu(Menu.Config);
    public void IrAGacha() => CambiarMenu(Menu.Gacha);
    public void SalirDeGachaAVelvet() => CambiarMenu(Menu.Velvet);
    public void SalirDeGachaAMain() => CambiarMenu(Menu.Main);

    public void IrAVelvet()
    {
        if (transicionVelvetCoroutine != null)
        {
            StopCoroutine(transicionVelvetCoroutine);
        }
        transicionVelvetCoroutine = StartCoroutine(TransicionVelvetRoutine());
    }

    // ==========================================
    // LÓGICA DE LA TIRADA DE GACHA
    // ==========================================
    public void tirar_gacha()
    {
        if (gachaCoroutine != null) return;
        gachaCoroutine = StartCoroutine(TirarGachaRoutine());
    }

    private IEnumerator TirarGachaRoutine()
    {
        if (panelResultado != null) panelResultado.SetActive(false);

        ultimaTirada = Random.Range(0, spritesPersonas.Length);
        personasDesbloqueadas[ultimaTirada] = true;
        ComprobarDesbloqueoIgor();

        if (botonSaltarAnimacion != null) botonSaltarAnimacion.gameObject.SetActive(true);

        if (objetoVideoGuillotina != null && videoPlayerGuillotina != null)
        {
            objetoVideoGuillotina.SetActive(true);
            videoPlayerGuillotina.enabled = true;
            videoPlayerGuillotina.Prepare();

            while (!videoPlayerGuillotina.isPrepared)
            {
                yield return null;
            }

            videoPlayerGuillotina.Play();
            yield return new WaitForSeconds(duracionVideoGuillotina);

            videoPlayerGuillotina.Stop();
            objetoVideoGuillotina.SetActive(false);
        }

        if (botonSaltarAnimacion != null) botonSaltarAnimacion.gameObject.SetActive(false);

        MostrarResultado(ultimaTirada);
        gachaCoroutine = null;
    }

    public void SkipearAnimacion()
    {
        if (gachaCoroutine != null)
        {
            StopCoroutine(gachaCoroutine);
            gachaCoroutine = null;

            if (videoPlayerGuillotina != null) videoPlayerGuillotina.Stop();
            if (objetoVideoGuillotina != null) objetoVideoGuillotina.SetActive(false);

            if (botonSaltarAnimacion != null) botonSaltarAnimacion.gameObject.SetActive(false);

            MostrarResultado(ultimaTirada);
        }
    }

    private void MostrarResultado(int indice)
    {
        if (imagenResultado != null && spritesPersonas[indice] != null)
        {
            imagenResultado.sprite = spritesPersonas[indice];
        }

        if (textoNombrePersona != null)
        {
            string nombre = (nombresPersonas.Length > indice && !string.IsNullOrEmpty(nombresPersonas[indice]))
                ? nombresPersonas[indice]
                : "Persona #" + (indice + 1);

            textoNombrePersona.text = "¡Has obtenido a:\n" + nombre + "!";
        }

        if (panelResultado != null)
        {
            panelResultado.SetActive(true);
        }
    }

    public void VolverAGacha()
    {
        if (panelResultado != null)
        {
            panelResultado.SetActive(false);
        }
        CambiarMenu(Menu.Gacha);
    }

    public void CerrarResultadoGacha()
    {
        VolverAGacha();
    }

    private void ComprobarDesbloqueoIgor()
    {
        bool todasConseguidas = true;
        for (int i = 0; i < personasDesbloqueadas.Length; i++)
        {
            if (!personasDesbloqueadas[i])
            {
                todasConseguidas = false;
                break;
            }
        }

        if (todasConseguidas)
        {
            igorDesbloqueado = true;
            Debug.Log("<color=cyan>¡Todas las Personas desbloqueadas! Igor está disponible.</color>");
        }
    }

    // ==========================================
    // MENÚ VER PERSONAS, LISTA Y EQUIPAR
    // ==========================================
    public void ver_personas()
    {
        CambiarMenu(Menu.VerPersonas);
        ActualizarVisualizacionPersonas();
    }

    public void SalirDeVerPersonas()
    {
        CambiarMenu(Menu.Gacha);
    }

    public void ActualizarVisualizacionPersonas()
    {
        ActualizarTextoEquipado();

        // 1. Actualizar las 10 Personas
        for (int i = 0; i < slotsPersonas.Length; i++)
        {
            bool estaDesbloqueada = (i < personasDesbloqueadas.Length && personasDesbloqueadas[i]);
            bool estaEquipada = (indiceEquipado == i);
            string nombreReal = (i < nombresPersonas.Length && !string.IsNullOrEmpty(nombresPersonas[i]))
                ? nombresPersonas[i]
                : "Persona " + (i + 1);

            if (slotsPersonas[i] != null)
            {
                slotsPersonas[i].sprite = estaDesbloqueada ? spritesPersonas[i] : spriteBloqueado;
            }

            if (textosNombresPersonas != null && i < textosNombresPersonas.Length && textosNombresPersonas[i] != null)
            {
                textosNombresPersonas[i].text = estaDesbloqueada ? nombreReal : "Locked";
            }

            if (botonesPersonas != null && i < botonesPersonas.Length && botonesPersonas[i] != null)
            {
                TextMeshProUGUI textoBoton = botonesPersonas[i].GetComponentInChildren<TextMeshProUGUI>();
                if (textoBoton != null)
                {
                    if (!estaDesbloqueada)
                    {
                        textoBoton.text = "Locked";
                    }
                    else if (estaEquipada)
                    {
                        textoBoton.text = "Equipped";
                    }
                    else
                    {
                        textoBoton.text = "Equip";
                    }
                }

                botonesPersonas[i].interactable = estaDesbloqueada && !estaEquipada;
            }
        }

        // 2. Slot de Igor
        bool igorEstaEquipado = (indiceEquipado == 99);

        if (slotIgor != null)
        {
            slotIgor.sprite = igorDesbloqueado ? spriteIgor : spriteSecreto;
        }

        if (textoNombreIgor != null)
        {
            textoNombreIgor.text = igorDesbloqueado ? nombreIgor : "Locked";
        }

        if (botonIgor != null)
        {
            TextMeshProUGUI textoBotonIgor = botonIgor.GetComponentInChildren<TextMeshProUGUI>();
            if (textoBotonIgor != null)
            {
                if (!igorDesbloqueado)
                {
                    textoBotonIgor.text = "Locked";
                }
                else if (igorEstaEquipado)
                {
                    textoBotonIgor.text = "Equipped";
                }
                else
                {
                    textoBotonIgor.text = "Equip";
                }
            }

            botonIgor.interactable = igorDesbloqueado && !igorEstaEquipado;
        }

        // 3. Botón de Reset a Adachi
        if (botonResetAdachi != null)
        {
            bool adachiEquipado = (indiceEquipado == -1);
            TextMeshProUGUI textoBotonReset = botonResetAdachi.GetComponentInChildren<TextMeshProUGUI>();
            if (textoBotonReset != null)
            {
                textoBotonReset.text = adachiEquipado ? "Equipped" : "Reset";
            }
            botonResetAdachi.interactable = !adachiEquipado;
        }
    }

    public void EquiparPersona(int indice)
    {
        if (indice >= 0 && indice < spritesPersonas.Length && personasDesbloqueadas[indice])
        {
            indiceEquipado = indice;
            spriteSeleccionado = spritesPersonas[indice];
            nombreSeleccionado = (nombresPersonas.Length > indice && !string.IsNullOrEmpty(nombresPersonas[indice]))
                ? nombresPersonas[indice]
                : "Persona #" + (indice + 1);

            ActualizarVisualizacionPersonas();
            Debug.Log("Equipado: " + nombreSeleccionado);
        }
    }

    public void EquiparIgor()
    {
        if (igorDesbloqueado)
        {
            indiceEquipado = 99;
            spriteSeleccionado = spriteIgor;
            nombreSeleccionado = nombreIgor;

            ActualizarVisualizacionPersonas();
            Debug.Log("Equipado: Igor");
        }
    }

    // FUNCIÓN PARA EL BOTÓN DE RESET A ADACHI
    public void ResetAAdachi()
    {
        indiceEquipado = -1;
        spriteSeleccionado = spriteAdachi;
        nombreSeleccionado = nombreAdachi;

        ActualizarVisualizacionPersonas();
        Debug.Log("Restablecido personaje por defecto: " + nombreAdachi);
    }

    private void ActualizarTextoEquipado()
    {
        if (textoEquipado != null)
        {
            textoEquipado.text = prefijoEquipado + nombreSeleccionado;
        }
    }

    // ==========================================
    // TRANSICIÓN VELVET ROOM
    // ==========================================
    private IEnumerator TransicionVelvetRoutine()
    {
        if (panelFundido1 != null) panelFundido1.SetActive(true);
        yield return null;

        if (animatorPanel1 != null) animatorPanel1.SetBool(boolFundidoAzul1, true);

        yield return new WaitForSeconds(duracionFundido1);

        if (objetoVideo != null) objetoVideo.SetActive(true);

        if (videoPlayer != null)
        {
            videoPlayer.enabled = true;
            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared)
            {
                yield return null;
            }
            videoPlayer.Play();
        }

        if (animatorPanel1 != null) animatorPanel1.SetBool(boolFundidoAzul1, false);
        if (panelFundido1 != null) panelFundido1.SetActive(false);

        float tiempoHastaPanel2 = Mathf.Max(0f, duracionVideo - anticipacionPanel2);
        yield return new WaitForSeconds(tiempoHastaPanel2);

        if (panelFundido2 != null) panelFundido2.SetActive(true);
        yield return null;

        if (animatorPanel2 != null) animatorPanel2.SetBool(boolFundidoAzul2, true);

        yield return new WaitForSeconds(anticipacionPanel2);

        if (videoPlayer != null) videoPlayer.Stop();
        if (objetoVideo != null) objetoVideo.SetActive(false);

        CambiarMenu(Menu.Velvet);

        yield return new WaitForSeconds(duracionFundido2);

        if (animatorPanel2 != null) animatorPanel2.SetBool(boolFundidoAzul2, false);
        if (panelFundido2 != null) panelFundido2.SetActive(false);

        transicionVelvetCoroutine = null;
    }
}