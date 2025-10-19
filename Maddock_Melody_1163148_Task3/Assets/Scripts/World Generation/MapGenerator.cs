using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

/// <summary>
/// This script generates procedural terrain maps using Perlin noise
/// </summary>

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColourMap, Mesh, FallOffMap };
    public DrawMode drawmode;

    [Header("Map Settings")]
    [SerializeField] private int mapWidth = 200;
    [SerializeField] private int mapHeight = 200;
    [SerializeField] private float noiseScale;
    [SerializeField] private bool useFallOff;
    [Range(0, 1)] [SerializeField] private float fallOffStrength;

    [Header("Noise Settings")]
    [SerializeField] private int octaves; // Layers of Perlin noise
    [Range(0,1)] [SerializeField] private float persistance; // Controls amplitute of each successive octave decreases (higher = rougher terrain) 
    [SerializeField] private float lacunarity; // Controls how frequency of each successive octave increases (higher = more small-scale detail)
    [SerializeField] private int seed; // Seed of a map
    [SerializeField] private Vector2 offset; // Offset to shift the noise pattern in X and Y

    [Header("Mesh Settings")]
    [SerializeField] private float meshHeightMultiplier; // Multiplies the height of verticies to increase the terrain height
    [SerializeField] private AnimationCurve meshHeightCurve; // Modify how heights are applied for smoother or exaggerated slopes
    [SerializeField] private float meshScale = 1f; // Overall scale of the mesh

    [Header("Colour Settings")]
    public TerrainType[] reigons;

    public bool autoUpdate;

    // Public read-only properties for accessing generated maps
    public float[,] NoiseMap { get; private set; }
    public float[,] falloffMap { get; private set; }
    public int MapWidth => mapWidth;
    public int MapHeight => mapHeight;

    private void Start()
    {
        GenerateMap();
    }

    // Generates the terrain map according to the currenct settinds and updates the display based on the selected DrawMode.
    [ContextMenu("Generate Map")]
    public void GenerateMap()
    {
        // Generate the Noise Map
        float[,] noiseMap = PerlinNoise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);
        NoiseMap = noiseMap;

        // Generate the Falloff Map 
        float[,] falloffMap = FallOffGenerator.GenerateFallOffMap(mapWidth);

        // Generate the Colour Map
        Color[] colourMap = new Color[mapWidth * mapHeight];

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float currentHeight = noiseMap[x, y];
                    // Apply Falloff if enabled
                    if (useFallOff)
                    {
                        currentHeight = Mathf.Clamp01(currentHeight - falloffMap[x, y] * fallOffStrength);
                        noiseMap[x, y] = currentHeight;
                    }
                    // Assign terrain colour based on height
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
        
        // Find the display object in the scene
        MapDisplay display = FindFirstObjectByType<MapDisplay>();

        // Draw the map based on the selected mode
        switch (drawmode)
        {
            // If the NoiseMap mode is selected
            case DrawMode.NoiseMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
                break;

            // If the ColourMap mode is selected
            case DrawMode.ColourMap:
                display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
                break;

            // If the Mesh mode is selected
            case DrawMode.Mesh:
                MeshData meshData = MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, meshScale); Mesh mesh = meshData.CreateMesh();
                display.DrawMesh(meshData, TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
                
                // Ensure a MeshCollider exists and assign the mesh
                MeshCollider meshCollider = display.gameObject.GetComponent<MeshCollider>();
                if (meshCollider == null)
                {
                    meshCollider = display.gameObject.AddComponent<MeshCollider>();
                }
                meshCollider.sharedMesh = mesh;
                break;

            // If the FallOff Map mode is selected
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

    // Updates only the colour map based on the current noise map.
    public void UpdateColourMap()
    {
        if (NoiseMap == null) return;

        Color[] colourMap = new Color[mapWidth * mapHeight];

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = NoiseMap[x, y];

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

        MapDisplay display = FindFirstObjectByType<MapDisplay>();
        if (display != null)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
        }
    }

    // Ensures parameters are within valid ranges when values change in the editor.
    void OnValidate()
    {
        if (mapWidth < 1) mapWidth = 1;
        if (mapHeight < 1) mapHeight = 1;
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;
    }

    [System.Serializable]
    public struct TerrainType
    {
        public string name; // Name of the terrain type
        public float height; // Max height for this terrain type
        public Color colour; // Colour associated with this terrain type
        public Texture2D texture; // Optional texture for this terrain type
    }
}
