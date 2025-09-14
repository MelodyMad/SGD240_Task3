using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColourMap, Mesh, FallOffMap };
    public DrawMode drawmode;

    [Header("Map Settings")]
    [SerializeField] private int mapWidth;
    [SerializeField] private int mapHeight;
    [SerializeField] private float noiseScale;
    [SerializeField] private bool useFallOff;
    [Range(0, 1)][SerializeField] private float fallOffStrength;

    [Header("Noise Settings")]
    [SerializeField] private int octaves;
    [Range(0,1)] [SerializeField] private float persistance;
    [SerializeField] private float lacunarity;
    [SerializeField] private int seed;
    [SerializeField] private Vector2 offset;

    [Header("Mesh Settings")]
    [SerializeField] private float meshHeightMultiplier;
    [SerializeField] private AnimationCurve meshHeightCurve;
    [SerializeField] private float meshScale = 1f;

    [Header("Colour Settings")]
    public TerrainType[] reigons;

    public bool autoUpdate;

    public void GenerateMap()
    {
        // Generate the Noise Map
        float[,] noiseMap = PerlinNoise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);
        // Generate the Falloff Map if it is needed
        float[,] falloffMap = FallOffGenerator.GenerateFallOffMap(mapWidth);
        // Generate the Colour Map
        Color[] colourMap = new Color[mapWidth * mapHeight];

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float currentHeight = noiseMap[x, y];
                    // Apply the Falloff
                    if (useFallOff)
                    {
                        currentHeight = Mathf.Clamp01(currentHeight - falloffMap[x, y] * fallOffStrength);
                        noiseMap[x, y] = currentHeight;
                    }
                    // Assign the colour
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
        
        // Draw the map so that it is visable
        MapDisplay display = FindFirstObjectByType<MapDisplay>();

        switch (drawmode)
        {
            case DrawMode.NoiseMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
                break;

            case DrawMode.ColourMap:
                display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
                break;

            case DrawMode.Mesh:
                MeshData meshData = MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, meshScale); Mesh mesh = meshData.CreateMesh();
                display.DrawMesh(meshData, TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));

                MeshCollider meshCollider = display.gameObject.GetComponent<MeshCollider>();
                if (meshCollider == null)
                {
                    meshCollider = display.gameObject.AddComponent<MeshCollider>();
                }
                meshCollider.sharedMesh = mesh;
                break;

            case DrawMode.FallOffMap:
                if (useFallOff)
                {
                    for (int y = 0; y < mapHeight; y++)
                    {
                        for (int x = 0; x < mapWidth; x++)
                        {
                            falloffMap[x, y] *= fallOffStrength;
                        }
                    }
                }
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(falloffMap));
                break;
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
