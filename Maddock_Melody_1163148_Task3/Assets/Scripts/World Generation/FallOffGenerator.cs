using UnityEngine;

/// <summary>
/// This script generates a 2D falloff map that transitions the nois smoothly from the centre to the edges.
/// </summary>

public static class FallOffGenerator 
{
    // Generates a falloff map of a given size based in the width and height of the map
    public static float[,] GenerateFallOffMap(int size)
    {
        float[,] map = new float[size, size];
        for (int i = 0; i < size; i ++)
        {
            for (int j = 0; j < size; j++)
            {
                // Normalise  the coordinates from -1 to 1
                float x = i / (float)(size-1) * 2 - 1;
                float y = j / (float)(size-1) * 2 - 1;

                // Get the max absolute distance from the centre
                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));

                // Apply the falloff curve
                map[i, j] = Evaluate(value);
            }
        }
        // Returns a 2D float array of values between 0 and 1
        return map;
    }

    // Evaluates how quickly the falloff increases from the centre to the edges of the map
    static float Evaluate (float value)
    {
        float a = 3f; // Controls the steepness of the curve
        float b = 2.2f; // Controls how far toward the edge the fall off begins
        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }

}
