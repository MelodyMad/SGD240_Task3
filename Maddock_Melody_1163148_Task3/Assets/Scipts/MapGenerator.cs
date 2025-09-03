using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColourMap };
    public DrawMode drawmode;

    [SerializeField] private int mapWidth;
    [SerializeField] private int mapHeight;
    [SerializeField] private float noiseScale;

    [SerializeField] private int octaves;
    [Range(0,1)] [SerializeField] private float persistance;
    [SerializeField] private float lacunarity;

    [SerializeField] private int seed;
    [SerializeField] private Vector2 offset;

    public bool autoUpdate;

    public TerrainType[] reigons;

    public void GenerateMap()
    {
        float[,] noiseMap = PerlinNoise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

        Color[] colourMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < reigons.Length; i++)
                {
                    if (currentHeight <= reigons[i].height)
                    {
                        colourMap[y * mapWidth + x] = reigons[i].colour;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawmode ==DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if (drawmode == DrawMode.ColourMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
        }
    }

    void OnValidate()
    {
        if (mapWidth < 1)
        {
            mapWidth = 1;
        }
        if (mapHeight < 1)
        {
            mapHeight = 1;
        }
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }

    }

    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color colour;
    }
}
