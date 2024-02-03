using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom editor for the MapPreview component.
/// </summary>
[CustomEditor(typeof(MapPreview))]
public class MapPreviewEditor : Editor
{
    /// <summary>
    /// Called to draw the inspector GUI for the MapPreview component.
    /// </summary>
    public override void OnInspectorGUI()
    {
        MapPreview mapPreview = (MapPreview)target;

        // Draw the default inspector GUI and check if any values have changed
        if (DrawDefaultInspector())
        {
            // If autoUpdate is enabled, redraw the map in the editor
            if (mapPreview.autoUpdate)
            {
                mapPreview.DrawMapInEditor();
            }
        }

        // Button to manually generate the map
        if (GUILayout.Button("Generate"))
        {
            mapPreview.DrawMapInEditor();
        }
    }
}