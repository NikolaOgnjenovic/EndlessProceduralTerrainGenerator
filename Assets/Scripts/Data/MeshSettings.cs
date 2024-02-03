using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Settings for generating and rendering terrain meshes.
/// </summary>
[CreateAssetMenu]
public class MeshSettings : UpdatableData
{
    /// <summary>
    /// The number of supported Levels of Detail (LODs) for terrain meshes.
    /// </summary>
    public const int numSupportedLODs = 5;

    /// <summary>
    /// The number of supported chunk sizes for terrain meshes.
    /// </summary>
    public const int numSupportedChunkSizes = 9;

    /// <summary>
    /// The number of supported flat-shaded chunk sizes for terrain meshes.
    /// </summary>
    public const int numSupportedFlatshadedChunkSizes = 3;

    /// <summary>
    /// Array of supported chunk sizes for terrain meshes.
    /// </summary>
    public static readonly int[] supportedChunkSizes = { 48, 72, 96, 120, 144, 168, 192, 216, 240 };

    /// <summary>
    /// The scale of the terrain mesh on the X, Y, and Z axes.
    /// </summary>
    public float meshScale = 2.5f;

    /// <summary>
    /// Determines whether to use flat shading for the terrain mesh.
    /// </summary>
    public bool useFlatShading;

    /// <summary>
    /// Index of the chunk size to use for the terrain mesh.
    /// </summary>
    [Range(0, numSupportedChunkSizes - 1)]
    public int chunkSizeIndex;

    /// <summary>
    /// Index of the flat-shaded chunk size to use for the terrain mesh.
    /// </summary>
    [Range(0, numSupportedFlatshadedChunkSizes - 1)]
    public int flatshadedChunkSizeIndex;

    /// <summary>
    /// Number of vertices per line of a mesh rendered at LOD = 0.
    /// Includes the 2 extra vertices used for normal calculation that are excluded from the final mesh.
    /// </summary>
    public int numVerticesPerLine =>
        supportedChunkSizes[useFlatShading ? flatshadedChunkSizeIndex : chunkSizeIndex] + 5;

    /// <summary>
    /// Calculates the world size of the terrain mesh based on the number of vertices per line and mesh scale.
    /// </summary>
    public float meshWorldSize => (numVerticesPerLine - 3) * meshScale;
}
