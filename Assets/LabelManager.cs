using UnityEngine;
using System.Collections.Generic;

public class LabelManager : MonoBehaviour
{
    public static LabelManager Instance;

    private Stack<GameObject> labelStack = new Stack<GameObject>();
    private CutRuntimeController cutController;

    void Awake()
    {
        Instance = this;

        cutController = GetComponent<CutRuntimeController>();

        if (cutController == null)
            cutController = gameObject.AddComponent<CutRuntimeController>();
    }

    public void RegisterLabel(GameObject label)
    {
        labelStack.Push(label);
    }

    public void UndoLabel()
    {
        if (labelStack.Count == 0)
            return;

        GameObject last = labelStack.Pop();
        if (last != null)
            Destroy(last);
    }

    public void ResetLabels()
    {
        while (labelStack.Count > 0)
        {
            GameObject lbl = labelStack.Pop();
            if (lbl != null)
                Destroy(lbl);
        }
    }

    // ✅ THESE CALL YOUR CUT SYSTEM
    public void SetCutPlaneX()
    {
        if (cutController != null)
            cutController.SetCutPlaneX();
    }

    public void SetCutPlaneY()
    {
        if (cutController != null)
            cutController.SetCutPlaneY();
    }

    public void SetCutPlaneZ()
    {
        if (cutController != null)
            cutController.SetCutPlaneZ();
    }
}