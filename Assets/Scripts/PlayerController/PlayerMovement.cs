using UnityEngine;

[RequireComponent(typeof(CustomCharacterPhysics))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkingSpeed = 12f;
    public float runningSpeed = 18f;

    [Header("Jump")]
    public float jumpHeight = 3f;

    [Header("References")]
    [Tooltip("Transform used to determine facing direction (usually the camera).")]
    public Transform visionObject;

    private CustomCharacterPhysics controller;

    // Ignore tiny stick drift instead of comparing floats to 0 with !=
    private const float InputDeadzone = 0.01f;

    // ================================================================
    // INITIALIZATION
    // ================================================================

    private void Awake()
    {
        controller = GetComponent<CustomCharacterPhysics>();

        if (controller == null)
        {
            Debug.LogError("[PlayerMovement] Missing CustomCharacterPhysics component.", this);
            enabled = false;
            return;
        }

        if (visionObject == null)
        {
            Debug.LogError("[PlayerMovement] visionObject has not been assigned.", this);
            enabled = false;
        }
    }

    // ================================================================
    // UPDATE
    // ================================================================

    private void Update()
    {
        HandleMovement();
        HandleJump();
    }

    // ================================================================
    // MOVEMENT
    // ================================================================

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector2 rawInput = new Vector2(horizontal, vertical);

        Vector2 moveDirection = Vector2.zero;

        // Skip the rotation math if there's effectively no input, instead
        // of an imprecise float != 0.0 check — but still fall through to
        // controller.Move() below.
        if (rawInput.sqrMagnitude >= InputDeadzone * InputDeadzone)
        {
            // Prevent diagonal movement (e.g. W+D) from being faster
            // than a single-axis movement (magnitude > 1).
            rawInput = Vector2.ClampMagnitude(rawInput, 1f);

            float yawRadians = visionObject.eulerAngles.y * Mathf.Deg2Rad;

            Vector2 rotatedInput = MatrixOperation.Matrix_Rotation(
                rawInput.y,
                rawInput.x,
                yawRadians
            );

            // Explicit Vector2 construction instead of relying on an
            // implicit Vector3 -> Vector2 cast.
            moveDirection = new Vector2(rotatedInput.y, rotatedInput.x);
        }

        float speed = Input.GetKey(KeyCode.LeftShift) ? runningSpeed : walkingSpeed;

        // IMPORTANT: always call Move(), even with a zero vector — the
        // controller uses this call to step gravity and ground detection
        // too, so skipping it while idle makes the character stop falling.
        controller.Move(moveDirection * speed);
    }

    // ================================================================
    // JUMP
    // ================================================================

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            controller.Jump(jumpHeight);
        }
    }
}
