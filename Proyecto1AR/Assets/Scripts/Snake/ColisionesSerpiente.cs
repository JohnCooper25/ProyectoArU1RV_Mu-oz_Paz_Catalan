using UnityEngine;

public class ColisionesSerpiente : MonoBehaviour
{
    [Header("Conexión con el Laberinto")]
    // Variable para arrastrar tu script RandomWalkMaze
    public RandomWalkMaze scriptLaberinto;

    private void OnCollisionEnter(Collision choque)
    {
        if (choque.gameObject.CompareTag("Pared"))
        {
            Debug.Log("¡GAME OVER! Chocaste contra una pared.");
            // Aquí luego pondremos la lógica de perder
        }
    }

    private void OnTriggerEnter(Collider atravesado)
    {
        if (atravesado.gameObject.CompareTag("Comida"))
        {
            Debug.Log("¡ÑAM ÑAM! Comiste una manzana.");

            // 1. Llamamos a TU función para que teletransporte la manzana a un nuevo lugar
            if (scriptLaberinto != null)
            {
                scriptLaberinto.SpawnApple();
            }

            // 2. Aquí llamaremos a la función para que la cola crezca (próximo paso)
            Crecer();
        }
    }

    void Crecer()
    {
        Debug.Log("Preparando lógica para añadir un bloque a la cola...");
    }
}