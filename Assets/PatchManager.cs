using UnityEngine;
using System.Collections.Generic;

public class PatchManager : MonoBehaviour
{
    private Transform modelHolder;
    private Stack<Mesh> previousMeshes = new Stack<Mesh>();

    void Awake()
    {
        GameObject holder = GameObject.Find("ModelHolder");
        if (holder != null)
            modelHolder = holder.transform;
    }

    // Store each previous full mesh state so Patch can step backward through cuts.
    public void SavePatch(Mesh meshBeforeCut, Mesh removedMesh, Material mat)
    {
        if (meshBeforeCut == null)
            return;

        previousMeshes.Push(Instantiate(meshBeforeCut));
        ClearPatchPieces();
    }

    // Restore the previous full mesh onto the current active model.
    public void RestorePatch()
    {
        if (previousMeshes.Count == 0)
            return;

        Transform model = GetActiveModel();
        if (model == null)
            return;

        MeshFilter mf = model.GetComponent<MeshFilter>();
        if (mf == null)
            return;

        Mesh restored = Instantiate(previousMeshes.Pop());
        mf.mesh = restored;

        MeshCollider col = model.GetComponent<MeshCollider>();
        if (col != null)
        {
            col.sharedMesh = null;
            col.sharedMesh = restored;
        }

        ClearPatchPieces();
    }

    public void ClearAllPatches()
    {
        previousMeshes.Clear();
        ClearPatchPieces();
    }

    private void ClearPatchPieces()
    {
        if (modelHolder == null)
            return;

        for (int i = modelHolder.childCount - 1; i >= 0; i--)
        {
            Transform child = modelHolder.GetChild(i);
            if (child != null && child.name == "PatchPiece")
                Destroy(child.gameObject);
        }
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
