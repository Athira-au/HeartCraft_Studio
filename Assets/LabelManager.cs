using UnityEngine;
using System.Collections.Generic;

public class LabelManager : MonoBehaviour
{
    public static LabelManager Instance;

    private Stack<GameObject> labelStack = new Stack<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    // Called when a new label is created
    public void RegisterLabel(GameObject label)
    {
        labelStack.Push(label);
    }

    // UNDO → remove last label
    public void UndoLabel()
    {
        if (labelStack.Count == 0) return;

        GameObject last = labelStack.Pop();
        if (last != null)
            Destroy(last);
    }

    // RESET → remove all labels
    public void ResetLabels()
    {
        while (labelStack.Count > 0)
        {
            GameObject lbl = labelStack.Pop();
            if (lbl != null)
                Destroy(lbl);
        }
    }
}
