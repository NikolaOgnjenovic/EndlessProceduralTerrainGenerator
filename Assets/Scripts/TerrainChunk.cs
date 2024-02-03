using UnityEngine;

/// <summary>
/// Represents a chunk of terrain in the game world.
/// </summary>
public class TerrainChunk
{
    /// <summary>
    /// Coordinates of the terrain chunk in the world.
    /// </summary>
    public Vector2 coord;
    
    /// <summary>
    /// Event triggered when the visibility of the terrain chunk changes.
    /// </summary>
    public event System.Action<TerrainChunk, bool> onVisibilityChanged;
    
    /// <summary>
    /// The distance threshold for generating colliders.
    /// </summary>
    private const float colliderGenerationDistanceThreshold = 5;

    private GameObject meshObject;
    private Vector2 sampleCenter;
    private Bounds bounds;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private LODInfo[] detailLevels;
    private LODMesh[] lodMeshes;
    private int colliderLODIndex;
    private HeightMap heightMap;
    private bool heightMapReceived;
    private int previousLodIndex = -1;
    private bool hasSetCollider;
    private float maxViewDistance;
    private HeightMapSettings heightMapSettings;
    private MeshSettings meshSettings;
    private Transform viewer;
    
    /// <summary>
    /// Constructor for TerrainChunk.
    /// </summary>
    /// <param name="coord">Coordinates of the terrain chunk.</param>
    /// <param name="heightMapSettings">Settings for generating height maps.</param>
    /// <param name="meshSettings">Settings for generating terrain meshes.</param>
    /// <param name="detailLevels">Array of LOD (Level of Detail) settings.</param>
    /// <param name="colliderLODIndex">Index of the collider LOD.</param>
    /// <param name="parent">Parent transform for the terrain chunk.</param>
    /// <param name="viewer">Transform of the viewer.</param>
    /// <param name="material">Material used for rendering.</param>
    public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings,
        LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Transform viewer, Material material)
    {
        this.coord = coord;
        this.detailLevels = detailLevels;
        this.colliderLODIndex = colliderLODIndex;
        this.heightMapSettings = heightMapSettings;
        this.meshSettings = meshSettings;
        this.viewer = viewer;

        sampleCenter = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
        Vector2 positionV2 = coord * meshSettings.meshWorldSize;
        bounds = new Bounds(positionV2, Vector2.one * meshSettings.meshWorldSize);

        meshObject = new GameObject("Terrain Chunk");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();
        meshRenderer.material = material;

        meshObject.transform.position = new Vector3(positionV2.x, 0, positionV2.y);
        meshObject.transform.parent = parent;
        SetVisible(false);

        lodMeshes = new LODMesh[detailLevels.Length];
        for (int i = 0; i < detailLevels.Length; ++i)
        {
            lodMeshes[i] = new LODMesh(detailLevels[i].lod);
            lodMeshes[i].updateCallback += UpdateTerrainChunk;
            if (i == colliderLODIndex)
            {
                lodMeshes[i].updateCallback += UpdateCollisionMesh;
            }
        }

        maxViewDistance = this.detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
    }

    /// <summary>
    /// Initiates the process of loading the terrain chunk.
    /// </summary>
    public void Load()
    {
        ThreadedDataRequester.RequestData(
            () => HeightMapGenerator.GenerateHeightMap(meshSettings.numVerticesPerLine, meshSettings.numVerticesPerLine,
                heightMapSettings, meshSettings, sampleCenter), OnHeightMapReceived);
    }
    
    void OnHeightMapReceived(object heightMapObject)
    {
        heightMap = (HeightMap) heightMapObject;
        heightMapReceived = true;

        UpdateTerrainChunk();
    }

    private Vector2 viewerPosition => new(viewer.position.x, viewer.position.z);

    /// <summary>
    /// Updates the terrain chunk based on the viewer's position.
    /// </summary>
    public void UpdateTerrainChunk()
    {
        if (!heightMapReceived)
        {
            return;
        }

        float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
        bool wasVisible = this.isVisible();
        bool isVisible = viewerDistanceFromNearestEdge <= maxViewDistance;

        if (isVisible)
        {
            int lodIndex = 0;
            for (int i = 0; i < detailLevels.Length - 1; ++i)
            {
                if (viewerDistanceFromNearestEdge <= detailLevels[i].visibleDistanceThreshold)
                {
                    break;
                }

                lodIndex = i + 1;
            }

            if (lodIndex != previousLodIndex)
            {
                LODMesh lodMesh = lodMeshes[lodIndex];
                if (lodMesh.hasMesh)
                {
                    previousLodIndex = lodIndex;
                    meshFilter.mesh = lodMesh.mesh;
                }
                else if (!lodMesh.hasRequestedMesh)
                {
                    lodMesh.RequestMesh(heightMap, meshSettings);
                }
            }
        }

        if (wasVisible != isVisible)
        {
            SetVisible(isVisible);
            
            if (onVisibilityChanged != null)
            {
                onVisibilityChanged(this, isVisible);
            }
        }
    }
    
    /// <summary>
    /// Updates the collision mesh for the terrain chunk.
    /// </summary>
    public void UpdateCollisionMesh()
    {
        if (hasSetCollider)
        {
            return;
        }

        float sqrDistanceFromViewerToEdge = bounds.SqrDistance(viewerPosition);

        if (sqrDistanceFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDistanceThreshold)
        {
            if (!lodMeshes[colliderLODIndex].hasRequestedMesh)
            {
                lodMeshes[colliderLODIndex].RequestMesh(heightMap, meshSettings);
            }
        }

        if (sqrDistanceFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold)
        {
            if (lodMeshes[colliderLODIndex].hasMesh)
            {
                meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                hasSetCollider = true;
            }
        }
    }

    /// <summary>
    /// Sets the visibility state of the terrain chunk.
    /// </summary>
    /// <param name="visible">The visibility state to set.</param>
    private void SetVisible(bool visible)
    {
        meshObject.SetActive(visible);
    }

    /// <summary>
    /// Checks if the terrain chunk is currently visible.
    /// </summary>
    /// <returns>True if the terrain chunk is visible, false otherwise.</returns>
    private bool isVisible()
    {
        return meshObject.activeSelf;
    }
}

