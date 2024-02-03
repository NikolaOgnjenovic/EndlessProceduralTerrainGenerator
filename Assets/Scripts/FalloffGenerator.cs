using UnityEngine;

/// <summary>
/// Utility class for generating falloff maps to be used in terrain generation.
/// </summary>
public static class FalloffGenerator
{
    /// <summary>
    /// Generates a 2D falloff map based on the provided size and falloff curve.
    /// </summary>
    /// <param name="size">Size of the falloff map.</param>
    /// <param name="falloffCurve">Animation curve defining the falloff shape.</param>
    /// <returns>Generated falloff map.</returns>
    public static float[,] GenerateFalloffMap(int size, AnimationCurve falloffCurve)
    {
        // Create a copy of the provided falloff curve
        AnimationCurve falloffCurveThreadSafe = new AnimationCurve(falloffCurve.keys);

        // Initialize a 2D array for the falloff map
        float[,] map = new float[size, size];

        // Iterate through each point in the falloff map
        for (int i = 0; i < size; ++i)
        {
            for (int j = 0; j < size; ++j)
            {
                // Normalize x and y to the range [-1, 1]
                float x = i / (float)size * 2 - 1;
                float y = j / (float)size * 2 - 1;

                // Calculate the falloff value based on the distance from the center using the falloff curve
                float value = falloffCurveThreadSafe.Evaluate(Mathf.Sqrt(x * x + y * y));

                // Set the falloff value in the map
                map[i, j] = value;
            }
        }

        return map;
    }
}