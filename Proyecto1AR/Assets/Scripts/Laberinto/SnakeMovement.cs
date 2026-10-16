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
    [Tooltip("Prefab del último segmento (la cola). Si se deja vacío usa el del cuerpo")]
    public GameObject tailPrefab;
    public int gap = 10;

    [Header("Colisión con el cuerpo")]
    [Tooltip("Cuántos segmentos pegados a la cabeza se ignoran para no morir por rozarlos")]
    public int segmentosSinColision = 2;

    // El último elemento de esta lista siempre es la cola
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
        // Primera manzana: lo único que aparece es la cola
        if (bodyParts.Count == 0)
        {
            GameObject prefabCola = tailPrefab != null ? tailPrefab : bodyPrefab;
            bodyParts.Add(CrearSegmento(prefabCola, transform.position, transform.rotation));
            return;
        }

        // Siguientes manzanas: el segmento nuevo se inserta justo antes de la cola
        GameObject cola = bodyParts[bodyParts.Count - 1];
        GameObject nuevo = CrearSegmento(bodyPrefab, cola.transform.position, cola.transform.rotation);
        bodyParts.Insert(bodyParts.Count - 1, nuevo);
    }

    GameObject CrearSegmento(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        GameObject segmento = Instantiate(prefab, pos, rot);
        segmento.transform.SetParent(transform.parent);
        segmento.transform.localScale = transform.localScale;
        return segmento;
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
            Debug.Log("¡Choque con pared!");
            Morir();
        }
        else
        {
            // ¿Es un segmento de mi propio cuerpo (la cola incluida)?
            int indice = bodyParts.IndexOf(other.gameObject);

            if (indice >= segmentosSinColision)
            {
                Debug.Log("¡Te mordiste a ti misma!");
                Morir();
            }
        }
    }

    void Morir()
    {
        isDead = true;
        Debug.Log("Generando un mapa nuevo...");

        ResetearSerpiente();

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