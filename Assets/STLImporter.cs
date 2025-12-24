using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class STLImporter
{
    public static Mesh LoadSTL(string path)
    {
        if (!File.Exists(path))
            return null;

        using (BinaryReader br = new BinaryReader(File.OpenRead(path)))
        {
            br.ReadBytes(80);
            uint triangleCount = br.ReadUInt32();

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            for (int i = 0; i < triangleCount; i++)
            {
                br.ReadSingle(); br.ReadSingle(); br.ReadSingle();

                Vector3 v1 = ReadVector(br);
                Vector3 v2 = ReadVector(br);
                Vector3 v3 = ReadVector(br);

                int index = vertices.Count;
                vertices.Add(v1);
                vertices.Add(v2);
                vertices.Add(v3);

                triangles.Add(index);
                triangles.Add(index + 1);
                triangles.Add(index + 2);

                br.ReadUInt16();
            }

            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;


            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }
    }

    static Vector3 ReadVector(BinaryReader br)
    {
        return new Vector3(
            br.ReadSingle(),
            br.ReadSingle(),
            br.ReadSingle()
        );
    }
}
