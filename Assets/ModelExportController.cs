using UnityEngine;
using System.IO;
using SFB;   // Standalone File Browser

public class ModelExportController : MonoBehaviour
{
    public Transform modelHolder;

    public void ExportAsSTL()
    {
        if (modelHolder.childCount == 0)
        {
            Debug.LogError("No model to export");
            return;
        }

        GameObject model = modelHolder.GetChild(0).gameObject;
        MeshFilter mf = model.GetComponent<MeshFilter>();

        if (mf == null || mf.mesh == null)
        {
            Debug.LogError("No mesh found");
            return;
        }

        var path = StandaloneFileBrowser.SaveFilePanel(
            "Export STL",
            "",
            "model_export",
            "stl"
        );

        if (string.IsNullOrEmpty(path))
            return;

        WriteBinarySTL(path, mf.mesh, model.transform);
        Debug.Log("STL exported: " + path);
    }

    void WriteBinarySTL(string path, Mesh mesh, Transform t)
    {
        using (BinaryWriter bw = new BinaryWriter(File.Open(path, FileMode.Create)))
        {
            bw.Write(new byte[80]);              // Header
            bw.Write(mesh.triangles.Length / 3); // Triangle count

            Vector3[] verts = mesh.vertices;
            int[] tris = mesh.triangles;

            for (int i = 0; i < tris.Length; i += 3)
            {
                // Normal (ignored by most slicers)
                bw.Write(0f); bw.Write(0f); bw.Write(0f);

                WriteVertex(bw, t.TransformPoint(verts[tris[i]]));
                WriteVertex(bw, t.TransformPoint(verts[tris[i + 1]]));
                WriteVertex(bw, t.TransformPoint(verts[tris[i + 2]]));

                bw.Write((ushort)0);
            }
        }
    }

    void WriteVertex(BinaryWriter bw, Vector3 v)
    {
        bw.Write(v.x);
        bw.Write(v.y);
        bw.Write(v.z);
    }
}
