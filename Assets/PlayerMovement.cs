using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private GameObject Adachi;
    public enum Carriles
    {
       izquierdo = 1,
       centro,
       derecha,
    }
    public int Posicion;
            
    void Start()
    {
        
    }
    void Update()
    {
        CarrilActual();
        Inputs();
    }

    private void Inputs()
    {
        //para cambiar de carril A - D
        if (Input.GetKeyDown(KeyCode.A))
        {

        }
        if (Input.GetKeyDown(KeyCode.D))
        {

        }
    }
    private void CarrilActual()
    {
     
    }
}
