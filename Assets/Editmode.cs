using UnityEngine;

public class EditMode : MonoBehaviour
{
    public enum Mode
    {
        None,
        Cut
    }

    public static Mode currentMode = Mode.None;

    public void EnableCutMode()
    {
        currentMode = Mode.Cut;
        Debug.Log("CUT MODE ENABLED");
    }

    public void DisableEditMode()
    {
        currentMode = Mode.None;
        Debug.Log("EDIT MODE DISABLED");
    }
}
