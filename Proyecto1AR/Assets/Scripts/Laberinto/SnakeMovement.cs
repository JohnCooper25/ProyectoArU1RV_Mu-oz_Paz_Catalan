using UnityEngine;
using System.Collections.Generic;

public class SnakeMovement : MonoBehaviour
{
    [Header("Referencias")]
    public RandomWalkMaze scriptLaberinto;

    [Header("Movimiento (2D - Ejes X e Y)")]
    public float moveSpeed = 5f;
    public float steerSpeed = 180f;

    [Header("Crecimiento del Cuerpo")]
    public GameObject bodyPrefab;
    public int gap = 10;

    private List<GameObject> bodyParts = new List<GameObject>();
    private List<Vector3> positionHistory = new List<Vector3>();
    private List<Quaternion> rotationHistory = new List<Quaternion>();

    private float turnInput;

    // Bandera para saber si perdimos
    private bool isDead = false;

    void Update()
    {
        if (isDead) return; // Si choca, ya no recibe controles
        turnInput = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // 1. AVANCE 2D: Se mueve hacia "arriba" en su propio eje Y
        transform.position += transform.up * moveSpeed * Time.fixedDeltaTime;

        // 2. GIRO 2D: Rota sobre el eje Z. 
        // Usamos negativo (-turnInput) para que al presionar la flecha derecha, 
        // gire en sentido horario (como es natural en 2D). Si lo notas invertido, quítale el signo menos.
        transform.Rotate(0, 0, -turnInput * steerSpeed * Time.fixedDeltaTime);

        positionHistory.Insert(0, transform.position);
        rotationHistory.Insert(0, transform.rotation);

        int index = 0;
        foreach (var body in bodyParts)
        {
            index += gap;

            if (index < positionHistory.Count)
            {
                body.transform.position = positionHistory[index];
                body.transform.rotation = rotationHistory[index];
            }
        }

        int limit = (bodyParts.Count + 1) * gap;
        if (positionHistory.Count > limit)
        {
            positionHistory.RemoveAt(positionHistory.Count - 1);
            rotationHistory.RemoveAt(rotationHistory.Count - 1);
        }
    }

    public void Grow()
    {
        GameObject body = Instantiate(bodyPrefab);

        // Es una buena práctica hacer que los bloques del cuerpo sean hijos del mismo contenedor
        body.transform.SetParent(transform.parent);

        // Ajustamos la escala del cuerpo para que coincida con la cabeza
        body.transform.localScale = transform.localScale;
        bodyParts.Add(body);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Comida"))
        {
            Debug.Log("¡ÑAM ÑAM! Comiste una manzana.");
            Grow();

            if (scriptLaberinto != null)
            {
                scriptLaberinto.SpawnApple();
            }
        }
        else if (other.CompareTag("Pared"))
        {
            Debug.Log("¡Choque con pared! Dando la vuelta...");

            // 3. REBOTE 2D: Darse la vuelta ahora significa rotar 180 grados en el eje Z
            transform.Rotate(0, 0, 180f);
        }
    }
}