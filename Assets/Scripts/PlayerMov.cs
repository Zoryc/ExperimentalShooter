using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMov : MonoBehaviour
{
    private CharacterController characterController;

    private float gravity;

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

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>(); // Already defined in Unity
        gravity = -9.81f * mass;
    }

    // Update is called once per frame
    void Update()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        float x = Input.GetAxis("Horizontal"); // red
        float z = Input.GetAxis("Vertical"); // blue

        Vector3 move = new Vector3(); // transform.right * x + transform.forward * z
        move.x = transform.right.x * x + transform.forward.x * z;
        move.z = transform.right.z * x + transform.forward.z * z;

        characterController.Move(move * speed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); // why Sqrt and not sqrt???!?
            Debug.Log("Jumped!");
        } else if (isGrounded)
        {
            // if is grounded
            velocity.y = 0f;
        } else
        {
            // Falling down
            velocity.y += gravity * Time.deltaTime;
        }

        // Executing the jump
        characterController.Move(velocity * Time.deltaTime);

        if (lastPosition != gameObject.transform.position && isGrounded)
        {
            isMoving = true;
        }
        else {
            isMoving = false;
        }

        lastPosition = gameObject.transform.position;
    }
}
