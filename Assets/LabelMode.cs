using UnityEngine;

public class LabelMode : MonoBehaviour
{
    public static bool labelMode = false;

    public void ToggleLabelMode()
    {
        bool nextState = !labelMode;
        labelMode = nextState;

        if (nextState)
        {
            CutRuntimeController cutController = FindFirstObjectByType<CutRuntimeController>();
            if (cutController != null)
                cutController.ExitCutMode();

            InteractionMode.FreeCutActive = false;
        }

        Debug.Log("LABEL BUTTON CLICKED. Label Mode: " + labelMode);
    }
}