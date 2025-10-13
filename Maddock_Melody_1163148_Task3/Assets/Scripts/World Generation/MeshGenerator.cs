using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// This script generates a terrain mesh from a height map and provides a MeshData structure for verticies, triangles, and UVs, which can be converted into a Unity Mesh
/// </summary>

public static class MeshGenerator 
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve, float meshScale)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        // Calculate top-left position so that the mesh is centered
        float topLeftX = (width - 1) / -2f * meshScale;
        float topLeftZ = (height - 1) / 2f * meshScale;

        MeshData meshData = new MeshData(width, height);
        int vertexIndex = 0;

        for (int y=0; y < height; y++)
        {
            for (int x =0; x < width; x++)
            {
                // Calculate vertex height using the height and curve multiplier
                float vertexHeight = heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier;
                meshData.vertices[vertexIndex] = new Vector3((topLeftX + x * meshScale), vertexHeight, (topLeftZ - y * meshScale));
                // Set UV coordinates for texturing
                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);
                
                // Add two triangles per quadrant, except for the last row and column
                if (x < width - 1 && y < height -1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + width + 1, vertexIndex + width);
                    meshData.AddTriangle(vertexIndex + width + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }
        return meshData;
    }
}

public class MeshData
{
    public Vector3[] vertices; // Vertex positions
    public int[] triangles; // Triangle indices
    public Vector2[] uvs;  // UV coordinates

    int triangleIndex; // Tracks where to insert the triangle

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6]; // 2 triangles per quad, 3 indices each
    }

    // Adds a triangle to the triangle array
    public void AddTriangle (int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex+1] = b;
        triangles[triangleIndex+2] = c;
        triangleIndex += 3;
    }

    // Creates a Unity Mesh from the MeshData
    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals(); // Recalcuate the normals for lighting
        return mesh;
    }
}
