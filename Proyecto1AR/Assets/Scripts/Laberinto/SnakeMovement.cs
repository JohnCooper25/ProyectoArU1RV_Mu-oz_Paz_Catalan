using UnityEngine;
using System.Collections.Generic;

public class SnakeMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public float steerSpeed = 180f;

    [Header("Crecimiento del Cuerpo")]
    public GameObject bodyPrefab;
    [Tooltip("Qué tan separadas están las piezas del cuerpo")]
    public int gap = 10;

    private List<GameObject> bodyParts = new List<GameObject>();
    private List<Vector3> positionHistory = new List<Vector3>();
    private List<Quaternion> rotationHistory = new List<Quaternion>();

    private float turnInput;

    void Update()
    {
        turnInput = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
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
        bodyParts.Add(body);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Comida"))
        {
            Grow();
            FindObjectOfType<RandomWalkMaze>().SpawnApple();
        }
        else if (other.CompareTag("Pared"))
        {
            Debug.Log("¡Chocaste contra una pared!");
        }
    }
}