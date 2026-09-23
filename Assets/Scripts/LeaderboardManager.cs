using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using LootLocker.Requests;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance;

    [Header("Configuración LootLocker")]
    public string leaderboardID;

    [Header("UI del High Score")]
    public GameObject Menu_HighScore;
    public TextMeshProUGUI Texto_Timer;
    public GameObject Grupo_InputName;
    public TMP_InputField Input_Name;
    public GameObject Texto_AvisoNick;
    public GameObject Boton_ChangeNick; // El botón para cambiar de nombre

    [Header("Conexión con UI Normal")]
    public GameObject Death_UI;
    public GameObject Basic_UI;

    private int scorePendiente = 0;
    private const string PREF_HIGHSCORE = "MiMejorScore";
    private const string PREF_PLAYERNAME = "MiNombreOnline";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (response.success)
            {
                Debug.Log("Conectada a LootLocker con éxito");
            }
            else
            {
                Debug.LogWarning("Error al iniciar sesión en LootLocker: " + response.errorData?.message);
            }
        });
    }

    public void ComprobarPuntuacion(int timerActual)
    {
        int miMejorRecord = PlayerPrefs.GetInt(PREF_HIGHSCORE, 0);

        if (timerActual > miMejorRecord)
        {
            LootLockerSDKManager.GetScoreList(leaderboardID, 10, 0, (response) =>
            {
                if (response.success)
                {
                    bool entraEnTop10 = false;
                    LootLockerLeaderboardMember[] top10 = response.items;

                    if (top10 == null || top10.Length < 10)
                    {
                        entraEnTop10 = true;
                    }
                    else
                    {
                        int puntuacionCorte = top10[top10.Length - 1].score;
                        if (timerActual > puntuacionCorte)
                        {
                            entraEnTop10 = true;
                        }
                    }

                    if (entraEnTop10)
                    {
                        PlayerPrefs.SetInt(PREF_HIGHSCORE, timerActual);
                        MostrarMenuHighScore(timerActual);
                    }
                    else
                    {
                        TerminarConMuerteNormal();
                    }
                }
                else
                {
                    TerminarConMuerteNormal();
                }
            });
        }
        else
        {
            TerminarConMuerteNormal();
        }
    }

    private void MostrarMenuHighScore(int score)
    {
        scorePendiente = score;

        if (Texto_Timer != null)
        {
            Texto_Timer.text = "Timer: " + scorePendiente;
        }

        if (Texto_AvisoNick != null)
        {
            Texto_AvisoNick.SetActive(false);
        }

        // Si ya tiene un nombre registrado previamente
        if (PlayerPrefs.HasKey(PREF_PLAYERNAME))
        {
            if (Grupo_InputName != null) Grupo_InputName.SetActive(false);
            if (Boton_ChangeNick != null) Boton_ChangeNick.SetActive(true);
        }
        else
        {
            // Primera vez: el campo de nombre se abre directo,
            // por lo que el botón de Change Nick se oculta de inmediato
            if (Grupo_InputName != null) Grupo_InputName.SetActive(true);
            if (Boton_ChangeNick != null) Boton_ChangeNick.SetActive(false);
        }

        if (Basic_UI != null) Basic_UI.SetActive(false);
        if (Menu_HighScore != null) Menu_HighScore.SetActive(true);
    }

    // ========================================================
    // MÉTODO PARA ABRIR EL MENÚ DE NICK DESDE EL BOTÓN
    // ========================================================
    public void Btn_AbrirCambiarNick()
    {
        // 1. Ocultamos el botón de Change Nick al abrir el menú de edición
        if (Boton_ChangeNick != null)
        {
            Boton_ChangeNick.SetActive(false);
        }

        // 2. Si el botón está fuera (por ejemplo en Death_UI), abre Menu_HighScore
        if (Menu_HighScore != null)
        {
            Menu_HighScore.SetActive(true);
        }

        // 3. Muestra el campo de texto con el nick que ya tenía escrito
        if (Grupo_InputName != null)
        {
            Grupo_InputName.SetActive(true);

            if (Input_Name != null)
            {
                Input_Name.text = PlayerPrefs.GetString(PREF_PLAYERNAME, "");
            }
        }

        if (Texto_AvisoNick != null)
        {
            Texto_AvisoNick.SetActive(false);
        }
    }

    public void Btn_UploadHighScore()
    {
        // Si el campo de escribir nombre está activo en pantalla
        if (Grupo_InputName != null && Grupo_InputName.activeSelf)
        {
            string nombreJugadora = Input_Name != null ? Input_Name.text.Trim() : "";

            if (string.IsNullOrEmpty(nombreJugadora))
            {
                if (Texto_AvisoNick != null)
                {
                    Texto_AvisoNick.SetActive(true);
                }
                return;
            }

            if (Texto_AvisoNick != null)
            {
                Texto_AvisoNick.SetActive(false);
            }

            PlayerPrefs.SetString(PREF_PLAYERNAME, nombreJugadora);
            PlayerPrefs.Save();

            LootLockerSDKManager.SetPlayerName(nombreJugadora, (response) =>
            {
                SubirPuntos();
            });
        }
        else
        {
            SubirPuntos();
        }
    }

    private void SubirPuntos()
    {
        if (scorePendiente > 0)
        {
            LootLockerSDKManager.SubmitScore("", scorePendiente, leaderboardID, (response) =>
            {
                CerrarMenuHighScore();
            });
        }
        else
        {
            CerrarMenuHighScore();
        }
    }

    public void Btn_DontUpload()
    {
        CerrarMenuHighScore();
    }

    private void CerrarMenuHighScore()
    {
        if (Menu_HighScore != null) Menu_HighScore.SetActive(false);
        TerminarConMuerteNormal();
    }

    private void TerminarConMuerteNormal()
    {
        if (Death_UI != null) Death_UI.SetActive(true);
        if (Basic_UI != null) Basic_UI.SetActive(true);

        // Si el botón de Change Nick está colocado dentro de Death_UI, se reactiva aquí
        if (Boton_ChangeNick != null && Death_UI != null && Boton_ChangeNick.transform.IsChildOf(Death_UI.transform))
        {
            Boton_ChangeNick.SetActive(true);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
            AudioManager.Instance.PlayPerder();
        }
    }
}