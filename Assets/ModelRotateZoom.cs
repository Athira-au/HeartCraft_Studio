using UnityEngine;
using System.Collections.Generic;

public class ModelRotateZoom : MonoBehaviour
{
    public float rotateSpeed = 0.1f;
    public int maxUndoSteps = 20;

    public static bool rotationLocked = false;

    Stack<Quaternion> rotationHistory = new Stack<Quaternion>();

  void Update()
{
    if (rotationLocked || InteractionMode.FreeCutActive || EditMode.currentMode == EditMode.Mode.Cut)
        return;

    if (!Input.GetMouseButton(0))
        return;

    if (Input.GetMouseButtonDown(0))
    {
        rotationHistory.Push(transform.rotation);

        if (rotationHistory.Count > maxUndoSteps)
            rotationHistory.Clear();
    }

    float mx = Input.GetAxis("Mouse X") * rotateSpeed * 100f;
    float my = Input.GetAxis("Mouse Y") * rotateSpeed * 100f;

    switch (RotationMode.currentMode)
    {
        case RotationMode.Mode.Free:
            transform.Rotate(Vector3.up, -mx, Space.World);
            transform.Rotate(Vector3.right, my, Space.World);
            break;

        case RotationMode.Mode.XY:
            transform.Rotate(Vector3.forward, -mx, Space.World);
            break;

        case RotationMode.Mode.XZ:
            transform.Rotate(Vector3.up, -mx, Space.World);
            break;

        case RotationMode.Mode.YZ:
            transform.Rotate(Vector3.right, my, Space.World);
            break;
    }
}
public void UndoLastRotation()
    {
        if (rotationHistory.Count > 0)
            transform.rotation = rotationHistory.Pop();
    }
}
