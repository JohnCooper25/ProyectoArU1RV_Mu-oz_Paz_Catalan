using UnityEngine;

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

    private int[,] grid;

    void Start()
    {
        GenerateMaze();
        BuildMaze3D();
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
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                GameObject newTile;
                Vector3 localPos;

                if (grid[x, z] == 1)
                {
                    // Calculamos la posición
                    localPos = new Vector3(x * tileSize, wallOffsetY, z * tileSize);
                    // 1. Instanciamos el objeto asignándole el padre directamente (transform)
                    newTile = Instantiate(wallPrefab, transform);
                }
                else
                {
                    localPos = new Vector3(x * tileSize, floorOffsetY, z * tileSize);
                    newTile = Instantiate(floorPrefab, transform);
                }

                // 2. LA CLAVE: Forzamos su posición LOCAL relativa al MazeGenerator
                newTile.transform.localPosition = localPos;
            }
        }
    }
}