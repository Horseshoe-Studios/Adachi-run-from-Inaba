using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControladorEscenario : MonoBehaviour
{
    [Header("Prefabs y Jugador")]
    public GameObject[] Prefabs;
    public Transform Jugador;

    [Header("Generación")]
    public int numeroCamino = 5;
    public float DistanciaGeneracion = 2000f;

    [Header("Optimización (Destrucción)")]
    [Tooltip("Distancia detrás del jugador a la que debe quedar el FINAL del tramo para borrarse")]
    public float distanciaBorrado = 800f; // Margen amplio para que nunca se vea el corte

    private float SpawnZ = 0;

    // Estructura interna para recordar el GameObject y dónde termina exactamente
    private class TramoCamino
    {
        public GameObject objeto;
        public float finZ;

        public TramoCamino(GameObject obj, float fin)
        {
            objeto = obj;
            finZ = fin;
        }
    }

    private List<TramoCamino> caminosActivos = new List<TramoCamino>();

    void Start()
    {
        for (int i = 0; i < numeroCamino; i++)
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
        if (Jugador == null) return;

        // 1. Generar camino hacia adelante
        float distanciaAlFinal = SpawnZ - Jugador.position.z;

        if (distanciaAlFinal < DistanciaGeneracion)
        {
            SpawnCamino(Random.Range(0, Prefabs.Length));
        }

        // 2. Destruir tramos que hayan quedado muy atrás de la cámara
        BorrarCaminosViejos();
    }

    public void SpawnCamino(int IndexCamino)
    {
        if (Prefabs == null || Prefabs.Length == 0) return;

        GameObject camino = Instantiate(
            Prefabs[IndexCamino],
            new Vector3(0, 0, SpawnZ),
            transform.rotation
        );

        BoxCollider collider = camino.GetComponent<BoxCollider>();
        float largo = 50f;

        if (collider != null)
        {
            largo = collider.size.z * camino.transform.localScale.z;
        }

        SpawnZ += largo;

        // Guardamos el objeto junto al punto exacto donde TERMINA
        caminosActivos.Add(new TramoCamino(camino, SpawnZ));
    }

    private void BorrarCaminosViejos()
    {
        while (caminosActivos.Count > 0)
        {
            TramoCamino tramoViejo = caminosActivos[0];

            // Comprobamos si el FINAL del bloque ya quedó a más de 'distanciaBorrado' metros por detrás
            if (Jugador.position.z - tramoViejo.finZ > distanciaBorrado)
            {
                if (tramoViejo.objeto != null)
                {
                    Destroy(tramoViejo.objeto);
                }
                caminosActivos.RemoveAt(0);
            }
            else
            {
                // Si el final de este tramo todavía está cerca, no borramos nada más
                break;
            }
        }
    }
}