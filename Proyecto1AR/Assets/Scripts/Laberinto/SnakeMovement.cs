using UnityEngine;
using System.Collections.Generic;

public class SnakeMovement : MonoBehaviour
{
    [Header("Referencias")]
    public RandomWalkMaze scriptLaberinto;

    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public float steerSpeed = 180f;

    [Header("Crecimiento del Cuerpo")]
    public GameObject bodyPrefab;
    public int gap = 10;

    private List<GameObject> bodyParts = new List<GameObject>();
    private List<Vector3> positionHistory = new List<Vector3>();
    private List<Quaternion> rotationHistory = new List<Quaternion>();

    private float turnInput;

    // NUEVO: Bandera para saber si perdimos
    private bool isDead = false;

    void Update()
    {
        if (isDead) return; // Si choca, ya no recibe controles
        turnInput = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
        if (isDead) return; // NUEVO: Detiene el avance de la serpiente

        transform.position += transform.forward * moveSpeed * Time.fixedDeltaTime;
        transform.Rotate(Vector3.up * turnInput * steerSpeed * Time.fixedDeltaTime);

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

            // Rota exactamente 180 grados en el eje Y (dirección opuesta)
            transform.Rotate(0, 180f, 0);
        }
    }
}