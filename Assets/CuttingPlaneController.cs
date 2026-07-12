using UnityEngine;

public class CuttingPlaneController : MonoBehaviour
{
    public float moveSpeed = 50f;
    public float rotateSpeed = 5f;

    void Update()
    {
        // Only active in Cut Mode
        if (EditMode.currentMode != EditMode.Mode.Cut)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        HandleMovement();
        HandleRotation();
    }

    void HandleMovement()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            transform.position += transform.forward * scroll * moveSpeed;
        }
    }

    void HandleRotation()
    {
        if (!Input.GetMouseButton(1)) return;

        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        transform.Rotate(Vector3.up, mx * rotateSpeed, Space.World);
        transform.Rotate(Vector3.right, -my * rotateSpeed, Space.World);
    }
}
