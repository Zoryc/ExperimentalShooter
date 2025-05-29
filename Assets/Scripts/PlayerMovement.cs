using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;

    private float gravityForce;

    public float speed = 12f;
    public float mass = 2f;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    Vector3 velocity;

    bool isGrounded;
    bool isMoving;

    private Vector3 lastPosition = new Vector3(0, 0, 0);

    void Start()
    {
        characterController = GetComponent<CharacterController>(); // Already defined in Unity
        gravityForce = Physics.gravity.y * mass;
    }

    void Update()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(); // transform.right * x + transform.forward * z
        move.x = transform.right.x * x + transform.forward.x * z;
        move.z = transform.right.z * x + transform.forward.z * z;

        characterController.Move(move * speed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityForce);
            Debug.Log("Jumped!");
        } else if (isGrounded)
        {
            // if is grounded
            velocity.y = 0f;
        } else
        {
            // Falling down
            velocity.y += gravityForce * Time.deltaTime;
        }

        // Executing the jump
        characterController.Move(velocity * Time.deltaTime);

        isMoving = lastPosition != gameObject.transform.position && isGrounded;
        lastPosition = gameObject.transform.position;
    }
}
