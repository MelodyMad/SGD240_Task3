using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class GroundErosion : MonoBehaviour
{
    [SerializeField] private Transform player;

    [Header("Erosion Settings")]
    [SerializeField] private float erosionRadius = 2f;
    [SerializeField] private float erosionStrength = 0.05f;
    [SerializeField] private float updateInterval = 0.2f; // Update collider every 0.2s for performance

    private Mesh mesh;
    private Vector3[] vertices;
    private Vector3[] originalVertices;
    private MeshCollider meshCollider;
    private float updateTimer;

    private float[,] noiseMap;
    private int mapWidth;
    private int mapHeight;

    [Header("Noise Map Refernces")]
    [SerializeField] private Texture2D noiseTexture;
    [SerializeField] private Material terrainMaterial;
    [SerializeField] private MapGenerator mapGenerator;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        originalVertices = mesh.vertices;

        meshCollider = GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }

        // Get references from MapGenerator
        if (mapGenerator != null)
        {
            noiseMap = mapGenerator.NoiseMap;
            mapWidth = mapGenerator.MapWidth;
            mapHeight = mapGenerator.MapHeight;
        }
    }

    void Update()
    {
        updateTimer += Time.deltaTime;

        if (mapGenerator == null || mapGenerator.NoiseMap == null)
        {
            return; // skip erosion until the map exists
        }

        // Safe references
        noiseMap = mapGenerator.NoiseMap;
        mapWidth = mapGenerator.MapWidth;
        mapHeight = mapGenerator.MapHeight;

        Vector3 playerPos = transform.InverseTransformPoint(player.position);

        // Convert player position to noise map coordinates
        float mapX = (playerPos.x / mapWidth + 0.5f) * noiseMap.GetLength(0);
        float mapY = (playerPos.z / mapHeight + 0.5f) * noiseMap.GetLength(1);

        int radiusInPixels = Mathf.CeilToInt((erosionRadius / mapWidth) * noiseMap.GetLength(0));

        for (int dx = -radiusInPixels; dx <= radiusInPixels; dx++)
        {
            for (int dy = -radiusInPixels; dy <= radiusInPixels; dy++)
            {
                int nx = Mathf.FloorToInt(mapX + dx);
                int ny = Mathf.FloorToInt(mapY + dy);

                if (nx < 0 || nx >= noiseMap.GetLength(0) || ny < 0 || ny >= noiseMap.GetLength(1))
                    continue;

                float distance = new Vector2(dx, dy).magnitude / radiusInPixels;
                if (distance > 1f) continue;

                float falloffValue = mapGenerator.falloffMap != null ? mapGenerator.falloffMap[nx, ny] : 1f;
                float falloff = 1 - Mathf.Pow(distance, 3); 

                noiseMap[nx, ny] -= erosionStrength * falloff * falloffValue * Time.deltaTime;
                noiseMap[nx, ny] = Mathf.Clamp01(noiseMap[nx, ny]);
            }
        }

        // Now loop vertices to update mesh height
        for (int i = 0; i < vertices.Length; i++)
        {
            int x = Mathf.FloorToInt((vertices[i].x + mapWidth / 2f) / mapWidth * noiseMap.GetLength(0));
            int y = Mathf.FloorToInt((vertices[i].z + mapHeight / 2f) / mapHeight * noiseMap.GetLength(1));

            vertices[i].y = noiseMap[x, y]; //* meshHeightMultiplier; // map noise to mesh height
        }



        // Apply updated mesh
        mesh.vertices = vertices;
        mesh.RecalculateNormals();

        // Update collider occasionally (not every frame)
        if (updateTimer >= updateInterval)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;

            if (mapGenerator != null)
            {
                mapGenerator.UpdateColourMap(); // refresh terrain colours
            }

            updateTimer = 0f;
        }

    }

}



