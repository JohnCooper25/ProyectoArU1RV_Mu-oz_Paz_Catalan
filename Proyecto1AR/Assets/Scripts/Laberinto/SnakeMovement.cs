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
    private bool isDead = false;
    private Quaternion rotacionInicial;

    void Awake()
    {
        rotacionInicial = transform.rotation;
    }

    void Update()
    {
        if (isDead) return;
        turnInput = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
        if (isDead) return;

        transform.position += transform.up * moveSpeed * Time.fixedDeltaTime;
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
        body.transform.SetParent(transform.parent);
        body.transform.localScale = transform.localScale;
        bodyParts.Add(body);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

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
            Morir();
        }
    }

    void Morir()
    {
        isDead = true;
        Debug.Log("¡Choque con pared! Generando un mapa nuevo...");

        ResetearSerpiente();

        // El generador borra el mapa viejo, crea uno nuevo
        // y al final vuelve a posicionar a la serpiente en el centro
        if (scriptLaberinto != null)
        {
            scriptLaberinto.GenerarTodo();
        }

        turnInput = 0f;
        isDead = false;
    }

    void ResetearSerpiente()
    {
        foreach (GameObject b in bodyParts)
        {
            if (b != null) Destroy(b);
        }
        bodyParts.Clear();
        positionHistory.Clear();
        rotationHistory.Clear();

        transform.rotation = rotacionInicial;
    }
}