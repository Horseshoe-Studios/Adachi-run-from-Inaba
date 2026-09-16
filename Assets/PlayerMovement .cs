using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //intentar cambiar el movimiento entre carriles para que no dependa de una resolución fija

    [SerializeField] private GameObject Adachi;
    [SerializeField] private GameObject Carril_1;
    [SerializeField] private GameObject Carril_2;
    [SerializeField] private GameObject Carril_3;
    public enum Carriles
    {
       izquierdo = 1,
       centro,
       derecha,
    }
    [SerializeField] private int vidas = 3;
    public Carriles PosicionActual = Carriles.centro;
    public int Carril;
    [SerializeField] private bool Shield_Active = false; //serialize temporal
    private bool Shield_Can_Active = true;
    [SerializeField] private float Shield_Cooldown = 6f;
    [SerializeField] private float Shield_Duration = 2f;

    void Start()
    {
        
    }
    void Update()
    {
        CarrilActual();
        Inputs();
        Posicionamiento_Jugador();
        TakeDamage();
    }

    private void Inputs()
    {
        //para cambiar de carril A - D
        if (Input.GetKeyDown(KeyCode.A) && PosicionActual != Carriles.izquierdo)
        {
            Carril--;
        }
        if (Input.GetKeyDown(KeyCode.D) && PosicionActual != Carriles.derecha)
        {
            Carril++;
        }
        if (Input.GetKeyDown(KeyCode.R) && !Shield_Active && Shield_Can_Active)
        {
            Shield_Active = true;
            Shield_Can_Active = false;
            StartCoroutine(Shield());
        }
    }
    private void CarrilActual()
    {
        if (PosicionActual == Carriles.izquierdo)
        {
            Carril = (int)PosicionActual;
        }
        else if (PosicionActual == Carriles.centro)
        {
            Carril = (int)PosicionActual;
        }
        else if (PosicionActual == Carriles.derecha)
        {
            Carril = (int)PosicionActual;
        }
    }
    private void Posicionamiento_Jugador()
    {
        if (Carril == 1)
        {
            PosicionActual = Carriles.izquierdo;
            Adachi.transform.position = Carril_1.transform.position;
        }
        else if (Carril == 2)
        {
            PosicionActual = Carriles.centro;
            Adachi.transform.position = Carril_2.transform.position;
        }
        else if (Carril == 3)
        {
            PosicionActual = Carriles.derecha;
            Adachi.transform.position = Carril_3.transform.position;
        }
    }
    private void TakeDamage()
    {
        if (Shield_Active) return;
        vidas--;
    }
    private IEnumerator Shield()
    {
        yield return new WaitForSeconds(Shield_Duration);
        Shield_Active = false;
        yield return new WaitForSeconds(Shield_Cooldown);
        Shield_Can_Active = true;
    }
}
