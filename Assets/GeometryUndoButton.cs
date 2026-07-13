using UnityEngine;

public class GeometryUndoButton : MonoBehaviour
{
    public Transform modelHolder;

    public void UndoCut()
    {
        if (MeshUndoManager.Instance == null || !MeshUndoManager.Instance.HasUndo())
            return;

        Transform model = GetActiveModel();
        if (model == null)
            return;

        MeshFilter mf = model.GetComponent<MeshFilter>();
        if (mf == null)
            return;

        Mesh previous = MeshUndoManager.Instance.Undo();
        if (previous != null)
            mf.mesh = previous;

        MeshCollider col = model.GetComponent<MeshCollider>();
        if (col != null)
            col.sharedMesh = mf.mesh;
    }

    private Transform GetActiveModel()
    {
        if (modelHolder == null || modelHolder.childCount == 0)
            return null;

        for (int i = modelHolder.childCount - 1; i >= 0; i--)
        {
            Transform child = modelHolder.GetChild(i);
            if (child == null || !child.gameObject.activeInHierarchy)
                continue;

            if (child.GetComponent<MeshFilter>() != null)
                return child;
        }

        return null;
    }
}
