using UnityEngine;
using SFB;

public class STLRuntimeLoader : MonoBehaviour
{
    public Transform modelHolder;
    public Material modelMaterial;

    public void UploadAndLoadSTL()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel(
            "Select STL file",
            "",
            "stl",
            false
        );

        if (paths.Length == 0)
            return;

        Mesh mesh = STLImporter.LoadSTL(paths[0]);

        if (mesh == null)
        {
            Debug.LogError("STL load failed");
            return;
        }

        Debug.Log("Mesh loaded. Vertices: " + mesh.vertexCount);

        ReplaceModel(mesh);
    }

  void ReplaceModel(Mesh mesh)
{
    // Remove old models
    for (int i = modelHolder.childCount - 1; i >= 0; i--)
        Destroy(modelHolder.GetChild(i).gameObject);

    // 🔴 Recenter geometry itself
    mesh.RecalculateBounds();
    RecenterMesh(mesh);

    // Create model object
    GameObject model = new GameObject("RuntimeSTLModel");
    model.transform.SetParent(modelHolder, false);

    // Mesh components
    MeshFilter mf = model.AddComponent<MeshFilter>();
    MeshRenderer mr = model.AddComponent<MeshRenderer>();

    mf.mesh = mesh;
    mr.material = modelMaterial;

    // 🔴 THIS IS THE REQUIRED FIX (DO NOT SKIP)
    MeshCollider col = model.AddComponent<MeshCollider>();
    col.sharedMesh = mesh;

    // Keep transform clean
    model.transform.localPosition = Vector3.zero;
    model.transform.localRotation = Quaternion.identity;
    model.transform.localScale = Vector3.one;
}

void RecenterMesh(Mesh mesh)
{
    Vector3[] vertices = mesh.vertices;
    Vector3 center = mesh.bounds.center;

    for (int i = 0; i < vertices.Length; i++)
    {
        vertices[i] -= center;
    }

    mesh.vertices = vertices;
    mesh.RecalculateBounds();
    mesh.RecalculateNormals();
}


}
