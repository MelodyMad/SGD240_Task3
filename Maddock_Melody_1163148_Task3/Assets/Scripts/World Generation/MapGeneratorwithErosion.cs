using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class MapGeneratorWithErosion : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private int mapSize = 100;
    [SerializeField] private float noiseScale = 15f;
    [SerializeField] private float heightMultiplier = 5f;

    [Header("Erosion Settings")]
    [SerializeField] private bool enableErosion = true;
    [SerializeField] private Transform player;
    [SerializeField] private float erosionRadius = 3f;
    [SerializeField] private float erosionStrength = 0.5f;
    [SerializeField] private float erosionFalloff = 3f;

    [Header("References")]
    [SerializeField] private Material terrainMaterial; // assign your custom shader here

    private Mesh mesh;
    private MeshCollider meshCollider;
    private Vector3[] vertices;
    private Vector2[] uvs;
    private int[] triangles;
    public bool IsMapReady { get; private set; } = false;

    private float[,] noiseMap;
    private int vertexCount;
    private Vector3 lastPlayerPos;

    void Start()
    {
        GenerateMap();
    }

    void Update()
    {
        if (enableErosion && player != null)
        {
            // Only erode when the player moves
            if ((player.position - lastPlayerPos).sqrMagnitude > 0.0001f)
            {
                ApplyErosionAtPosition(player.position);
            }

            lastPlayerPos = player.position;
        }
    }

    void GenerateMap()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        meshCollider = GetComponent<MeshCollider>();
        GetComponent<MeshRenderer>().material = terrainMaterial;

        int width = mapSize;
        int height = mapSize;

        noiseMap = new float[width, height];
        vertexCount = (width + 1) * (height + 1);

        vertices = new Vector3[vertexCount];
        uvs = new Vector2[vertexCount];
        triangles = new int[width * height * 6];

        // Generate noise map and vertices
        for (int y = 0; y <= height; y++)
        {
            for (int x = 0; x <= width; x++)
            {
                float sampleX = (float)x / noiseScale;
                float sampleY = (float)y / noiseScale;
                float noise = Mathf.PerlinNoise(sampleX, sampleY);
                noiseMap[x % width, y % height] = noise;

                int i = y * (width + 1) + x;
                vertices[i] = new Vector3(x, noise * heightMultiplier, y);
                uvs[i] = new Vector2((float)x / width, (float)y / height);
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

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = mesh;

        if (terrainMaterial != null)
        {
            terrainMaterial.SetFloat("_MapHeight", heightMultiplier);
        }

        IsMapReady = true;
    }

    // universal erosion function (used by both player & AI)
    public void ApplyErosionAtPosition(Vector3 worldPosition)
    {
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
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;

        if (terrainMaterial != null)
        {
            terrainMaterial.SetFloat("_MapHeight", heightMultiplier);
        }
    }

    // Return world height at a position
    public float GetHeightAtPosition(Vector3 worldPos)
    {
        if (vertices == null || vertices.Length == 0)
            return 0f;

        int x = Mathf.Clamp(Mathf.RoundToInt(worldPos.x), 0, mapSize);
        int z = Mathf.Clamp(Mathf.RoundToInt(worldPos.z), 0, mapSize);

        return vertices[z * (mapSize + 1) + x].y;
    }

    public int MapSize => mapSize;

    // Return normalized height (0-1) for height preference
    public float GetNormalizedHeightAtPosition(Vector3 worldPos)
    {
        float height = GetHeightAtPosition(worldPos);
        return Mathf.InverseLerp(0f, heightMultiplier, height);
    }

    // Clamp agent spawn/movement inside map edges
    public Vector3 ClampToMap(Vector3 position, float margin = 1f)
    {
        float clampedX = Mathf.Clamp(position.x, margin, mapSize - margin);
        float clampedZ = Mathf.Clamp(position.z, margin, mapSize - margin);
        return new Vector3(clampedX, position.y, clampedZ);
    }

    public float HeightMultiplier => heightMultiplier;
}


