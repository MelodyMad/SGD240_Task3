using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// This script generates a procedural terrain mesh with erosion applied by the player or agents. 
/// </summary>

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ErosionMapGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private int mapSize = 100;
    [SerializeField] private float noiseScale = 15f;
    [SerializeField] private float heightMultiplier = 5f; // Multiplier for vertex heights

    [Header("Erosion Settings")]
    [SerializeField] private bool enableErosion = true; // Toggle erosion
    [SerializeField] private Transform player; // Refernce to the player for erosion
    [SerializeField] private float erosionRadius = 3f; // Radius around the player where erosion is applied
    [SerializeField] private float erosionStrength = 0.5f; // How strong the erosion is
    [SerializeField] private float erosionFalloff = 3f; // Controls how erosion fades with distance around the radius

    [Header("References")]
    [SerializeField] private Material terrainMaterial; // Assign custom shader

    private Mesh mesh; // Terrain mesh
    private MeshCollider meshCollider; // Collider for the mesh
    private Vector3[] vertices; // Vertex positions
    private Vector2[] uvs; // UVs for texturing
    private int[] triangles; // Triangle indicies
    private float[,] noiseMap; // Noise values for terrain
    private int vertexCount; // Total number of verticies
    private Vector3 lastPlayerPos; // Last recorded player position
    public bool IsMapReady { get; private set; } = false; // Check if the map is ready

    // When the game starts
    void Start()
    {
        GenerateMap();
    }

    // Called Every frame
    void Update()
    {
        if (enableErosion && player != null)
        {
            // Only apply erosion if the player has moved
            if ((player.position - lastPlayerPos).sqrMagnitude > 0.0001f)
            {
                // Apply erosion around the player
                ApplyErosionAtPosition(player.position);
            }
            // Update last player position
            lastPlayerPos = player.position;
        }
    }

    // Generates a flat terrain mesh using Pelin noise and sets up the mesh and collider.
    void GenerateMap()
    {
        mesh = new Mesh(); // Create new mesh
        GetComponent<MeshFilter>().mesh = mesh; // Assign mesh to MeshFilter
        meshCollider = GetComponent<MeshCollider>(); // Get MeshCollider reference
        GetComponent<MeshRenderer>().material = terrainMaterial; // Assign the material

        int width = mapSize;
        int height = mapSize;

        // Initialise the noise map
        noiseMap = new float[width, height];
        vertexCount = (width + 1) * (height + 1);

        vertices = new Vector3[vertexCount]; // Allocate vertex array
        uvs = new Vector2[vertexCount]; // Allocate UV array
        triangles = new int[width * height * 6]; // Allocate triangle indices

        // Generate noise map and vertices
        for (int y = 0; y <= height; y++)
        {
            for (int x = 0; x <= width; x++)
            {
                float sampleX = (float)x / noiseScale;
                float sampleY = (float)y / noiseScale;
                float noise = Mathf.PerlinNoise(sampleX, sampleY); // Get the Perlin noise value
                noiseMap[x % width, y % height] = noise; // Store the noise in a 2D array

                int i = y * (width + 1) + x; // Convert 2D index to 1D
                vertices[i] = new Vector3(x, noise * heightMultiplier, y); // Set vertex position
                uvs[i] = new Vector2((float)x / width, (float)y / height); // Set UVs
            }
        }

        // Generate triangles
        int triIndex = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = y * (width + 1) + x;

                triangles[triIndex++] = i;
                triangles[triIndex++] = i + width + 1;
                triangles[triIndex++] = i + 1;

                triangles[triIndex++] = i + 1;
                triangles[triIndex++] = i + width + 1;
                triangles[triIndex++] = i + width + 2;
            }
        }
        // Assign vertices and triangles to mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals(); // Recalculate normals for lighting
        meshCollider.sharedMesh = mesh; // Assign mesh to collider

        if (terrainMaterial != null)
        {
            terrainMaterial.SetFloat("_MapHeight", heightMultiplier); // Update shader height
        }

        IsMapReady = true; // Mark map as ready
    }

    // Applies erosion at a given world position, lowering vertices based on distance and falloff.
    public void ApplyErosionAtPosition(Vector3 worldPosition)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPosition); // Convert to local space

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = vertices[i];
            float dist = Vector2.Distance(new Vector2(v.x, v.z), new Vector2(localPos.x, localPos.z)); // Horizontal distance

            // To only affect verticies within the radius
            if (dist < erosionRadius)
            {
                float falloff = 1 - Mathf.Pow(dist / erosionRadius, erosionFalloff); // Smooth falloff
                v.y -= erosionStrength * falloff * Time.deltaTime; // Lower vertex height
                vertices[i] = v; // Store modified vertex
            }
        }

        mesh.vertices = vertices; // Update mesh verticies
        mesh.RecalculateNormals(); // Recalculate normals
        meshCollider.sharedMesh = null; // Refresh collider
        meshCollider.sharedMesh = mesh;

        if (terrainMaterial != null)
        {
            terrainMaterial.SetFloat("_MapHeight", heightMultiplier); // Update shader height
        }
    }

    // Returns world height at a specific position
    public float GetHeightAtPosition(Vector3 worldPos)
    {
        if (vertices == null || vertices.Length == 0) return 0f;

        int x = Mathf.Clamp(Mathf.RoundToInt(worldPos.x), 0, mapSize); // Clamp X to map
        int z = Mathf.Clamp(Mathf.RoundToInt(worldPos.z), 0, mapSize); // Clamp Z to map
        
        // Return vertex height
        return vertices[z * (mapSize + 1) + x].y;
    }

    public int MapSize => mapSize;

    // Returns normalized height (0-1) at a specific position
    public float GetNormalizedHeightAtPosition(Vector3 worldPos)
    {
        float height = GetHeightAtPosition(worldPos);
        return Mathf.InverseLerp(0f, heightMultiplier, height);
    }

    // Clamps a position to remain inside the map boundaries
    public Vector3 ClampToMap(Vector3 position, float margin = 1f)
    {
        float clampedX = Mathf.Clamp(position.x, margin, mapSize - margin); // Clamp X
        float clampedZ = Mathf.Clamp(position.z, margin, mapSize - margin); // Clamp Z
        return new Vector3(clampedX, position.y, clampedZ);
    }

    public float HeightMultiplier => heightMultiplier;
}


