using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //intentar cambiar el movimiento entre carriles para que no dependa de una resolución fija

    [SerializeField] private GameObject Adachi;
 
    public enum Carriles
    {
       izquierdo = 1,
       centro,
       derecha,
    }

    [SerializeField] private int vidas = 3;

    [SerializeField] private float separacionCarriles = 2.5f; // distancia en X entre carriles
    [SerializeField] private float velocidadCambioCarril = 15f;

    public Carriles PosicionActual = Carriles.centro;
    public int Carril;

    [SerializeField] private bool Shield_Active = false; //serialize temporal
    private bool Shield_Can_Active = true;
    [SerializeField] private float Shield_Cooldown = 6f;
    [SerializeField] private float Shield_Duration = 2f;

    private float xCentro; // posicion X inicial del jugador, se usa como referencia
    private Vector3 posicionObjetivo;

    void Start()
    {
        xCentro = Adachi.transform.position.x;
        posicionObjetivo = Adachi.transform.position;
    }
    void Update()
    {
        Inputs();
        Posicionamiento_Jugador();
        MoverHaciaCarril();
    }

    private void Inputs()
    {
        //para cambiar de carril A - D
        if (Input.GetKeyDown(KeyCode.A) && Carril > 1)
        {
            Carril--;
        }
        if (Input.GetKeyDown(KeyCode.D) && Carril < 3)
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
    private void Posicionamiento_Jugador()
    {
        float offsetX = 0f;

        switch (Carril)
        {
            case 1:
                PosicionActual = Carriles.izquierdo;
                offsetX = -separacionCarriles;
                break;
            case 2:
                PosicionActual = Carriles.centro;
                offsetX = 0f;
                break;
            case 3:
                PosicionActual = Carriles.derecha;
                offsetX = separacionCarriles;
                break;
        }

        // Solo cambia X; Y y Z los deja como estén (útil si el jugador salta o la pista se mueve en Z)
        posicionObjetivo = new Vector3(xCentro + offsetX, Adachi.transform.position.y, Adachi.transform.position.z);
    }

    private void MoverHaciaCarril()
    {
        Adachi.transform.position = Vector3.MoveTowards(Adachi.transform.position, posicionObjetivo, velocidadCambioCarril * Time.deltaTime);
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
