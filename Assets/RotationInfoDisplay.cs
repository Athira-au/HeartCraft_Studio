using UnityEngine;
using TMPro;

public class RotationInfoDisplay : MonoBehaviour
{
    public Transform target;   // ModelHolder
    public TMP_Text text;

    void Update()
    {
        if (target == null || text == null) return;

        Vector3 euler = target.eulerAngles;

        // Convert Unity's 0–360 to -180–180 for readability
        euler.x = NormalizeAngle(euler.x);
        euler.y = NormalizeAngle(euler.y);
        euler.z = NormalizeAngle(euler.z);

        text.text =
            $"X : {euler.x:F1}°\n" +
            $"Y : {euler.y:F1}°\n" +
            $"Z : {euler.z:F1}°";
    }

    float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}
