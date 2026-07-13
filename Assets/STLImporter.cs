using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

public static class STLImporter
{
    public static Mesh LoadSTL(string path)
    {
        if (!File.Exists(path))
            return null;

        if (LooksLikeAsciiStl(path))
            return LoadAsciiStl(path);

        return LoadBinaryStl(path);
    }

    static Mesh LoadBinaryStl(string path)
    {
        using (BinaryReader br = new BinaryReader(File.OpenRead(path)))
        {
            br.ReadBytes(80);
            uint triangleCount = br.ReadUInt32();

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            for (int i = 0; i < triangleCount; i++)
            {
                br.ReadSingle();
                br.ReadSingle();
                br.ReadSingle();

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

            return BuildMesh(vertices, triangles);
        }
    }

    static Mesh LoadAsciiStl(string path)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        foreach (string rawLine in File.ReadLines(path))
        {
            string line = rawLine.Trim();
            if (!line.StartsWith("vertex "))
                continue;

            string[] parts = line.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4)
                continue;

            if (!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) ||
                !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) ||
                !float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
            {
                continue;
            }

            vertices.Add(new Vector3(x, y, z));
        }

        for (int i = 0; i + 2 < vertices.Count; i += 3)
        {
            triangles.Add(i);
            triangles.Add(i + 1);
            triangles.Add(i + 2);
        }

        if (vertices.Count == 0 || triangles.Count == 0)
            return null;

        return BuildMesh(vertices, triangles);
    }

    static Mesh BuildMesh(List<Vector3> vertices, List<int> triangles)
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    static bool LooksLikeAsciiStl(string path)
    {
        using (FileStream stream = File.OpenRead(path))
        {
            int sampleLength = (int)Mathf.Min(512, stream.Length);
            byte[] buffer = new byte[sampleLength];
            stream.Read(buffer, 0, sampleLength);

            string sample = System.Text.Encoding.ASCII.GetString(buffer).TrimStart();
            if (!sample.StartsWith("solid"))
                return false;

            return sample.Contains("facet") || sample.Contains("vertex");
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
