using UnityEngine;
using System.Collections.Generic;

public class RandomWalkMaze : MonoBehaviour
{
    [Header("Configuración del Mapa")]
    public int width = 20;
    public int length = 20;
    public int maxFloorTiles = 300;

    [Header("Comportamiento del Laberinto")]
    public int minCorridorLength = 3;
    public int maxCorridorLength = 7;
    public int minPathWidth = 2;
    public int maxPathWidth = 3;

    [Header("Prefabs")]
    public GameObject wallPrefab;
    public GameObject floorPrefab;

    [Header("Ajustes de Modelos 3D")]
    [Tooltip("El tamaño real de tus bloques. Si tu cubo mide 2x2, pon 2. Si mide 0.5, pon 0.5")]
    public float tileSize = 0.5f;
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

    [Header("Generación")]
    [Tooltip("Si lo desactivas, el mapa solo se genera cuando llames a GenerarTodo() desde otro script")]
    public bool generarAlIniciar = true;

    private List<Vector3> validFloorPositions = new List<Vector3>();
    private List<GameObject> tiles = new List<GameObject>();
    private GameObject currentApple;
    private int[,] grid;

    void Start()
    {
        if (generarAlIniciar) GenerarTodo();
    }

    // Llama a esto desde tu activador del juego o desde la serpiente al morir
    public void GenerarTodo()
    {
        LimpiarMapa();
        GenerateMaze();
        WidenNarrowPaths();
        BuildMaze3D();
        SpawnApple();
        PosicionarJugador();
    }

    void LimpiarMapa()
    {
        foreach (GameObject t in tiles)
        {
            if (t != null) Destroy(t);
        }
        tiles.Clear();
        validFloorPositions.Clear();
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

        // Seguro anti-bucle infinito: no se pueden tallar más casillas que las interiores
        int maxPosibles = (width - 2) * (length - 2);
        int objetivo = Mathf.Min(maxFloorTiles, maxPosibles);

        int currentX = width / 2;
        int currentZ = length / 2;
        int tilesCarved = 0;
        int intentos = 0;
        int maxIntentos = 10000;

        while (tilesCarved < objetivo && intentos < maxIntentos)
        {
            intentos++;

            int dir = Random.Range(0, 4);
            int corridorLength = Random.Range(minCorridorLength, maxCorridorLength + 1);
            int currentPathWidth = Random.Range(minPathWidth, maxPathWidth + 1);

            for (int i = 0; i < corridorLength; i++)
            {
                if (tilesCarved >= objetivo) break;

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
        validFloorPositions.Clear();

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
                    validFloorPositions.Add(localPos);
                }

                newTile.transform.localPosition = localPos;
                tiles.Add(newTile);
            }
        }
    }

    void WidenNarrowPaths()
    {
        for (int x = 1; x < width - 1; x++)
        {
            for (int z = 1; z < length - 1; z++)
            {
                if (grid[x, z] == 0)
                {
                    if (grid[x, z + 1] == 1 && grid[x, z - 1] == 1)
                    {
                        if (z - 1 > 0) grid[x, z - 1] = 0;
                        else if (z + 1 < length - 1) grid[x, z + 1] = 0;
                    }

                    if (grid[x + 1, z] == 1 && grid[x - 1, z] == 1)
                    {
                        if (x - 1 > 0) grid[x - 1, z] = 0;
                        else if (x + 1 < width - 1) grid[x + 1, z] = 0;
                    }
                }
            }
        }

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

    public void PosicionarJugador()
    {
        if (snakePlayer == null) return;

        int centerX = width / 2;
        int centerZ = length / 2;

        List<Vector2Int> mejoresOpciones = new List<Vector2Int>();
        Queue<Vector2Int> cola = new Queue<Vector2Int>();
        bool[,] visitado = new bool[width, length];

        cola.Enqueue(new Vector2Int(centerX, centerZ));
        visitado[centerX, centerZ] = true;

        Vector2Int[] direcciones = new Vector2Int[]
        {
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 0), new Vector2Int(-1, 0)
        };

        // Búsqueda por capas desde el centro (BFS)
        while (cola.Count > 0 && mejoresOpciones.Count == 0)
        {
            int casillasEnEstaCapa = cola.Count;

            for (int i = 0; i < casillasEnEstaCapa; i++)
            {
                Vector2Int actual = cola.Dequeue();

                if (grid[actual.x, actual.y] == 0)
                {
                    mejoresOpciones.Add(actual);
                }

                if (mejoresOpciones.Count == 0)
                {
                    foreach (Vector2Int dir in direcciones)
                    {
                        int nx = actual.x + dir.x;
                        int nz = actual.y + dir.y;

                        if (nx >= 0 && nx < width && nz >= 0 && nz < length)
                        {
                            if (!visitado[nx, nz])
                            {
                                visitado[nx, nz] = true;
                                cola.Enqueue(new Vector2Int(nx, nz));
                            }
                        }
                    }
                }
            }
        }

        if (mejoresOpciones.Count > 0)
        {
            Vector2Int casilla = mejoresOpciones[Random.Range(0, mejoresOpciones.Count)];

            // Posición de la casilla en el espacio local del laberinto
            Vector3 posLocalLaberinto = new Vector3(
                casilla.x * tileSize,
                floorOffsetY,
                casilla.y * tileSize
            );

            // A coordenadas de mundo, respetando posición/rotación/escala del laberinto
            Vector3 posMundo = transform.TransformPoint(posLocalLaberinto);

            // Conservamos la profundidad actual del snake
            posMundo.z = snakePlayer.transform.position.z;

            snakePlayer.transform.position = posMundo;
        }
    }

    public void SpawnApple()
    {
        if (validFloorPositions.Count == 0 || applePrefab == null) return;

        int randomIndex = Random.Range(0, validFloorPositions.Count);
        Vector3 spawnPos = validFloorPositions[randomIndex];
        spawnPos.y += appleOffsetY;

        if (currentApple == null)
        {
            currentApple = Instantiate(applePrefab, transform);
        }
        currentApple.transform.localPosition = spawnPos;
    }
}