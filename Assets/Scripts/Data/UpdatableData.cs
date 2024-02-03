using UnityEngine;

/// <summary>
/// Base class for scriptable objects with updatable values.
/// </summary>
public class UpdatableData : ScriptableObject
{
    /// <summary>
    /// Event triggered when the values of the scriptable object are updated.
    /// </summary>
    public event System.Action OnValuesUpdated;

    /// <summary>
    /// Determines whether the values should automatically update when changed in the Unity Editor.
    /// </summary>
    public bool autoUpdate;

#if UNITY_EDITOR

    /// <summary>
    /// Called when the scriptable object is loaded or a value is changed in the Inspector.
    /// If autoUpdate is enabled, schedules the NotifyOfUpdateValues method to be called after the shader is done compiling.
    /// </summary>
    protected virtual void OnValidate()
    {
        if (autoUpdate)
        {
            // Delay the callback until the shader is done compiling
            UnityEditor.EditorApplication.update += NotifyOfUpdateValues;
        }
    }

    /// <summary>
    /// Notifies subscribers that the values of the scriptable object have been updated.
    /// </summary>
    public void NotifyOfUpdateValues()
    {
        UnityEditor.EditorApplication.update -= NotifyOfUpdateValues;

        // Trigger the update event if there are subscribers
        OnValuesUpdated?.Invoke();
    }

#endif
}