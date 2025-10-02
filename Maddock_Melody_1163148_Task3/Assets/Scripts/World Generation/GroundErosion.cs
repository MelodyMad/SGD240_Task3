using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class GroundErosion : MonoBehaviour
{
    public Transform player;
    public float erosionRadius = 2f;
    public float erosionStrength = 0.05f;
    public float updateInterval = 0.2f; // Update collider every 0.2s for performance

    private Mesh mesh;
    private Vector3[] vertices;
    private Vector3[] originalVertices;
    private MeshCollider meshCollider;
    private float updateTimer;

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
    }

    void Update()
    {
        Vector3 playerPos = transform.InverseTransformPoint(player.position);

        // Loop through vertices and erode near player
        for (int i = 0; i < vertices.Length; i++)
        {
            float dist = Vector3.Distance(new Vector3(vertices[i].x, 0, vertices[i].z),
                                          new Vector3(playerPos.x, 0, playerPos.z));

            if (dist < erosionRadius)
            {
                float falloff = 1 - (dist / erosionRadius); // smooth falloff
                vertices[i].y -= erosionStrength * falloff * Time.deltaTime;
            }
        }

        // Apply updated mesh
        mesh.vertices = vertices;
        mesh.RecalculateNormals();

        // Update collider occasionally (not every frame)
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
            updateTimer = 0f;
        }

    }
}



