using UnityEngine;

/// <summary>
/// This script keeps this GameObject's position synched with a target transform.
/// </summary>

public class CameraHolder : MonoBehaviour
{
    public Transform cameraPosition; // The target transform to follow

    // Sync this object's position with the target's position every frame
    private void Update()
    {
        transform.position = cameraPosition.position;
    }

}
