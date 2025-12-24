using UnityEngine;

public class ForceCameraToModel : MonoBehaviour
{
    public Transform target;   // ModelHolder
    public float distance = 300f;

    [Header("Zoom")]
    public float zoomSpeed = 200f;
    public float minDistance = 50f;
    public float maxDistance = 600f;

    Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        // ---------- ZOOM ----------
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    void LateUpdate()
    {
        if (target == null || cam == null) return;

        cam.transform.position = target.position - cam.transform.forward * distance;
        cam.transform.LookAt(target.position);
    }
    public void ResetZoom()
    {
        distance = 300f;   // same as your default distance
    }

}
