using UnityEngine;
using SFB;

public class STLRuntimeLoader : MonoBehaviour
{
    public Transform modelHolder;
    public Material modelMaterial;

    public void UploadAndLoadSTL()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel(
            "Select STL file", "", "stl", false);

        if (paths.Length == 0)
            return;

        Mesh mesh = STLImporter.LoadSTL(paths[0]);

        if (mesh == null)
        {
            Debug.LogError("STL load failed");
            return;
        }

        ReplaceModel(mesh);
    }

    void ReplaceModel(Mesh mesh)
    {
        for (int i = modelHolder.childCount - 1; i >= 0; i--)
            Destroy(modelHolder.GetChild(i).gameObject);

        PatchManager patchManager = FindFirstObjectByType<PatchManager>();
        if (patchManager != null)
            patchManager.ClearAllPatches();

        if (MeshUndoManager.Instance != null)
            MeshUndoManager.Instance.Clear();

        mesh.RecalculateBounds();
        RecenterMesh(mesh);

        GameObject model = new GameObject("RuntimeSTLModel");
        model.transform.SetParent(modelHolder, false);

        MeshFilter mf = model.AddComponent<MeshFilter>();
        MeshRenderer mr = model.AddComponent<MeshRenderer>();
        MeshCollider col = model.AddComponent<MeshCollider>();

        mf.mesh = mesh;
        mr.material = modelMaterial;
        col.sharedMesh = mesh;

        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one;

        FreeCutProcessor processor = FindFirstObjectByType<FreeCutProcessor>();
        if (processor != null)
            processor.targetHeart = model;

        CutRuntimeController cutController = FindFirstObjectByType<CutRuntimeController>();
        if (cutController != null)
        {
            cutController.RefreshSetup();
            cutController.CaptureCurrentModelAsOriginal();
        }
    }

    void RecenterMesh(Mesh mesh)
    {
        Vector3[] verts = mesh.vertices;
        Vector3 center = mesh.bounds.center;

        for (int i = 0; i < verts.Length; i++)
            verts[i] -= center;

        mesh.vertices = verts;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}
