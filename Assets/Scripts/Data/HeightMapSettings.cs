using UnityEngine;

/// <summary>
/// Settings for generating height maps used in procedural terrain generation.
/// </summary>
[CreateAssetMenu]
public class HeightMapSettings : UpdatableData
{
    /// <summary>
    /// Settings for the noise generator used to create the height map.
    /// </summary>
    public NoiseSettings noiseSettings;

    /// <summary>
    /// Determines whether to apply a falloff effect to the generated height map.
    /// </summary>
    public bool useFalloff;

    /// <summary>
    /// Multiplier applied to the height values, scaling on the Y axis.
    /// </summary>
    public float heightMultiplier;

    /// <summary>
    /// The curve used to shape the height values along the Y axis.
    /// </summary>
    public AnimationCurve heightCurve;

    /// <summary>
    /// The curve used to shape the falloff effect, if enabled.
    /// </summary>
    public AnimationCurve falloffCurve;

    /// <summary>
    /// The minimum height value of the height map, taking into account the height multiplier and curve.
    /// </summary>
    public float minHeight => heightMultiplier * heightCurve.Evaluate(0);

    /// <summary>
    /// The maximum height value of the height map, taking into account the height multiplier and curve.
    /// </summary>
    public float maxHeight => heightMultiplier * heightCurve.Evaluate(1);

#if UNITY_EDITOR
    /// <summary>
    /// Called when the script is loaded or a value is changed in the Inspector.
    /// Validates noise settings to ensure they are within acceptable ranges.
    /// </summary>
    protected override void OnValidate()
    {
        noiseSettings.ValidateValues();

        base.OnValidate();
    }
#endif
}