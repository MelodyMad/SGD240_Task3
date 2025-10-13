using UnityEngine;

/// <summary>
/// This script handles the first-person player movement, which includes walking, sprinting, jumping and slope handling.
/// </summary>

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed; 
    [SerializeField] private float sprintSpeed;
    private float moveSpeed; // Current speed depending on state
    [SerializeField] private float groundDrag; // Drag applied when grounded

    [Header("Jumping")]
    [SerializeField] private float jumpForce; 
    [SerializeField] private float jumpCooldown; // Delay before being able to jump again
    [SerializeField] private float airMultiplier; // Speed multiplier when in air
    private bool readyToJump; // Flag to track jump cooldown

    [Header("Ground Check")]
    [SerializeField] private float playerHeight; // The player's height for ground detection
    [SerializeField] private LayerMask whatIsGround; // Layer mask to definine ground
    private bool grounded; // Check if the player is on the ground

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle; // Max slope angle player can walk on
    private RaycastHit slopeHit; // Infomation about the slope underneath the player
    private bool exitingSlope; // Flag to prevent jump issues on slopes

    [Header("Keybinds")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space; // Set key used for jumping, set to space bar
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift; // Set key used for sprinting, set to left shift

    public Transform orientation; // Player's orientation 

    private float horizontalInput; // Input axis for horizontal movement
    private float verticalInput; // Input axis for vertical movement
    private Vector3 moveDirection; // Movement direction
    private Rigidbody rigidBody; // Rigidbody component for physics

    public MovementState state; // Current movement state
    public enum MovementState { walking, sprinting, air } // Different movement states

    // When the game starts
    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>(); // Assign the Rigidbody
        rigidBody.freezeRotation = true; // Stop the rotation so the player does not fall over
        readyToJump = true; // The player can jump
    }

    // On every frame
    private void Update()
    {
        // Check if the player is grounded using a downward raycast
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        PlayerInput(); // Handle Input
        SpeedControl(); // Limit speed
        StateHandler(); // Update the movement state

        // Apply drag based on whether player is grounded
        rigidBody.linearDamping = grounded ? groundDrag : 0;

        // Increase gravity when falling
        if (rigidBody.linearVelocity.y < 0)
        {
            rigidBody.AddForce(Vector3.down * 15f, ForceMode.Acceleration);
        }
        else if (rigidBody.linearVelocity.y > 0 && !Input.GetKey(jumpKey))
        {
            // Create a shorter jump if the key is released early
            rigidBody.AddForce(Vector3.down * 10f, ForceMode.Acceleration);
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Jump Logic
        if (Input.GetKeyDown(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown); // Reset jump after cooldown
        }
    }

    private void MovePlayer()
    {
        // Calculate movement direction relative to the player's orientation
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Handling movement on slopes
        if (OnSlope() && !exitingSlope)
        {
            rigidBody.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);
            
            if (rigidBody.linearVelocity.y > 0)
            {
                rigidBody.AddForce(Vector3.down * 80f, ForceMode.Force); // To keep the player grounded
            }
        }

        // Handling movement when on the ground
        if (grounded)
        {
            rigidBody.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }

        // Handling movement when in the air
        else if (!grounded)
        {
            rigidBody.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        // Disable built-in gravity while on a slope to prevent sliding
        rigidBody.useGravity = !OnSlope();
    }

    // To change the different movement states
    private void StateHandler()
    {
        // Sprinting
        if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }
        // Walking
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        // Air
        else
        {
            state = MovementState.air;
        }
    }

    private void SpeedControl()
    {
        // Limit speed on a slopes
        if (OnSlope() && !exitingSlope)
        {
            if (rigidBody.linearVelocity.magnitude > moveSpeed)
            {
                rigidBody.linearVelocity = rigidBody.linearVelocity.normalized * moveSpeed;
            }
        }
        // Limit horizontal speed when on the ground or in the air
        else
        {
            Vector3 flatVelocity = new Vector3(rigidBody.linearVelocity.x, 0f, rigidBody.linearVelocity.z);
            // Limit velocity if required
            if (flatVelocity.magnitude > moveSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * moveSpeed;
                rigidBody.linearVelocity = new Vector3(limitedVelocity.x, rigidBody.linearVelocity.y, limitedVelocity.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true; // To prevent slope issues

        // Reset verticle velocity before jumping
        rigidBody.linearVelocity = new Vector3(rigidBody.linearVelocity.x, 0f, rigidBody.linearVelocity.z);
        // Apply the jump force
        rigidBody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    private bool OnSlope()
    {
        // Check if the player is standing on a slope
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        // Project movement direction onto the slope plane
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

}
