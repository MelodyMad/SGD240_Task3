using UnityEngine;

/// <summary>
/// This script handles first-person camer rotation based on mouse movement, which rotates the player's orientation object to match the camera's horizontal rotation.
/// </summary>

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float xSensitivity; // Mouse sensitivity for vertical movement
    [SerializeField] private float ySensitivity; // Mouse sensitivity for horizontal movement

    public Transform orientation; // Reference to player orientation 

    private float xRotation; // Current vertical rotation
    private float yRotation; // Current horizontal rotation

    // When the game starts
    private void Start()
    {
        // Lock and hide cursor for first-person control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // On every frame
    private void Update()
    {
        // Get raw mouse input and apply sensitivity & frame time
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * xSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * ySensitivity;

        // Update rotations based on mouse input
        yRotation += mouseX;
        xRotation -= mouseY;

        // Clamp vertical rotation to avoid flipping the camera
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply camera and orientation rotations
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

}
