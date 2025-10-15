using UnityEngine;
using System.Collections;
using Unity.AI.Navigation;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class CustomMapGenerator : MonoBehaviour, IMapGenerator
{
    [Header("Map Settings")]
    [SerializeField] private int mapWidth = 200;
    [SerializeField] private int mapHeight = 200;
    [SerializeField] private float noiseScale = 20f;

    [Header("Noise Settings")]
    [SerializeField] private int octaves = 4;
    [Range(0, 1)][SerializeField] private float persistance = 0.5f;
    [SerializeField] private float lacunarity = 2f;
    [SerializeField] private int seed = 0;
    [SerializeField] private Vector2 offset;

    [Header("Mesh Settings")]
    [SerializeField] private float meshHeightMultiplier = 5f;
    [SerializeField] private AnimationCurve meshHeightCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private float meshScale = 1f;

    [Header("Falloff Settings")]
    [SerializeField] private bool useFallOff = true;
    [Range(0, 1)][SerializeField] private float fallOffStrength = 0.5f;

    [Header("Erosion Settings")]
    [SerializeField] private bool enablePlayerErosion = true;
    [SerializeField] private Transform player;
    [SerializeField] private float minMoveDistance = 0.2f;
    [SerializeField] private float erosionRadius = 3f;
    [SerializeField] private float erosionStrength = 0.5f;
    [SerializeField] private float erosionFalloff = 3f;

    [Header("References")]
    [SerializeField] private Material terrainMaterial;

    private Mesh mesh;
    private MeshCollider meshCollider;
    private NavMeshSurface navMeshSurface;
    private Vector3[] vertices;
    private float[,] noiseMap;
    private Vector3 lastPlayerPos;

    private void Awake()
    {
        if (player != null)
            lastPlayerPos = player.position; // initialize player's last position
    }

    private void Start()
    {
        GenerateMap();
        // Add or get NavMeshSurface
        navMeshSurface = GetComponent<NavMeshSurface>();
        if (navMeshSurface == null)
            navMeshSurface = gameObject.AddComponent<NavMeshSurface>();

        // Build the NavMesh now that terrain exists
        navMeshSurface.BuildNavMesh();
    }

    private void Update()
    {
        if (enablePlayerErosion && player != null && mesh != null)
        {
            // Only apply erosion if the player has actually moved
            if ((player.position - lastPlayerPos).sqrMagnitude > minMoveDistance * minMoveDistance)
            {
                ApplyErosionAtPosition(player.position);
                lastPlayerPos = player.position;
            }
        }
    }

    [ContextMenu("Generate Map")]
    public void GenerateMap()
    {
        noiseMap = PerlinNoise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

        if (useFallOff)
        {
            float[,] falloffMap = FallOffGenerator.GenerateFallOffMap(mapWidth);
            for (int y = 0; y < mapHeight; y++)
                for (int x = 0; x < mapWidth; x++)
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y] * fallOffStrength);
        }

        MeshData meshData = MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, meshScale);
        mesh = meshData.CreateMesh();

        GetComponent<MeshFilter>().mesh = mesh;
        meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        if (terrainMaterial != null)
            GetComponent<MeshRenderer>().material = terrainMaterial;

        vertices = mesh.vertices;
    }

    public void ApplyErosionAtPosition(Vector3 worldPosition)
    {
        if (!enablePlayerErosion && player != null && worldPosition == player.position)
            return; // skip if player erosion disabled

        Vector3 localPos = transform.InverseTransformPoint(worldPosition);

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = vertices[i];
            float dist = Vector2.Distance(new Vector2(v.x, v.z), new Vector2(localPos.x, localPos.z));
            if (dist < erosionRadius)
            {
                float falloff = 1 - Mathf.Pow(dist / erosionRadius, erosionFalloff);
                v.y -= erosionStrength * falloff * Time.deltaTime;
                vertices[i] = v;
            }
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = mesh;
    }

    public float GetHeightAtPosition(Vector3 worldPos)
    {
        if (vertices == null || vertices.Length == 0) return 0f;

        int x = Mathf.Clamp(Mathf.RoundToInt(worldPos.x), 0, mapWidth - 1);
        int z = Mathf.Clamp(Mathf.RoundToInt(worldPos.z), 0, mapHeight - 1);
        return vertices[z * (mapWidth + 1) + x].y;
    }

    public Vector3 ClampToMap(Vector3 position, float margin = 1f)
    {
        float clampedX = Mathf.Clamp(position.x, margin, mapWidth - margin);
        float clampedZ = Mathf.Clamp(position.z, margin, mapHeight - margin);
        return new Vector3(clampedX, position.y, clampedZ);
    }

    public float GetNormalizedHeightAtPosition(Vector3 worldPos)
    {
        return Mathf.InverseLerp(0f, meshHeightMultiplier, GetHeightAtPosition(worldPos));
    }

    // Call this whenever you regenerate the terrain
    public void RebuildNavMesh()
    {
        // If using Unity's built-in NavMesh:
        var surface = GetComponent<UnityEngine.AI.NavMeshSurface>();
        if (surface != null)
        {
            surface.BuildNavMesh();
        }
    }
}





