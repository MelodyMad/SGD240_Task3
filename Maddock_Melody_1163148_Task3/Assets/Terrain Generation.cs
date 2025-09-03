using UnityEngine;
using UnityEngine.UIElements;

public class TerrainGeneration : MonoBehaviour
{
    [SerializeField] private int mountainHeight = 20;
    [SerializeField] private int terrainWidth = 256;
    [SerializeField] private int terrainLength = 256;
    [SerializeField] private float scale = 20f;

    [SerializeField] private float offsetX = 100f;
    [SerializeField] private float offsetY = 100f;


    void Start()
    {
        offsetX = Random.Range(0f, 9999f);
        offsetY = Random.Range(0f, 9999f);

        // Create a variable that is a reference to the terrain in the editor
        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);

    }

    // Generate the Data needed to create the height
    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = terrainWidth + 1;

        terrainData.size = new Vector3(terrainWidth, mountainHeight, terrainLength);
        terrainData.SetHeights(0, 0, GenerateHeights());
        return terrainData;
    }

    float[,] GenerateHeights()
    {
        // Create a float value of the Perlin noise value
        float[,] heights = new float[terrainWidth, terrainLength];
        for (int x = 0; x < terrainWidth; x++)
        {
            for (int y = 0; y < terrainLength; y++)
            {
                // Create Perlin Noise Value
                heights[x, y] = CalculateHeight(x, y);
            }
        }

        return heights;
    }

    // Calculate Perlin Noise x and y values
    float CalculateHeight(int x, int y)
    {
        float xCoord = (float)x / terrainWidth * scale + offsetX;
        float yCoord = (float)y / terrainLength * scale + offsetY;

        return Mathf.PerlinNoise(xCoord, yCoord);
    }

}
