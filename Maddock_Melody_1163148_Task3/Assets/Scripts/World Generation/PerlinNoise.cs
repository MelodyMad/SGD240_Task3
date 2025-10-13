using UnityEngine;

/// <summary>
/// This script generates a Perlin noise map with support for multiple octaves, persistence, lacunarity, and offsets.
/// This allows realistic terrain height maps for procedural generation to be created.
/// </summary>

public static class PerlinNoise
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float [mapWidth, mapHeight];

        // Initialize pseudo-random number generator and octave offsets
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        // Prevent division by zero
        if (scale <=0)
        {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        // Generate noise values
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1; // Influence of this octave
                float frequency = 1; // Frequency of this octave
                float noiseHeight = 0; // Accumulated noise

                for (int i = 0; i < octaves; i++)
                {
                    // Calculate sample coordinates with offsets and frequency
                    float xCoord = (x-halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float yCoord = (y-halfHeight) / scale * frequency + octaveOffsets[i].y;

                    // Perlin noise in range [-1, 1]
                    float perlinValue = Mathf.PerlinNoise(xCoord, yCoord) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance; // Decrease amplitude for higher octaves
                    frequency *= lacunarity; // Increase frequency for higher octaves
                }
                // Track min/max for normalization
                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;
            }
        }

        // Normalize the noise map to [0, 1]
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }
}
