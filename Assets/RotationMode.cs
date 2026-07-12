using UnityEngine;

public class RotationMode : MonoBehaviour
{
    public enum Mode
    {
        Free,
        XY,
        XZ,
        YZ
    }

    public static Mode currentMode = Mode.Free;

    public void SetFree() => currentMode = Mode.Free;
    public void SetXY()   => currentMode = Mode.XY;
    public void SetXZ()   => currentMode = Mode.XZ;
    public void SetYZ()   => currentMode = Mode.YZ;
}
