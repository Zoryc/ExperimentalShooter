using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float walkingSpeed = 12f;
    public float runningSpeed = 18f;

    public float jumpHeight = 3f;

    public Transform visionObject;

    private CustomCharacterPhysics controller;

    void Start()
    {
        controller = this.GetComponent<CustomCharacterPhysics>();
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if ((horizontal != 0.0 || vertical != 0.0))
        {
            Vector2 matrixRotation = MatrixOperation.Matrix_Rotation(vertical, horizontal, (visionObject.eulerAngles.y * Mathf.Deg2Rad));
            Vector2 speedDirection = new Vector3(matrixRotation.y, matrixRotation.x);

            if (Input.GetKey(KeyCode.LeftShift))
            {
                speedDirection *= runningSpeed;
            }
            else
            {
                speedDirection *= walkingSpeed;
            }

            controller.Move(speedDirection);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            controller.Jump(jumpHeight);
        }
    }
}
