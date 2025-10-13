using UnityEngine;

/// <summary>
/// This script handles displaying the maps in the scene, either as a texture on a plane or as a mesh with a texture.
/// </summary>

public class MapDisplay : MonoBehaviour
{
    // Renderer for displaying a simple texture map
    public Renderer textureRender; 

    // Components for displaying a 3D mesh map
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    // Draw a 2D texture map on the assigned renderer
    public void DrawTexture(Texture2D texture)
    {
        textureRender.sharedMaterial.mainTexture = texture;
        // Adjust the plane scale to match the texture size
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    // Draw a 3D mesh map with an applied texture where the mesh data contains the verticies and triangles
    public void DrawMesh(MeshData meshData, Texture2D texture)
    {
        // Create and assign the mesh
        meshFilter.sharedMesh = meshData.CreateMesh();
        // Apply the texture to the mesh material
        meshRenderer.sharedMaterial.mainTexture = texture;
    }

}
