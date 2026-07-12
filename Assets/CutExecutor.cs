using UnityEngine;

public class CutExecutor : MonoBehaviour
{
    public Transform modelHolder;
    public Transform cuttingPlane;

    private PatchManager patchManager;

    void Start()
    {
        patchManager = FindFirstObjectByType<PatchManager>();
    }

    public void ApplyCut()
    {
        Transform model = GetActiveModel();
        if (model == null || cuttingPlane == null)
            return;

        MeshFilter mf = model.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
            return;

        Mesh previousMesh = Instantiate(mf.sharedMesh);
        Mesh mesh = Instantiate(mf.sharedMesh);
        mf.sharedMesh = mesh;

        if (MeshUndoManager.Instance != null)
            MeshUndoManager.Instance.SaveState(mesh);

        Vector3 worldNormal = cuttingPlane.forward;
        Vector3 worldPoint = cuttingPlane.position;

        Vector3 localNormal = model.InverseTransformDirection(worldNormal).normalized;
        Vector3 localPoint = model.InverseTransformPoint(worldPoint);

        Plane plane = new Plane(localNormal, localPoint);

        CutResult result = MeshCutter.Cut(mesh, plane);

        if (result == null || result.kept == null || result.removed == null)
            return;

        if (result.kept.vertexCount == 0 || result.removed.vertexCount == 0)
            return;

        mf.sharedMesh = result.kept;

        MeshCollider col = model.GetComponent<MeshCollider>();
        if (col != null)
        {
            col.sharedMesh = null;
            col.sharedMesh = result.kept;
        }

        if (patchManager != null)
        {
            MeshRenderer mr = model.GetComponent<MeshRenderer>();
            Material mat = mr != null ? mr.sharedMaterial : null;
            patchManager.SavePatch(previousMesh, result.removed, mat);
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