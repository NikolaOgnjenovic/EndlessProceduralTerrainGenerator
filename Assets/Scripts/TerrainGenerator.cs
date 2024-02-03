using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates and manages terrain chunks based on the viewer's position.
/// </summary>
public class TerrainGenerator : MonoBehaviour
{
    private const float viewerMoveThresholdForChunkUpdate = 25f;
    private const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    /// <summary>
    /// Index of the collider Level of Detail (LOD).
    /// </summary>
    public int colliderLODIndex;

    /// <summary>
    /// Array of LODInfo structs representing different Level of Detail settings.
    /// </summary>
    public LODInfo[] detailLevels;

    /// <summary>
    /// Settings for generating terrain meshes.
    /// </summary>
    public MeshSettings meshSettings;

    /// <summary>
    /// Settings for generating height maps.
    /// </summary>
    public HeightMapSettings heightMapSettings;

    /// <summary>
    /// Settings for texturing the terrain.
    /// </summary>
    public TextureSettings textureSettings;

    /// <summary>
    /// Transform of the viewer in the scene.
    /// </summary>
    public Transform viewer;

    /// <summary>
    /// Material used for rendering the terrain.
    /// </summary>
    public Material terrainMaterial;

    private Vector2 viewerPosition;
    private Vector2 previousViewerPosition;

    private float meshWorldSize;
    private int chunksVisibleInViewDistance;

    private Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    private List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    /// <summary>
    /// Initializes the Terrain Generator, applies material settings, and updates visible chunks.
    /// </summary>
    void Start()
    {
        textureSettings.ApplyToMaterial(terrainMaterial);
        textureSettings.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        float maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        meshWorldSize = meshSettings.meshWorldSize;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / meshWorldSize);

        UpdateVisibleChunks();
    }

    /// <summary>
    /// Updates viewer position and triggers terrain chunk updates if necessary.
    /// </summary>
    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if (viewerPosition != previousViewerPosition)
        {
            foreach (TerrainChunk chunk in visibleTerrainChunks)
            {
                chunk.UpdateCollisionMesh();
            }
        }

        if ((previousViewerPosition - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            previousViewerPosition = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    /// <summary>
    /// Updates the visible terrain chunks based on the viewer's position.
    /// </summary>
    void UpdateVisibleChunks()
    {
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();
        for (int i = visibleTerrainChunks.Count - 1; i >= 0; --i)
        {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

        for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; ++yOffset)
        {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; ++xOffset)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
                {
                    continue;
                }

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    TerrainChunk terrainChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings,
                        detailLevels, colliderLODIndex, transform, viewer, terrainMaterial);
                    terrainChunkDictionary.Add(viewedChunkCoord, terrainChunk);
                    terrainChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
                    terrainChunk.Load();
                }
            }
        }
    }

    /// <summary>
    /// Handles the visibility change event of a terrain chunk.
    /// </summary>
    /// <param name="terrainChunk">The terrain chunk whose visibility changed.</param>
    /// <param name="isVisible">Whether the terrain chunk is now visible.</param>
    void OnTerrainChunkVisibilityChanged(TerrainChunk terrainChunk, bool isVisible)
    {
        if (isVisible)
        {
            visibleTerrainChunks.Add(terrainChunk);
        }
        else
        {
            visibleTerrainChunks.Remove(terrainChunk);
        }
    }
}

/// <summary>
/// Represents Level of Detail (LOD) settings for terrain generation.
/// </summary>
[System.Serializable]
public struct LODInfo
{
    /// <summary>
    /// Index of the LOD.
    /// </summary>
    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int lod;

    /// <summary>
    /// Distance threshold at which the LOD becomes visible.
    /// </summary>
    public float visibleDistanceThreshold;

    /// <summary>
    /// Square of the visible distance threshold for optimized distance comparisons.
    /// </summary>
    public float sqrVisibleDistanceThreshold => visibleDistanceThreshold * visibleDistanceThreshold;
}