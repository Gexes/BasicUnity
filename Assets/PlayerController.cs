using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Public variables can be adjusted in the Unity Inspector.

    [Tooltip("Speed of the player's movement. Controls how fast the player moves in the game world.")]
    public float moveSpeed = 5f;

    [Tooltip("Speed of the player's dash. When the player activates the dash, this determines how fast they move.")]
    public float dashSpeed = 15f;

    [Tooltip("Duration of the dash. How long the dash lasts before returning to normal speed.")]
    public float dashDuration = 0.2f;

    [Tooltip("Cooldown time between dashes. The player must wait this long before being able to dash again.")]
    public float dashCooldown = 1f;

    [Tooltip("Strength of the player's jump. Determines how high the player jumps when the jump is activated.")]
    public float jumpForce = 7f;

    [Tooltip("The distance to check for the ground using raycasting. Used to determine if the player is grounded.")]
    public float raycastDistance = 1.1f;

    [Tooltip("Defines what the ground is (LayerMask). Only objects on this layer are considered as ground for the player.")]
    public LayerMask groundLayer;

    [Tooltip("Time window after leaving the ground during which the player can still jump. Helps with smoother jumps.")]
    public float coyoteTime = 0.2f;

    // Private variables, not exposed to the Unity Inspector.
    private Rigidbody rb; // The Rigidbody component, used for applying physics to the player.
    private Transform cameraTransform; // The camera's position for relative movement.
    private bool isDashing = false; // Flag to check if the player is dashing.
    private float dashTimer = 0f; // Timer to keep track of the dash duration.
    private float lastDashTime = -1f; // Stores the last time the player dashed.
    private float lastGroundedTime = 0f; // Stores the last time the player was grounded.

    void Start()
    {
        // Get the Rigidbody component attached to the player.
        rb = GetComponent<Rigidbody>();

        // Get the main camera's transform to make the movement relative to the camera.
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        // Call the Move function to handle movement every frame.
        Move();

        // Update the last grounded time when the player is grounded.
        if (IsGrounded())
        {
            lastGroundedTime = Time.time;
        }

        // Allow the player to jump if they're grounded or within coyote time.
        if (Input.GetKeyDown(KeyCode.Space) && (IsGrounded() || Time.time - lastGroundedTime <= coyoteTime))
        {
            Jump();
        }

        // Check if the player presses Left Shift to start dashing, with a cooldown.
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= lastDashTime + dashCooldown)
        {
            StartDash();
        }
    }

    void Move()
    {
        // Get horizontal and vertical input (e.g., WASD keys or arrow keys).
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Get the camera's forward and right directions.
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Set the Y values to 0 to prevent the player from moving vertically based on camera rotation.
        forward.y = 0;
        right.y = 0;

        // Normalize the vectors to ensure uniform movement in all directions.
        forward.Normalize();
        right.Normalize();

        // Calculate the movement direction based on player input and camera orientation.
        Vector3 moveDirection = (forward * moveZ + right * moveX).normalized;

        // If the player is moving, set the velocity accordingly.
        if (moveDirection.magnitude > 0.1f)
        {
            // If dashing, use dash speed, else use normal move speed.
            float currentSpeed = isDashing ? dashSpeed : moveSpeed;

            // Apply the velocity in the direction the player is moving, preserving the Y velocity (for jumping).
            Vector3 moveVelocity = moveDirection * currentSpeed;
            rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);
        }
        else
        {
            // Stop horizontal movement if no input is given, but keep the Y velocity (for jumping).
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }

        // Handle dash timer and stop dashing when the timer ends.
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                isDashing = false;
            }
        }

        // Rotate the player to face the direction of movement.
        if (moveDirection.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    // Jump the player by setting the vertical velocity to the jump force.
    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
    }

    // Start the dash, setting the dash timer and updating the last dash time.
    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        lastDashTime = Time.time;
    }

    // Check if the player is grounded using a raycast to detect the ground.
    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, raycastDistance, groundLayer);
    }

    // Visualize the raycast in the editor for debugging purposes.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * raycastDistance);
    }
}
