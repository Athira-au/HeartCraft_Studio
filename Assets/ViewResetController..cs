using UnityEngine;

public class ViewResetController : MonoBehaviour
{
    public Transform modelHolder;
    public ForceCameraToModel cameraController;

    Quaternion initialRotation;

    void Start()
    {
        if (modelHolder != null)
            initialRotation = modelHolder.rotation;
    }

    public void ResetView()
    {
        if (modelHolder != null)
            modelHolder.rotation = initialRotation;

        if (cameraController != null)
            cameraController.ResetZoom();
    }
}
