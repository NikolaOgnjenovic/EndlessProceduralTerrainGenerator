using UnityEngine;

/// <summary>
/// Utility class for generating height maps based on noise and settings.
/// </summary>
public static class HeightMapGenerator
{
    /// <summary>
    /// Generates a height map using noise and specified settings.
    /// </summary>
    /// <param name="width">Width of the height map.</param>
    /// <param name="height">Height of the height map.</param>
    /// <param name="heightMapSettings">Settings for generating the height map.</param>
    /// <param name="meshSettings">Settings for the terrain mesh.</param>
    /// <param name="sampleCenter">Center of the sample area.</param>
    /// <returns>Generated height map.</returns>
    public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings heightMapSettings, MeshSettings meshSettings, Vector2 sampleCenter)
    {
        // Generate a noise map using specified settings
        float[,] values = Noise.GenerateNoiseMap(width, height, heightMapSettings.noiseSettings, sampleCenter);

        // Create a thread-safe copy of the height curve
        AnimationCurve heightCurveThreadSafe = new AnimationCurve(heightMapSettings.heightCurve.keys);

        // Variables to track the min and max values in the height map
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        // Iterate through each point in the height map
        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                // Apply height curve and multiplier to the noise values
                values[i, j] *= heightCurveThreadSafe.Evaluate(values[i, j]) * heightMapSettings.heightMultiplier;

                // Apply falloff map if specified in settings
                if (heightMapSettings.useFalloff)
                {
                    // Uncomment the following lines to apply falloff map
                    // TODO: Find out why this crashes the editor
                    //float[,] falloffMap = FalloffGenerator.GenerateFalloffMap(meshSettings.numVerticesPerLine, heightMapSettings.falloffCurve);
                    //values[i, j] = Mathf.Clamp01(values[i, j] - falloffMap[i, j]);
                }

                // Track min and max values in the height map
                if (values[i, j] > maxValue)
                {
                    maxValue = values[i, j];
                }

                if (values[i, j] < minValue)
                {
                    minValue = values[i, j];
                }
            }
        }

        // Return a new HeightMap struct with the generated values, min, and max values
        return new HeightMap(values, minValue, maxValue);
    }
}

/// <summary>
/// Represents a height map with associated values, minimum, and maximum values.
/// </summary>
public struct HeightMap
{
    /// <summary>
    /// 2D array of height values in the height map.
    /// </summary>
    public readonly float[,] values;

    /// <summary>
    /// The minimum height value in the height map.
    /// </summary>
    public readonly float minValue;

    /// <summary>
    /// The maximum height value in the height map.
    /// </summary>
    public readonly float maxValue;

    /// <summary>
    /// Constructor for the HeightMap struct.
    /// </summary>
    /// <param name="values">2D array of height values.</param>
    /// <param name="minValue">Minimum height value.</param>
    /// <param name="maxValue">Maximum height value.</param>
    public HeightMap(float[,] values, float minValue, float maxValue)
    {
        this.values = values;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}