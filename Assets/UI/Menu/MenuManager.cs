using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
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

    [Header("Ajustes - Sliders de Volumen")]
    public Slider sliderMaster;
    public Slider sliderMusica;
    public Slider sliderSFX;

    [Header("Menú Principal - Visualización Personaje")]
    public Image imagenPersonajeMainMenu;

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

    public static Sprite spriteSeleccionado;
    public static string nombreSeleccionado = "Adachi";
    public static int indiceEquipado = -1;

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

        CargarDatosGuardados();
        ConfigurarClicksDeBotones();
        ActualizarTextoEquipado();

        // Inicializar sliders con el valor que tengan guardado
        SincronizarSlidersVolumen();

        CambiarMenu(Menu.Main);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusicaMenu();
        }
    }

    // ==========================================
    // SINCRONIZACIÓN DE SLIDERS DE AJUSTES
    // ==========================================
    private void SincronizarSlidersVolumen()
    {
        if (sliderMaster != null)
        {
            float val = PlayerPrefs.GetFloat("VolumenMaster", 1f);
            sliderMaster.SetValueWithoutNotify(val);
            sliderMaster.onValueChanged.RemoveAllListeners();
            sliderMaster.onValueChanged.AddListener((v) => {
                if (AudioManager.Instance != null) AudioManager.Instance.SetMasterVolume(v);
            });
        }

        if (sliderMusica != null)
        {
            float val = PlayerPrefs.GetFloat("VolumenMusic", 1f);
            sliderMusica.SetValueWithoutNotify(val);
            sliderMusica.onValueChanged.RemoveAllListeners();
            sliderMusica.onValueChanged.AddListener((v) => {
                if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(v);
            });
        }

        if (sliderSFX != null)
        {
            float val = PlayerPrefs.GetFloat("VolumenSFX", 1f);
            sliderSFX.SetValueWithoutNotify(val);
            sliderSFX.onValueChanged.RemoveAllListeners();
            sliderSFX.onValueChanged.AddListener((v) => {
                if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(v);
            });
        }
    }

    private void CargarDatosGuardados()
    {
        for (int i = 0; i < personasDesbloqueadas.Length; i++)
        {
            personasDesbloqueadas[i] = (PlayerPrefs.GetInt(PREF_PERSONA_PREFIX + i, 0) == 1);
        }

        igorDesbloqueado = (PlayerPrefs.GetInt(PREF_IGOR, 0) == 1);
        ComprobarDesbloqueoIgor();

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

    [ContextMenu("Resetear Todo el Juego (PlayerPrefs)")]
    public void ResetGame()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayResetearJuego();
        }

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // Devolver las barras de volumen al 100% y aplicarlo
        if (sliderMaster != null) sliderMaster.SetValueWithoutNotify(1f);
        if (sliderMusica != null) sliderMusica.SetValueWithoutNotify(1f);
        if (sliderSFX != null) sliderSFX.SetValueWithoutNotify(1f);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(1f);
            AudioManager.Instance.SetMusicVolume(1f);
            AudioManager.Instance.SetSFXVolume(1f);
        }

        for (int i = 0; i < personasDesbloqueadas.Length; i++)
        {
            personasDesbloqueadas[i] = false;
        }
        igorDesbloqueado = false;

        ResetAAdachi();
        ActualizarVisualizacionPersonas();
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
        if (AudioManager.Instance != null) AudioManager.Instance.PlayBotonPlay();
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    public void IrAMain()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBotonMenu();
            AudioManager.Instance.PlayMusicaMenu();
        }
        CambiarMenu(Menu.Main);
    }

    public void IrAConfig()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayBotonMenu();

        // Asegura que las barras reflejen la posición actual al abrir Ajustes
        SincronizarSlidersVolumen();

        CambiarMenu(Menu.Config);
    }

    public void IrAGacha()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayBotonMenu();
        CambiarMenu(Menu.Gacha);
    }

    public void SalirDeGachaAVelvet()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayBotonMenu();
        CambiarMenu(Menu.Velvet);
    }

    public void SalirDeGachaAMain()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBotonMenu();
            AudioManager.Instance.PlayMusicaMenu();
        }
        CambiarMenu(Menu.Main);
    }

    public void IrAVelvet()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayBotonMenu();

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

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTirarGacha();
        }

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
            yield return null;

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
        if (AudioManager.Instance != null) AudioManager.Instance.PlayBotonMenu();

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
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayResultadoGacha();
        }

        if (imagenResultado != null && spritesPersonas[indice] != null)
        {
            imagenResultado.sprite = spritesPersonas[indice];
        }

        if (textoNombrePersona != null)
        {
            string nombre = (nombresPersonas.Length > indice && !string.IsNullOrEmpty(nombresPersonas[indice]))
                ? nombresPersonas[indice]
                : "Persona #" + (indice + 1);

            textoNombrePersona.text = "You just got a \n" + nombre + "!";
        }

        if (panelResultado != null)
        {
            panelResultado.SetActive(true);
        }
    }

    public void VolverAGacha()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayBotonMenu();

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
        }
    }

    // ==========================================
    // MENÚ VER PERSONAS, LISTA Y EQUIPAR
    // ==========================================
    public void ver_personas()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayBotonMenu();
        CambiarMenu(Menu.VerPersonas);
        ActualizarVisualizacionPersonas();
    }

    public void SalirDeVerPersonas()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayBotonMenu();
        CambiarMenu(Menu.Gacha);
    }

    public void ActualizarVisualizacionPersonas()
    {
        ActualizarTextoEquipado();

        for (int i = 0; i < slotsPersonas.Length; i++)
        {
            bool estaDesbloqueada = (i < personasDesbloqueadas.Length && personasDesbloqueadas[i]);
            bool estaEquipada = (indiceEquipado == i);
            string nombreReal = (i < nombresPersonas.Length && !string.IsNullOrEmpty(nombresPersonas[indiceEquipado >= 0 ? i : 0]))
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
                    if (!estaDesbloqueada) textoBoton.text = "Locked";
                    else if (estaEquipada) textoBoton.text = "Equipped";
                    else textoBoton.text = "Equip";
                }

                botonesPersonas[i].interactable = estaDesbloqueada && !estaEquipada;
            }
        }

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
                if (!igorDesbloqueado) textoBotonIgor.text = "Locked";
                else if (igorEstaEquipado) textoBotonIgor.text = "Equipped";
                else textoBotonIgor.text = "Equip";
            }

            botonIgor.interactable = igorDesbloqueado && !igorEstaEquipado;
        }

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
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEquiparPersona();
            }

            indiceEquipado = indice;
            PlayerPrefs.SetInt(PREF_EQUIPADO, indiceEquipado);
            PlayerPrefs.Save();

            spriteSeleccionado = spritesPersonas[indice];
            nombreSeleccionado = (nombresPersonas.Length > indice && !string.IsNullOrEmpty(nombresPersonas[indice]))
                ? nombresPersonas[indice]
                : "Persona #" + (indice + 1);

            ActualizarVisualizacionPersonas();
        }
    }

    public void EquiparIgor()
    {
        if (igorDesbloqueado)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEquiparPersona();
            }

            indiceEquipado = 99;
            PlayerPrefs.SetInt(PREF_EQUIPADO, 99);
            PlayerPrefs.Save();

            spriteSeleccionado = spriteIgor;
            nombreSeleccionado = nombreIgor;

            ActualizarVisualizacionPersonas();
        }
    }

    public void ResetAAdachi()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEquiparPersona();
        }

        indiceEquipado = -1;
        PlayerPrefs.SetInt(PREF_EQUIPADO, -1);
        PlayerPrefs.Save();

        spriteSeleccionado = spriteAdachi;
        nombreSeleccionado = nombreAdachi;

        ActualizarVisualizacionPersonas();
    }

    private void ActualizarTextoEquipado()
    {
        if (textoEquipado != null)
        {
            textoEquipado.text = prefijoEquipado + nombreSeleccionado;
        }

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

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusicaVelvet();
        }

        CambiarMenu(Menu.Velvet);

        yield return new WaitForSeconds(duracionFundido2);

        if (animatorPanel2 != null) animatorPanel2.SetBool(boolFundidoAzul2, false);
        if (panelFundido2 != null) panelFundido2.SetActive(false);

        transicionVelvetCoroutine = null;
    }
}