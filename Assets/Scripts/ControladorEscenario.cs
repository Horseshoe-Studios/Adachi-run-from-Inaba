using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControladorEscenario : MonoBehaviour
{
    public GameObject[] Prefabs;
    public Transform Jugador;
    public int numeroCamino = 5;
    public float DistanciaGeneracion = 2000;
    private float SpawnZ = 0;
    void Start()
    {        for (int i = 0; i < numeroCamino; i++)
        {
            if (i == 0)
            {
                SpawnCamino(0);
            }
            else
            {
                SpawnCamino(Random.Range(0, Prefabs.Length));
            }
        }
    }

    void Update()
    {
        float distanciaAlFinal = SpawnZ - Jugador.position.z;

        if (distanciaAlFinal < DistanciaGeneracion)
        {
            SpawnCamino(Random.Range(0, Prefabs.Length));
        }
    }

    public void SpawnCamino(int IndexCamino)
    {
        GameObject camino = Instantiate(
            Prefabs[IndexCamino],
            new Vector3(0, 0, SpawnZ),
            transform.rotation
        );
        BoxCollider collider = camino.GetComponent<BoxCollider>();

        if (collider != null)
        {
            float largo = collider.size.z * camino.transform.localScale.z;
            SpawnZ += largo;
        }
    }
}