using UnityEngine;

public static class PerlinNoise
{
    public static float[,] GenerateNoiseMap(int mapWidth, int MapHeight, float scale, int octaves, float persistance, float lacunarity)
    {
        float[,] noiseMap = new float [mapWidth, MapHeight];

        if (scale <=0)
        {
            scale = 0.0001f;
        }

        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float xCoord = x / scale * frequency;
                    float yCoord = y / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(xCoord, yCoord);
                    noiseMap[x, y] = perlinValue;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }
            }
        }
        return noiseMap;
    }



}
