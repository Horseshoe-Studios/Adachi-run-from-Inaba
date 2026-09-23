using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target; // Arrastra aquí a adachi (o Jugador)

    [Header("Distancia (Offset)")]
    public Vector3 offset = new Vector3(0f, 8.5f, -18.2f); // Tus valores exactos

    [Header("Suavizado")]
    public float suavizadoX = 12f;      // Fluidez al cambiar de carril
    public bool seguirSaltoEnY = false; // Desmarcado para que la cámara no suba y baje al saltar

    private float alturaFijaY;

    void Start()
    {
        if (target != null)
        {
            alturaFijaY = target.position.y + offset.y;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Sigue suavemente los cambios de carril laterales
        float posX = Mathf.Lerp(transform.position.x, target.position.x + offset.x, suavizadoX * Time.deltaTime);

        // 2. Altura: fija para no marear con el salto
        float posY = seguirSaltoEnY ? (target.position.y + offset.y) : alturaFijaY;

        // 3. Sigue el avance hacia delante en Z sin retraso
        float posZ = target.position.z + offset.z;

        transform.position = new Vector3(posX, posY, posZ);
    }
}