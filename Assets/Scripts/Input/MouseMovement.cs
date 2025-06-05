using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    // A float is just enough here :D Don't need any double
    private float xRotation = 0f;
    private float yRotation = 0f;

    public float top_clamp = 90f;
    public float bottom_clamp = -90f;

    public float mouseSens = 500f;

    public Transform visionObject;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * mouseSens;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * mouseSens;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, bottom_clamp, top_clamp);

        visionObject.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
    }

    private void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
