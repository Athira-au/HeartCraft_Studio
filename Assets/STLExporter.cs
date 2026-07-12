using UnityEngine;
using System.IO;
using SFB;

public class STLExporter : MonoBehaviour
{
    public Transform modelHolder;

    public void ExportSTL()
    {
        if (modelHolder == null || modelHolder.childCount == 0)
        {
            Debug.LogWarning("No model to export.");
            return;
        }

        MeshFilter mf = modelHolder.GetChild(0).GetComponent<MeshFilter>();
        if (mf == null)
        {
            Debug.LogWarning("Mesh not found.");
            return;
        }

        Mesh mesh = mf.mesh;

        var extensions = new[]
        {
            new ExtensionFilter("STL File", "stl")
        };

        string path = StandaloneFileBrowser.SaveFilePanel(
            "Save STL File",
            "",
            "exported_model",
            extensions
        );

        if (!string.IsNullOrEmpty(path))
        {
            WriteSTL(mesh, path);
            Debug.Log("STL exported to: " + path);
        }
    }

    void WriteSTL(Mesh mesh, string path)
    {
        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine("solid unity");

            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 v1 = vertices[triangles[i]];
                Vector3 v2 = vertices[triangles[i + 1]];
                Vector3 v3 = vertices[triangles[i + 2]];

                Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;

                writer.WriteLine($"facet normal {normal.x} {normal.y} {normal.z}");
                writer.WriteLine("outer loop");

                writer.WriteLine($"vertex {v1.x} {v1.y} {v1.z}");
                writer.WriteLine($"vertex {v2.x} {v2.y} {v2.z}");
                writer.WriteLine($"vertex {v3.x} {v3.y} {v3.z}");

                writer.WriteLine("endloop");
                writer.WriteLine("endfacet");
            }

            writer.WriteLine("endsolid unity");
        }
    }
}