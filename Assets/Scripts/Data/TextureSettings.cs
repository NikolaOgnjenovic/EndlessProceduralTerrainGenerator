using UnityEngine;
using System.Linq;

/// <summary>
/// Settings for generating and applying textures to a terrain.
/// </summary>
[CreateAssetMenu]
public class TextureSettings : UpdatableData
{
    // Constants for the texture size and format
    private const int textureSize = 512;
    private const TextureFormat textureFormat = TextureFormat.RGB565;

    // Array of layers defining different textures and their properties
    public Layer[] layers;

    // Variables to store the saved min and max height for updating mesh heights
    private float savedMinHeight;
    private float savedMaxHeight;

    /// <summary>
    /// Applies texture settings to a material, updating various properties like colors, heights, and textures.
    /// </summary>
    /// <param name="material">The material to apply the texture settings to.</param>
    public void ApplyToMaterial(Material material)
    {
        material.SetInt("layerCount", layers.Length);
        material.SetColorArray("baseColors", layers.Select(x => x.tint).ToArray());
        material.SetFloatArray("baseStartHeights", layers.Select(x => x.startHeight).ToArray());
        material.SetFloatArray("baseBlends", layers.Select(x => x.blendStrength).ToArray());
        material.SetFloatArray("baseColorStrengths", layers.Select(x => x.tintStrength).ToArray());
        material.SetFloatArray("baseTextureScales", layers.Select(x => x.textureScale).ToArray());

        // Generate a texture array from the provided textures and set it in the material
        Texture2DArray texture2DArray = GenerateTextureArray(layers.Select(x => x.texture2D).ToArray());
        material.SetTexture("baseTextures", texture2DArray);

        // Update mesh heights in the material using saved min and max height values
        UpdateMeshHeights(material, savedMinHeight, savedMaxHeight);
    }

    /// <summary>
    /// Updates mesh heights in a material, setting the minimum and maximum height values.
    /// </summary>
    /// <param name="material">The material to update.</param>
    /// <param name="minHeight">The minimum height value.</param>
    /// <param name="maxHeight">The maximum height value.</param>
    public void UpdateMeshHeights(Material material, float minHeight, float maxHeight)
    {
        savedMinHeight = minHeight;
        savedMaxHeight = maxHeight;

        // Set the minimum and maximum height values in the material
        material.SetFloat("minHeight", minHeight);
        material.SetFloat("maxHeight", maxHeight);
    }

    /// <summary>
    /// Generates a texture array from an array of 2D textures.
    /// </summary>
    /// <param name="textures">Array of 2D textures.</param>
    /// <returns>The generated texture array.</returns>
    Texture2DArray GenerateTextureArray(Texture2D[] textures)
    {
        Texture2DArray texture2DArray =
            new Texture2DArray(textureSize, textureSize, textures.Length, textureFormat, true);

        // Set pixels for each texture in the texture array
        for (int i = 0; i < textures.Length; ++i)
        {
            texture2DArray.SetPixels(textures[i].GetPixels(), i);
        }

        texture2DArray.Apply();
        return texture2DArray;
    }

    /// <summary>
    /// Represents a layer with properties for a terrain texture.
    /// </summary>
    [System.Serializable]
    public class Layer
    {
        // Texture for the layer
        public Texture2D texture2D;

        // Tint color for the layer
        public Color tint;

        // Strength of the tint color
        [Range(0, 1)]
        public float tintStrength;

        // Starting height for blending the layer onto the terrain
        [Range(0, 1)]
        public float startHeight;

        // Strength of the blending between layers
        [Range(0, 1)]
        public float blendStrength;

        // Scale of the texture on the terrain
        public float textureScale;
    }
}