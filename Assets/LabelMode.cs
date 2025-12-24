using UnityEngine;

public class LabelMode : MonoBehaviour
{
    public static bool labelMode = false;

    public void ToggleLabelMode()
    {
        labelMode = !labelMode;
        Debug.Log("LABEL BUTTON CLICKED. Label Mode: " + labelMode);
    }
}
