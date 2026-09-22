using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] private PlayerMovement PlayerScript;
    [SerializeField] private string nombreEscenaMenu = "SampleScene"; // Escribe aquí el nombre exacto de la escena de tus menús

    public void Restart_Level()
    {
        Time.timeScale = 1f; // Descongelar el juego antes de recargar
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Main_Menu()
    {
        Time.timeScale = 1f; // Descongelar el juego antes de volver al menú
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    public void Escudo()
    {
        Debug.Log("boton pulsado");
        if (PlayerScript != null)
        {
            PlayerScript.ActivateShield();
        }
    }
}