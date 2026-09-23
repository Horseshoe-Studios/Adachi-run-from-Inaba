using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using LootLocker.Requests;

public class LeaderboardDisplay : MonoBehaviour
{
    [Header("Configuración LootLocker")]
    public string leaderboardID;

    [Header("Panel Principal")]
    public GameObject panelLeaderboard;
    public TextMeshProUGUI textoCargando;

    [Header("Columnas de Texto")]
    public TextMeshProUGUI textoRanks;
    public TextMeshProUGUI textoNombres;
    public TextMeshProUGUI textoScores;

    private void Start()
    {
        // Conexión como invitado por si la jugadora entra directo al menú principal
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (response.success)
            {
                Debug.Log("LootLocker conectado en el menú principal");
            }
        });
    }

    // Llama a este método con el botón de "Ranking" del Menú Principal
    public void AbrirLeaderboard()
    {
        panelLeaderboard.SetActive(true);

        // Limpiamos los textos y mostramos el mensaje de carga
        textoRanks.text = "";
        textoNombres.text = "";
        textoScores.text = "";

        if (textoCargando != null)
        {
            textoCargando.gameObject.SetActive(true);
            textoCargando.text = "Loading...";
        }

        // Pedimos los 10 mejores registros
        LootLockerSDKManager.GetScoreList(leaderboardID, 10, 0, (response) =>
        {
            if (response.success)
            {
                LootLockerLeaderboardMember[] items = response.items;

                if (items == null || items.Length == 0)
                {
                    if (textoCargando != null)
                    {
                        textoCargando.text = "No scores yet!";
                    }
                    return;
                }

                string ranksCadena = "";
                string nombresCadena = "";
                string scoresCadena = "";

                for (int i = 0; i < items.Length; i++)
                {
                    // Puesto (1., 2., etc.)
                    ranksCadena += items[i].rank + ".\n";

                    // Nombre del jugador
                    string nombre = items[i].player.name;
                    if (string.IsNullOrEmpty(nombre))
                    {
                        nombre = "Player " + items[i].player.id;
                    }
                    nombresCadena += nombre + "\n";

                    // Puntuación / Timer
                    scoresCadena += items[i].score + "s\n";
                }

                // Ocultamos el texto de carga y mostramos los datos
                if (textoCargando != null)
                {
                    textoCargando.gameObject.SetActive(false);
                }

                textoRanks.text = ranksCadena;
                textoNombres.text = nombresCadena;
                textoScores.text = scoresCadena;
            }
            else
            {
                if (textoCargando != null)
                {
                    textoCargando.text = "Failed to load leaderboard.";
                }
            }
        });
    }

    // Llama a este método con el botón de "Cerrar / Back"
    public void CerrarLeaderboard()
    {
        panelLeaderboard.SetActive(false);
    }
}