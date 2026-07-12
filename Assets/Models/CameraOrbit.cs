using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;     // what we look at
    public float distance = 5f;
    public float zoomSpeed = 2f;
    public float rotationSpeed = 100f;
    public float minDistance = 1f;
    public float maxDistance = 20f;

    float yaw = 0f;
    float pitch = 20f;

    void LateUpdate()
    {
        if (target == null) return;

        // Right mouse drag = rotate
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            yaw += mouseX * rotationSpeed * Time.deltaTime;
            pitch -= mouseY * rotationSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, -80f, 80f);
        }

        // Scroll = zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Move camera
        Vector3 dir = new Vector3(0, 0, -distance);
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0);
        transform.position = target.position + rot * dir;
        transform.LookAt(target);
    }
}
