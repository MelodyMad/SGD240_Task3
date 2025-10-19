using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// This script generates a procedural terrain mesh with erosion applied by the player or agents. Implements IMapGenerator so it can be used by the AI agent system.
/// </summary>

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ErosionMapGenerator : MonoBehaviour, IMapGenerator
{
    [Header("Map Settings")]
    [SerializeField] private int mapSize = 100;
    [SerializeField] private float noiseScale = 15f;
    [SerializeField] private float heightMultiplier = 5f; // Multiplier for vertex heights

    [Header("Erosion Settings")]
    [SerializeField] private bool enableErosion = true; // Toggle erosion
    [SerializeField] private Transform player; // Reference to the player for erosion
    [SerializeField] private float erosionRadius = 3f; // Radius around the player where erosion is applied
    [SerializeField] private float erosionStrength = 0.5f; // How strong the erosion is
    [SerializeField] private float erosionFalloff = 3f; // Controls how erosion fades with distance around the radius

    [Header("References")]
    [SerializeField] private Material terrainMaterial; // Assign custom shader
    private float originalMapHeight; // store the original shader value

    private Mesh mesh; // Terrain mesh
    private MeshCollider meshCollider; // Collider for the mesh
    private Vector3[] vertices; // Vertex positions
    private Vector3 lastPlayerPos; // Last recorded player position

    public bool IsMapReady { get; private set; } = false; // Check if the map is ready

    // Before the game starts
    private void OnEnable()
    {
        if (terrainMaterial != null)
        {
            // Save the original map height before play
            originalMapHeight = terrainMaterial.GetFloat("_MapHeight");

            // Set it to the runtime value
            terrainMaterial.SetFloat("_MapHeight", heightMultiplier);
        }

        // Editor only
        #if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        #endif
    }

    private void Awake()
    {
        // Initialise the player's last position
        if (player != null)
        {
            lastPlayerPos = player.position;
        }
    }

    // When the game starts
    void Start()
    {
        GenerateMap();
    }

    // Called every frame
    void Update()
    {
        // Only apply erosion if enabled, player exists, and mesh is created
        if (enableErosion && player != null && mesh != null)
        {
            // Only apply erosion if the player has moved a noticeable amount
            if ((player.position - lastPlayerPos).sqrMagnitude > 0.01f) // Slightly larger threshold
            {
                ApplyErosionAtPosition(player.position); // Apply erosion around the player
                lastPlayerPos = player.position; // Update last player position
            }
        }
    }

    // Generates a flat terrain mesh using Perlin noise and sets up the mesh and collider.
    public void GenerateMap()
    {
        mesh = new Mesh(); // Create new mesh
        GetComponent<MeshFilter>().mesh = mesh; // Assign mesh to MeshFilter
        meshCollider = GetComponent<MeshCollider>(); // Get MeshCollider reference
        GetComponent<MeshRenderer>().material = terrainMaterial; // Assign the material

        int width = mapSize;
        int height = mapSize;
        vertices = new Vector3[(width + 1) * (height + 1)]; // Allocate vertex array
        Vector2[] uvs = new Vector2[vertices.Length]; // Allocate UV array

        // Generate vertices using Perlin noise
        for (int y = 0; y <= height; y++)
        {
            for (int x = 0; x <= width; x++)
            {
                float sampleX = (float)x / noiseScale;
                float sampleY = (float)y / noiseScale;
                float noise = Mathf.PerlinNoise(sampleX, sampleY); // Get noise value
                int i = y * (width + 1) + x; // Convert 2D index to 1D

                vertices[i] = new Vector3(x, noise * heightMultiplier, y); // Set vertex position

                // Assign UVs so the texture maps correctly
                float textureScale = 5f; // Increase to tile texture more often
                uvs[i] = new Vector2((float)x / width * textureScale, (float)y / height * textureScale);
            }
        }

        // Generate triangles for mesh
        int[] triangles = new int[width * height * 6]; // Allocate triangle indices
        int triIndex = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = y * (width + 1) + x;

                // First triangle
                triangles[triIndex++] = i;
                triangles[triIndex++] = i + width + 1;
                triangles[triIndex++] = i + 1;

                // Second triangle
                triangles[triIndex++] = i + 1;
                triangles[triIndex++] = i + width + 1;
                triangles[triIndex++] = i + width + 2;
            }
        }

        // Assign everything to the mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs; // Add UVs 
        mesh.RecalculateNormals(); // Recalculate normals for lighting
        meshCollider.sharedMesh = mesh; // Assign mesh to collider

        IsMapReady = true; // Mark map as ready
    }


    // Rebuilds the NavMesh at runtime (requires NavMeshSurface component)
    public void RebuildNavMesh()
    {
        var surface = GetComponent<NavMeshSurface>();
        if (surface != null)
        {
            surface.BuildNavMesh();
        }
    }

    // Returns the height of the terrain at a given world position.
    public float GetHeightAtPosition(Vector3 worldPos)
    {
        if (vertices == null || vertices.Length == 0) return 0f;

        int x = Mathf.Clamp(Mathf.RoundToInt(worldPos.x), 0, mapSize);
        int z = Mathf.Clamp(Mathf.RoundToInt(worldPos.z), 0, mapSize);

        return vertices[z * (mapSize + 1) + x].y;
    }

    // Returns the normalized height (0-1) at a given world position.
    public float GetNormalizedHeightAtPosition(Vector3 worldPos)
    {
        return Mathf.InverseLerp(0f, heightMultiplier, GetHeightAtPosition(worldPos));
    }

    // Clamps a position to remain inside the map boundaries.
    public Vector3 ClampToMap(Vector3 position, float margin = 1f)
    {
        float clampedX = Mathf.Clamp(position.x, margin, mapSize - margin);
        float clampedZ = Mathf.Clamp(position.z, margin, mapSize - margin);
        return new Vector3(clampedX, position.y, clampedZ);
    }

    // Applies erosion at a given world position, lowering vertices based on distance and falloff.
    public void ApplyErosionAtPosition(Vector3 worldPosition)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPosition);

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = vertices[i];
            float dist = Vector2.Distance(new Vector2(v.x, v.z), new Vector2(localPos.x, localPos.z));

            if (dist < erosionRadius)
            {
                float falloff = 1 - Mathf.Pow(dist / erosionRadius, erosionFalloff); // Smooth falloff
                v.y -= erosionStrength * falloff; // Lower vertex height
                vertices[i] = v; // Store modified vertex
            }
        }

        // Update mesh and collider
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = mesh;
    }

    // Property for external access to height multiplier
    public float HeightMultiplier => heightMultiplier;

    // Property for external access to map size
    public int MapSize => mapSize;

    // When the game is stopped
    private void OnDisable()
    {
        // Restore when object is disabled or destroyed
        RestoreOriginalMapHeight();

        #if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        #endif
    }

    #if UNITY_EDITOR
    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // When exiting play mode, restore the value
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            RestoreOriginalMapHeight();
        }
    }
    #endif

    // Revert the MapHeight value from the shader back to the original
    private void RestoreOriginalMapHeight()
    {
        if (terrainMaterial != null)
        {
            terrainMaterial.SetFloat("_MapHeight", originalMapHeight);
        }
    }

}


