using UnityEngine;

/// <summary>
/// Handles previewing terrain maps in the Unity editor.
/// </summary>
public class MapPreview : MonoBehaviour
{
    public Renderer textureRender;
    public MeshFilter meshFilter;

    // Enumeration for different draw modes
    public enum DrawMode { NoiseMap, Mesh, FalloffMap };
    public DrawMode drawMode;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureSettings textureSettings;

    public Material terrainMaterial;

    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int editorPreviewLOD;

    // Toggle to automatically update the preview
    public bool autoUpdate;

    /// <summary>
    /// Draws the map in the Unity editor based on the selected draw mode.
    /// </summary>
    public void DrawMapInEditor()
    {
        // Apply texture settings to the terrain material
        textureSettings.ApplyToMaterial(terrainMaterial);
        
        // Update mesh heights in the terrain material
        textureSettings.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        // Generate a height map based on the mesh settings and height map settings
        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVerticesPerLine, meshSettings.numVerticesPerLine, heightMapSettings, meshSettings, Vector2.zero);

        // Choose the draw mode and visualize accordingly
        if (drawMode == DrawMode.NoiseMap)
        {
            DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD));
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(FalloffGenerator.GenerateFalloffMap(meshSettings.numVerticesPerLine, heightMapSettings.falloffCurve), 0, 1)));
        }
    }

    /// <summary>
    /// Draws a texture on the specified renderer.
    /// </summary>
    /// <param name="texture">The texture to be drawn.</param>
    public void DrawTexture(Texture2D texture)
    {
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height) / 10f;

        // Activate the texture renderer and deactivate the mesh filter
        textureRender.gameObject.SetActive(true);
        meshFilter.gameObject.SetActive(false);
    }

    /// <summary>
    /// Draws a mesh on the specified mesh filter.
    /// </summary>
    /// <param name="meshData">The mesh data to be drawn.</param>
    public void DrawMesh(MeshData meshData)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();

        // Activate the mesh filter and deactivate the texture renderer
        textureRender.gameObject.SetActive(false);
        meshFilter.gameObject.SetActive(true);
    }

    /// <summary>
    /// Callback method when the values are updated in the inspector.
    /// </summary>
    void OnValuesUpdated()
    {
        // Check if the application is not playing (editor mode)
        if (!Application.isPlaying)
        {
            // Redraw the map in the editor
            DrawMapInEditor();
        }
    }

    /// <summary>
    /// Callback method when texture values are updated in the inspector.
    /// </summary>
    void OnTextureValuesUpdated()
    {
        // Apply texture settings to the terrain material
        textureSettings.ApplyToMaterial(terrainMaterial);
    }

    /// <summary>
    /// Called when the script is loaded or a value is changed in the inspector.
    /// Sets up event callbacks for value changes.
    /// </summary>
    void OnValidate()
    {
        // Register callbacks for value changes in meshSettings, heightMapSettings, and textureSettings
        if (meshSettings != null)
        {
            meshSettings.OnValuesUpdated -= OnValuesUpdated;
            meshSettings.OnValuesUpdated += OnValuesUpdated;
        }

        if (heightMapSettings != null)
        {
            heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
            heightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }

        if (textureSettings != null)
        {
            textureSettings.OnValuesUpdated -= OnTextureValuesUpdated;
            textureSettings.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }
}