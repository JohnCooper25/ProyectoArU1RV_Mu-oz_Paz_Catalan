using UnityEngine;

public class IniciadorAR : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject laberinto3D;      // Aquí va tu "PivoteLaberinto"
    public Transform puntoDeEscaneo;    // Tu Image Target

    [Header("Jugador")]
    public GameObject jugadorSerpiente; // NUEVO: Tu serpiente desactivada

    private bool juegoYaIniciado = false;

    public void DesplegarJuego()
    {
        // Solo queremos que aparezca la primera vez que se escanee el QR
        if (!juegoYaIniciado)
        {
            // 1. Mueve el pivote a las coordenadas donde está el QR
            laberinto3D.transform.position = puntoDeEscaneo.position;
            laberinto3D.transform.rotation = puntoDeEscaneo.rotation;

            // 2. Enciende los objetos del laberinto
            laberinto3D.SetActive(true);

            // 3. NUEVO: Enciende al jugador si está asignado
            if (jugadorSerpiente != null)
            {
                jugadorSerpiente.SetActive(true);
            }

            // 4. Marca el juego como iniciado
            juegoYaIniciado = true;

            Debug.Log("¡Juego y Jugador Desplegados en la habitación!");
        }
    }
}