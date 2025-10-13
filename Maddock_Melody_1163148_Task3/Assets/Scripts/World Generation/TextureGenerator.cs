using UnityEngine;

/// <summary>
/// This script generates a Texture2D objects from either colour maps or height maps.
/// </summary>

public static class TextureGenerator 
{
    // Create a texture from a Color array
    public static Texture2D TextureFromColourMap(Color[] colourmap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point; // To keep pixels sharp
        texture.wrapMode = TextureWrapMode.Clamp; // Clamp edges to avoid wrapping
        texture.SetPixels(colourmap); // Apply colours
        texture.Apply(); 
        return texture;
    }

    // Create a grayscale texture from a height map
    public static Texture2D TextureFromHeightMap(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Color[] colourMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Map height (0-1) to grayscale
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
            }
        }
        // Reuse the colour map function to create the texture
        return TextureFromColourMap(colourMap, width, height);
    }
}
