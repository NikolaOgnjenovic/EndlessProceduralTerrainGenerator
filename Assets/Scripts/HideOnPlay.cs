using UnityEngine;

/// <summary>
/// Utility class to hide the GameObject it is attached to when the game starts.
/// </summary>
public class HideOnPlay : MonoBehaviour
{
    /// <summary>
    /// Called when the script instance is being loaded.
    /// Hides the GameObject by deactivating it.
    /// </summary>
    void Start()
    {
        gameObject.SetActive(false);
    }
}