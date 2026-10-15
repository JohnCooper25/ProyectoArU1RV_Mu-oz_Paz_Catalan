using UnityEngine;
using System.Collections.Generic;

public class RandomWalkMaze : MonoBehaviour
{
    [Header("Configuración del Mapa")]
    public int width = 30;
    public int length = 30;
    public int maxFloorTiles = 400;

    [Header("Comportamiento del Laberinto")]
    public int minCorridorLength = 3;
    public int maxCorridorLength = 7;
    public int minPathWidth = 2;
    public int maxPathWidth = 3;

    [Header("Prefabs")]
    public GameObject wallPrefab;
    public GameObject floorPrefab;

    [Header("Ajustes de Modelos 3D (NUEVO)")]
    [Tooltip("El tamaño real de tus bloques. Si tu cubo mide 2x2, pon 2. Si mide 0.5, pon 0.5")]
    public float tileSize = 1f;
    [Tooltip("Ajuste de altura para la pared (0.5 para cubos por defecto, 0 para modelos con pivote en la base)")]
    public float wallOffsetY = 0.5f;
    [Tooltip("Ajuste de altura para el suelo")]
    public float floorOffsetY = 0f;

    [Header("Comida")]
    public GameObject applePrefab;
    [Tooltip("Altura extra para que la manzana no quede enterrada en el suelo")]
    public float appleOffsetY = 0.5f;

    [Header("Jugador")]
    public GameObject snakePlayer;
    [Tooltip("Altura a la que flotará la serpiente para no tocar el suelo")]
    public float snakeSpawnHeight = 0.5f;

    // Aquí guardaremos todas las posiciones válidas de suelo
    private List<Vector3> validFloorPositions = new List<Vector3>();
    private GameObject currentApple;

    private int[,] grid;

    void Start()
    {
        GenerateMaze();
        WidenNarrowPaths();
        BuildMaze3D();
        SpawnApple();

        // NUEVO: Posicionar a la serpiente en el centro exacto del mapa
        if (snakePlayer != null)
        {
            // Calculamos la posición central basada en el tamaño del mapa y los tiles
            Vector3 centerLocalPos = new Vector3((width / 2) * tileSize, snakeSpawnHeight, (length / 2) * tileSize);

            // Movemos la serpiente a esa posición (adaptada a la escala del generador)
            snakePlayer.transform.position = transform.TransformPoint(centerLocalPos);

            // Reiniciamos su rotación para que mire hacia adelante
            snakePlayer.transform.rotation = Quaternion.identity;
        }
    }

    void GenerateMaze()
    {
        grid = new int[width, length];
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                grid[x, z] = 1;
            }
        }

        int currentX = width / 2;
        int currentZ = length / 2;
        int tilesCarved = 0;

        while (tilesCarved < maxFloorTiles)
        {
            int dir = Random.Range(0, 4);
            int corridorLength = Random.Range(minCorridorLength, maxCorridorLength + 1);
            int currentPathWidth = Random.Range(minPathWidth, maxPathWidth + 1);

            for (int i = 0; i < corridorLength; i++)
            {
                if (tilesCarved >= maxFloorTiles) break;

                int nextX = currentX;
                int nextZ = currentZ;

                if (dir == 0) nextZ++;
                else if (dir == 1) nextZ--;
                else if (dir == 2) nextX++;
                else if (dir == 3) nextX--;

                if (nextX > 0 && nextX < width - 1 && nextZ > 0 && nextZ < length - 1)
                {
                    currentX = nextX;
                    currentZ = nextZ;

                    for (int wX = 0; wX < currentPathWidth; wX++)
                    {
                        for (int wZ = 0; wZ < currentPathWidth; wZ++)
                        {
                            int carveX = currentX + wX;
                            int carveZ = currentZ + wZ;

                            if (carveX > 0 && carveX < width - 1 && carveZ > 0 && carveZ < length - 1)
                            {
                                if (grid[carveX, carveZ] == 1)
                                {
                                    grid[carveX, carveZ] = 0;
                                    tilesCarved++;
                                }
                            }
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }

    void BuildMaze3D()
    {
        validFloorPositions.Clear(); // Limpiamos la lista por si reiniciamos el nivel

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                GameObject newTile;
                Vector3 localPos;

                if (grid[x, z] == 1)
                {
                    localPos = new Vector3(x * tileSize, wallOffsetY, z * tileSize);
                    newTile = Instantiate(wallPrefab, transform);
                }
                else
                {
                    localPos = new Vector3(x * tileSize, floorOffsetY, z * tileSize);
                    newTile = Instantiate(floorPrefab, transform);

                    // NUEVO: Guardamos esta posición como un lugar válido para la manzana
                    validFloorPositions.Add(localPos);
                }

                newTile.transform.localPosition = localPos;
            }
        }
    }
    void WidenNarrowPaths()
    {
        // Recorremos todo el mapa evitando los bordes extremos
        for (int x = 1; x < width - 1; x++)
        {
            for (int z = 1; z < length - 1; z++)
            {
                if (grid[x, z] == 0) // Si la casilla es Suelo
                {
                    // 1. Detectar pasillo estrecho horizontal (Pared arriba y abajo)
                    if (grid[x, z + 1] == 1 && grid[x, z - 1] == 1)
                    {
                        // Romper la pared de abajo (asegurando no romper el muro límite del juego)
                        if (z - 1 > 0) grid[x, z - 1] = 0;
                        else if (z + 1 < length - 1) grid[x, z + 1] = 0;
                    }
                    
                    // 2. Detectar pasillo estrecho vertical (Pared a izquierda y derecha)
                    if (grid[x + 1, z] == 1 && grid[x - 1, z] == 1)
                    {
                        // Romper la pared izquierda (asegurando no romper el muro límite del juego)
                        if (x - 1 > 0) grid[x - 1, z] = 0;
                        else if (x + 1 < width - 1) grid[x + 1, z] = 0;
                    }
                }
            }
        }
        
        // Opcional: Ejecutar la limpieza una segunda vez para asegurar 
        // que al romper paredes no se crearon cuellos de botella nuevos
        for (int x = 1; x < width - 1; x++)
        {
            for (int z = 1; z < length - 1; z++)
            {
                if (grid[x, z] == 0)
                {
                    if (grid[x, z + 1] == 1 && grid[x, z - 1] == 1)
                        if (z - 1 > 0) grid[x, z - 1] = 0;
                        
                    if (grid[x + 1, z] == 1 && grid[x - 1, z] == 1)
                        if (x - 1 > 0) grid[x - 1, z] = 0;
                }
            }
        }
    }
    public void SpawnApple()
    {
        // 1. Elegir un índice al azar de nuestra lista de suelos
        int randomIndex = Random.Range(0, validFloorPositions.Count);
        Vector3 spawnPos = validFloorPositions[randomIndex];

        // Ajustamos la altura para que flote o repose sobre el piso
        spawnPos.y += appleOffsetY;

        // 2. Si la manzana no existe, la creamos. Si ya existe, solo la movemos.
        if (currentApple == null)
        {
            currentApple = Instantiate(applePrefab, transform);
        }

        currentApple.transform.localPosition = spawnPos;
    }
}