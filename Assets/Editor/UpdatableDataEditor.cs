using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom editor for UpdatableData, designed to be used with derived classes.
/// </summary>
[CustomEditor(typeof(UpdatableData), true)]
public class UpdatableDataEditor : Editor
{
    /// <summary>
    /// Override of the default inspector GUI method to add custom functionality.
    /// </summary>
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        UpdatableData data = (UpdatableData)target;

        // Button to manually trigger an update and mark the target as dirty
        if (GUILayout.Button("Update"))
        {
            data.NotifyOfUpdateValues();
            EditorUtility.SetDirty(target);
        }
    }
}