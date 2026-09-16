using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PruebaMov : MonoBehaviour
{

    private CharacterController controlador;
    private Vector3 direccion;
    public float VelocidadDelante;
    // Start is called before the first frame update
    void Start()
    {
        controlador = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        direccion.z = VelocidadDelante;
    }

    private void FixedUpdate()
    {
        controlador.Move(direccion * Time.deltaTime);
 
    }

}
