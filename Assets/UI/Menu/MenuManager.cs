using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    // Claves de PlayerPrefs para persistencia
    private const string PREF_PERSONA_PREFIX = "PersonaDesbloqueada_";
    private const string PREF_IGOR = "IgorDesbloqueado";
    private const string PREF_EQUIPADO = "IndiceEquipado";
    private const string PREF_TOKENS = "TokensGuardados";

    [Header("Menús")]
    public GameObject mainMenu;
    public GameObject velvet;
    public GameObject config;
    public GameObject gachaMenu;
    public GameObject menuVerPersonas;

    [Header("Menú Principal - Visualización Personaje")]
    public Image imagenPersonajeMainMenu;           // Arrastra aquí la Image del menú principal

    [Header("Configuración Escena de Juego")]
    public string nombreEscenaJuego = "GameScene";

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
    public Sprite spriteAdachi;
    public string nombreAdachi = "Adachi";
    public Button botonResetAdachi;

    [Header("Menú Ver Personas - UI General")]
    public TextMeshProUGUI textoEquipado;
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

    // Variables estáticas para pasar los datos entre escenas
    public static Sprite spriteSeleccionado;
    public static string nombreSeleccionado = "Adachi";
    public static int indiceEquipado = -1; // -1 = Adachi, 0-9 = Personas, 99 = Igor

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

        // 1. CARGAR DATOS GUARDADOS DESDE DISCO
        CargarDatosGuardados();

        ConfigurarClicksDeBotones();
        ActualizarTextoEquipado();
        CambiarMenu(Menu.Main);
    }

    // ==========================================
    // PERSISTENCIA Y CARGA DE DATOS
    // ==========================================
    private void CargarDatosGuardados()
    {
        // Cargar desbloqueos de las 10 Personas
        for (int i = 0; i < personasDesbloqueadas.Length; i++)
        {
            personasDesbloqueadas[i] = (PlayerPrefs.GetInt(PREF_PERSONA_PREFIX + i, 0) == 1);
        }

        // Cargar desbloqueo de Igor
        igorDesbloqueado = (PlayerPrefs.GetInt(PREF_IGOR, 0) == 1);
        ComprobarDesbloqueoIgor();

        // Cargar qué personaje estaba equipado
        indiceEquipado = PlayerPrefs.GetInt(PREF_EQUIPADO, -1);

        if (indiceEquipado >= 0 && indiceEquipado < spritesPersonas.Length && personasDesbloqueadas[indiceEquipado])
        {
            spriteSeleccionado = spritesPersonas[indiceEquipado];
            nombreSeleccionado = (nombresPersonas.Length > indiceEquipado && !string.IsNullOrEmpty(nombresPersonas[indiceEquipado]))
                ? nombresPersonas[indiceEquipado]
                : "Persona #" + (indiceEquipado + 1);
        }
        else if (indiceEquipado == 99 && igorDesbloqueado)
        {
            spriteSeleccionado = spriteIgor;
            nombreSeleccionado = nombreIgor;
        }
        else
        {
            indiceEquipado = -1;
            spriteSeleccionado = spriteAdachi;
            nombreSeleccionado = nombreAdachi;
        }
    }

    // FUNCIÓN PARA EL BOTÓN DE RESETEAR TODO EL JUEGO
    [ContextMenu("Resetear Todo el Juego (PlayerPrefs)")]
    public void ResetGame()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        for (int i = 0; i < personasDesbloqueadas.Length; i++)
        {
            personasDesbloqueadas[i] = false;
        }
        igorDesbloqueado = false;

        ResetAAdachi();
        ActualizarVisualizacionPersonas();

        Debug.Log("<color=yellow>¡Partida reseteada con éxito! Monedas a 0 y personajes bloqueados.</color>");
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
    // NAVEGACIÓN Y CAMBIO DE ESCENA
    // ==========================================
    public void jugar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscenaJuego);
    }

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
        PlayerPrefs.SetInt(PREF_PERSONA_PREFIX + ultimaTirada, 1);
        PlayerPrefs.Save();

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

            textoNombrePersona.text = "You just got a:\n" + nombre + "!";
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
            PlayerPrefs.SetInt(PREF_IGOR, 1);
            PlayerPrefs.Save();
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
            PlayerPrefs.SetInt(PREF_EQUIPADO, indiceEquipado);
            PlayerPrefs.Save();

            spriteSeleccionado = spritesPersonas[indice];
            nombreSeleccionado = (nombresPersonas.Length > indice && !string.IsNullOrEmpty(nombresPersonas[indice]))
                ? nombresPersonas[indice]
                : "Persona #" + (indice + 1);

            ActualizarVisualizacionPersonas();
            Debug.Log("Equipado y guardado: " + nombreSeleccionado);
        }
    }

    public void EquiparIgor()
    {
        if (igorDesbloqueado)
        {
            indiceEquipado = 99;
            PlayerPrefs.SetInt(PREF_EQUIPADO, 99);
            PlayerPrefs.Save();

            spriteSeleccionado = spriteIgor;
            nombreSeleccionado = nombreIgor;

            ActualizarVisualizacionPersonas();
            Debug.Log("Equipado y guardado: Igor");
        }
    }

    public void ResetAAdachi()
    {
        indiceEquipado = -1;
        PlayerPrefs.SetInt(PREF_EQUIPADO, -1);
        PlayerPrefs.Save();

        spriteSeleccionado = spriteAdachi;
        nombreSeleccionado = nombreAdachi;

        ActualizarVisualizacionPersonas();
        Debug.Log("Restablecido y guardado por defecto: " + nombreAdachi);
    }

    // Actualiza tanto el texto como la imagen del personaje en el menú principal
    private void ActualizarTextoEquipado()
    {
        if (textoEquipado != null)
        {
            textoEquipado.text = prefijoEquipado + nombreSeleccionado;
        }

        // Muestra el sprite del personaje equipado en el menú principal
        if (imagenPersonajeMainMenu != null && spriteSeleccionado != null)
        {
            imagenPersonajeMainMenu.sprite = spriteSeleccionado;
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