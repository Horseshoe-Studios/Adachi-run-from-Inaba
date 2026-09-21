using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] private PlayerMovement PlayerScript;

    public void Restart_Level()
    {
        Debug.Log("Poner la misma escena");
    }
    public void Main_Menu()
    {
        Debug.Log("Ir al menú");
    }
    public void Escudo()
    {
        Debug.Log("boton pulsado");
       PlayerScript.ActivateShield();
    }
}
