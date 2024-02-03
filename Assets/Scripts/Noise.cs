using UnityEngine;

/// <summary>
/// A static class for generating procedural noise maps.
/// </summary>
public static class Noise
{
    /// <summary>
    /// The mode used for normalizing the generated noise values.
    /// </summary>
    public enum NormalizeMode
    {
        Local,
        Global // Estimate global min & max value
    }
    
    /// <summary>
    /// Generates a 2D noise map using specified settings and the sample center.
    /// </summary>
    /// <param name="mapWidth">Width of the noise map.</param>
    /// <param name="mapHeight">Height of the noise map.</param>
    /// <param name="settings">Settings for the noise generation.</param>
    /// <param name="sampleCenter">Center of the sample area.</param>
    /// <returns>Generated 2D noise map.</returns>
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, NoiseSettings settings, Vector2 sampleCenter)
    {
        System.Random prng = new System.Random(settings.seed);
        Vector2[] octaveOffsets = new Vector2[settings.octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        
        // Calculate octave offsets
        for (int i = 0; i < settings.octaves; ++i)
        {
            float offsetX = prng.Next(-100000, 100000) + settings.offset.x + sampleCenter.x;
            float offsetY = prng.Next(-100000, 100000) - settings.offset.y - sampleCenter.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= settings.persistence;
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;
        float[,] noiseMap = new float[mapWidth, mapHeight];

        PerlinNoiseGenerator perlinGenerator = new PerlinNoiseGenerator(settings.seed);
        
        // Generate noise values
        for (int y = 0; y < mapHeight; ++y)
        {
            for (int x = 0; x < mapWidth; ++x)
            {
                amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                // Combine octaves
                for (int i = 0; i < settings.octaves; ++i)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / settings.scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / settings.scale * frequency;

                    //float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    float perlinValue = perlinGenerator.Generate(sampleX, sampleY);
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= settings.persistence;
                    frequency *= settings.lacunarity;
                }

                // Track min and max heights for normalization
                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;

                // Normalize if in global mode
                if (settings.normalizeMode == NormalizeMode.Global)
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / maxPossibleHeight;
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);                    
                }
            }
        }

        // Normalize if in local mode using local min & max height
        if (settings.normalizeMode == NormalizeMode.Local)
        {
            for (int y = 0; y < mapHeight; ++y)
            {
                for (int x = 0; x < mapWidth; ++x)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                }
            }
        }
        
        return noiseMap;
    }
}

/// <summary>
/// Settings for a Noise generator used in procedural terrain generation.
/// </summary>
[System.Serializable]
public class NoiseSettings
{
    /// <summary>
    /// The mode used for normalizing the generated noise values.
    /// </summary>
    public Noise.NormalizeMode normalizeMode;

    /// <summary>
    /// The scale of the noise. Larger values result in more zoomed-out, smoother patterns.
    /// </summary>
    [Range(0.01f, 100f)]
    public float scale = 50;

    /// <summary>
    /// The number of octaves determines the level of detail in the noise. Higher octaves add finer details.
    /// </summary>
    [Range(1, 10)]
    public int octaves = 6;

    /// <summary>
    /// Persistence controls how quickly the amplitudes diminish for each successive octave in the noise.
    /// </summary>
    [Range(0, 1)]
    public float persistence = 0.6f;

    /// <summary>
    /// Lacunarity controls how quickly the frequency increases for each successive octave in the noise.
    /// </summary>
    public float lacunarity = 2;

    /// <summary>
    /// The seed used for generating the noise. Changing the seed will produce different patterns.
    /// </summary>
    public int seed;

    /// <summary>
    /// The offset applied to the noise generation, useful for creating different variations of terrain.
    /// </summary>
    public Vector2 offset;

    /// <summary>
    /// Validates and clamps values to ensure they are within acceptable ranges.
    /// </summary>
    public void ValidateValues()
    {
        scale = Mathf.Max(scale, 0.01f);
        octaves = Mathf.Max(octaves, 1);
        lacunarity = Mathf.Max(lacunarity, 1);
        persistence = Mathf.Clamp01(persistence);
    }
}
