using UnityEngine;
using System.Collections.Generic;

public class FreeCutProcessor : MonoBehaviour
{
    public GameObject targetHeart;
    public PatchManager patchManager;
    public Material modelMaterial;

    public bool allowCut = false;

    public GameObject GetCurrentTargetHeart()
    {
        if (targetHeart == null)
            targetHeart = GetActiveHeart();

        return targetHeart;
    }

    GameObject GetActiveHeart()
    {
        GameObject holder = GameObject.Find("ModelHolder");
        if (holder == null) return null;
        if (holder.transform.childCount == 0) return null;

        for (int i = holder.transform.childCount - 1; i >= 0; i--)
        {
            Transform model = holder.transform.GetChild(i);
            if (model == null || !model.gameObject.activeInHierarchy)
                continue;

            if (model.GetComponent<MeshFilter>() != null)
                return model.gameObject;
        }

        return null;
    }

    public void ProcessCut(List<Vector3> points, Vector3 viewDirection)
    {
        if (!allowCut) return;
        if (points == null || points.Count < 3) return;

        targetHeart = GetCurrentTargetHeart();
        if (targetHeart == null) return;

        MeshFilter mf = targetHeart.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Mesh previousMesh = Instantiate(mf.sharedMesh);
        Mesh mesh = Instantiate(mf.sharedMesh);
        mf.sharedMesh = mesh;

        if (MeshUndoManager.Instance != null)
            MeshUndoManager.Instance.SaveState(mesh);

        CutResult result = LocalMeshEraser.RemoveInsideStroke(mesh, targetHeart.transform, points, cam);

        if (result == null || result.kept == null || result.removed == null)
        {
            allowCut = false;
            return;
        }

        if (result.kept.vertexCount == 0 || result.removed.vertexCount == 0)
        {
            allowCut = false;
            return;
        }

        mf.sharedMesh = result.kept;

        MeshCollider col = targetHeart.GetComponent<MeshCollider>();
        if (col != null)
        {
            col.sharedMesh = null;
            col.sharedMesh = result.kept;
        }

        if (patchManager != null)
            patchManager.SavePatch(previousMesh, result.removed, modelMaterial);

        allowCut = false;
    }
}