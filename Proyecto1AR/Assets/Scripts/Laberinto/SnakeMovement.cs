using UnityEngine;
using System.Collections.Generic;

public class SnakeMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 180f;

    [Header("Crecimiento")]
    public GameObject bodyPrefab; // El modelo para el cuerpo de la serpiente
    private List<Transform> bodyParts = new List<Transform>();

    private RandomWalkMaze mazeGenerator;

    void Start()
    {
        // Buscamos el generador del laberinto en la escena
        mazeGenerator = FindObjectOfType<RandomWalkMaze>();
    }

    void Update()
    {
        // Movimiento de tanque (Avanzar y Rotar)
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        float turnInput = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up * turnInput * turnSpeed * Time.deltaTime);

        // Hacer que el cuerpo siga a la cabeza
        MoveBody();
    }

    void MoveBody()
    {
        // Lógica básica para que cada segmento siga al anterior
        // (En 3D suele usarse Vector3.Lerp o guardar posiciones previas)
        if (bodyParts.Count > 0)
        {
            bodyParts[0].position = Vector3.Lerp(bodyParts[0].position, transform.position, Time.deltaTime * moveSpeed);
            for (int i = 1; i < bodyParts.Count; i++)
            {
                bodyParts[i].position = Vector3.Lerp(bodyParts[i].position, bodyParts[i - 1].position, Time.deltaTime * moveSpeed);
            }
        }
    }

    // CUANDO LA CABEZA TOCA ALGO
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Apple"))
        {
            Grow();
            // Le decimos al laberinto que mueva la manzana a otro lado
            mazeGenerator.SpawnApple();
        }
        else if (other.CompareTag("Wall") || other.CompareTag("Body"))
        {
            // Lógica de Game Over (chocar contra pared o cuerpo)
            Debug.Log("¡Game Over!");
        }
    }

    void Grow()
    {
        // Instanciar un nuevo segmento de cuerpo
        GameObject newBodyPart = Instantiate(bodyPrefab);

        // Posicionarlo detrás de la cabeza (o del último segmento)
        Vector3 spawnPosition = transform.position;
        if (bodyParts.Count > 0)
        {
            spawnPosition = bodyParts[bodyParts.Count - 1].position;
        }

        newBodyPart.transform.position = spawnPosition;
        bodyParts.Add(newBodyPart.transform);
    }

}