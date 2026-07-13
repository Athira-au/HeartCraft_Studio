using UnityEngine;
using TMPro;

public class PlanarCutDropdownHandler : MonoBehaviour
{
    public CutRuntimeController cutController;

    void Awake()
    {
        if (cutController == null)
            cutController = FindFirstObjectByType<CutRuntimeController>();
    }

    public void OnDropdownChanged(int value)
    {
        if (cutController == null)
            cutController = FindFirstObjectByType<CutRuntimeController>();

        if (cutController == null)
            return;

        if (value == 0)
            cutController.SetCutPlaneX();
        else if (value == 1)
            cutController.SetCutPlaneY();
        else if (value == 2)
            cutController.SetCutPlaneZ();
    }
}