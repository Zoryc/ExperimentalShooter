using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    // A float is just enough here :D Don't need any double
    private float xRotation = 0f;
    private float yRotation = 0f;

    private Transform orientation;

    public float top_clamp = 90f;
    public float bottom_clamp = -90f;

    public float mouseSens = 500f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * mouseSens;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * mouseSens;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, bottom_clamp, top_clamp);

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
    }

    private void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
