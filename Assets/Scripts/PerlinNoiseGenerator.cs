using UnityEngine;

/// <summary>
/// Generates Perlin noise based on 2D coordinates.
/// </summary>
public class PerlinNoiseGenerator
{
    private int[] permutation;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerlinNoiseGenerator"/> class with a given seed.
    /// </summary>
    /// <param name="seed">The seed used for generating random permutations.</param>
    public PerlinNoiseGenerator(int seed)
    {
        permutation = GeneratePermutationTable(seed);
    }

    /// <summary>
    /// Generates Perlin noise at the specified coordinates.
    /// </summary>
    /// <param name="x">The x-coordinate of the noise point.</param>
    /// <param name="y">The y-coordinate of the noise point.</param>
    /// <returns>The Perlin noise value at the specified coordinates.</returns>
    public float Generate(float x, float y)
    {
        int X = Mathf.FloorToInt(x) & 255;
        int Y = Mathf.FloorToInt(y) & 255;

        x -= Mathf.Floor(x);
        y -= Mathf.Floor(y);

        float u = Fade(x);
        float v = Fade(y);

        int A = permutation[X] + Y;
        int AA = permutation[A];
        int AB = permutation[A + 1];
        int B = permutation[X + 1] + Y;
        int BA = permutation[B];
        int BB = permutation[B + 1];

        return Mathf.Lerp(
            Mathf.Lerp(Grad(AA, x, y), Grad(BA, x - 1, y), u),
            Mathf.Lerp(Grad(AB, x, y - 1), Grad(BB, x - 1, y - 1), u),
            v
        ) * 0.5f + 0.5f;
    }

    /// <summary>
    /// Performs fade interpolation for smoother transitions.
    /// </summary>
    /// <param name="t">The interpolation parameter.</param>
    /// <returns>The faded interpolation value.</returns>
    private float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    /// <summary>
    /// Calculates the gradient value for a given hash value and 2D coordinates.
    /// </summary>
    /// <param name="hash">The hash value determining the gradient direction.</param>
    /// <param name="x">The x-coordinate of the gradient point.</param>
    /// <param name="y">The y-coordinate of the gradient point.</param>
    /// <returns>The gradient value at the specified coordinates.</returns>
    private float Grad(int hash, float x, float y)
    {
        int h = hash & 7; // Gradient value 1-4
        float u = h < 4 ? x : y; // Choose x or y axis
        float v = h < 4 ? y : x; // Choose x or y axis
        return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -2.0f * v : 2.0f * v); // Randomly invert half of them
    }

    /// <summary>
    /// Generates a permutation table based on the specified seed.
    /// </summary>
    /// <param name="seed">The seed used for generating the permutation table.</param>
    /// <returns>The generated permutation table.</returns>
    private int[] GeneratePermutationTable(int seed)
    {
        int[] permutation = new int[512];
        int[] p = new int[256];

        for (int i = 0; i < 256; i++)
        {
            p[i] = i;
        }

        // Shuffle the array based on the seed
        System.Random random = new System.Random(seed);

        for (int i = 0; i < 256; i++)
        {
            int randomIndex = random.Next(256 - i);
            permutation[i] = p[randomIndex];
            p[randomIndex] = p[256 - i - 1];
            permutation[i + 256] = permutation[i];
        }

        return permutation;
    }
}