/// <summary>
/// Represents a LOD (Level of Detail) mesh for a terrain chunk.
/// </summary>
class LODMesh
{
    /// <summary>
    /// The generated mesh.
    /// </summary>
    public Mesh mesh;
    
    /// <summary>
    /// Flag indicating whether a mesh has been requested.
    /// </summary>
    public bool hasRequestedMesh;
    
    /// <summary>
    /// Flag indicating whether a mesh has been generated.
    /// </summary>
    public bool hasMesh;
    
    /// <summary>
    /// The LOD of the mesh.
    /// </summary>
    private int lod;
    
    public event System.Action updateCallback;

    /// <summary>
    /// Constructor for LODMesh.
    /// </summary>
    /// <param name="lod">The LOD level of the mesh.</param>
    public LODMesh(int lod)
    {
        this.lod = lod;
    }

    /// <summary>
    /// Callback invoked when mesh data is received.
    /// </summary>
    /// <param name="meshDataObject">The received mesh data.</param>
    private void OnMeshDataReceived(object meshDataObject)
    {
        mesh = ((MeshData)meshDataObject).CreateMesh();
        hasMesh = true;

        updateCallback();
    }

    /// <summary>
    /// Requests mesh data for the LOD level.
    /// </summary>
    /// <param name="heightMap">The height map data.</param>
    /// <param name="meshSettings">The mesh generation settings.</param>
    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
    {
        hasRequestedMesh = true;
        ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod),
            OnMeshDataReceived);
    }
}