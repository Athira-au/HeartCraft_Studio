using UnityEngine;
using System.Collections.Generic;

public static class MeshCutter
{
    public static CutResult Cut(Mesh mesh, Plane plane)
    {
        Vector3[] verts = mesh.vertices;
        int[] tris = mesh.triangles;

        List<Vector3> keepVerts = new List<Vector3>();
        List<int> keepTris = new List<int>();
        List<Vector3> cutVerts = new List<Vector3>();
        List<int> cutTris = new List<int>();

        for (int i = 0; i < tris.Length; i += 3)
        {
            Vector3 v0 = verts[tris[i]];
            Vector3 v1 = verts[tris[i + 1]];
            Vector3 v2 = verts[tris[i + 2]];

            List<Vector3> triangle = new List<Vector3> { v0, v1, v2 };

            AddPolygon(keepVerts, keepTris, ClipPolygon(triangle, plane, true));
            AddPolygon(cutVerts, cutTris, ClipPolygon(triangle, plane, false));
        }

        return new CutResult()
        {
            kept = BuildMesh(keepVerts, keepTris),
            removed = BuildMesh(cutVerts, cutTris)
        };
    }

    static Mesh BuildMesh(List<Vector3> vertices, List<int> triangles)
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        if (vertices.Count == 0 || triangles.Count == 0)
            return mesh;

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    static void AddPolygon(List<Vector3> vertices, List<int> triangles, List<Vector3> polygon)
    {
        if (polygon == null || polygon.Count < 3)
            return;

        for (int i = 1; i < polygon.Count - 1; i++)
            AddTriangle(vertices, triangles, polygon[0], polygon[i], polygon[i + 1]);
    }

    static List<Vector3> ClipPolygon(List<Vector3> polygon, Plane plane, bool keepPositive)
    {
        const float epsilon = 0.00001f;
        List<Vector3> result = new List<Vector3>();

        for (int i = 0; i < polygon.Count; i++)
        {
            Vector3 current = polygon[i];
            Vector3 next = polygon[(i + 1) % polygon.Count];

            float currentDistance = plane.GetDistanceToPoint(current);
            float nextDistance = plane.GetDistanceToPoint(next);

            bool currentInside = keepPositive
                ? currentDistance >= -epsilon
                : currentDistance <= epsilon;
            bool nextInside = keepPositive
                ? nextDistance >= -epsilon
                : nextDistance <= epsilon;

            if (currentInside && nextInside)
            {
                result.Add(next);
                continue;
            }

            if (currentInside && !nextInside)
            {
                result.Add(Intersect(current, next, currentDistance, nextDistance));
                continue;
            }

            if (!currentInside && nextInside)
            {
                result.Add(Intersect(current, next, currentDistance, nextDistance));
                result.Add(next);
            }
        }

        return result;
    }

    static Vector3 Intersect(Vector3 a, Vector3 b, float distanceA, float distanceB)
    {
        float denominator = distanceA - distanceB;
        if (Mathf.Abs(denominator) < Mathf.Epsilon)
            return a;

        float t = distanceA / denominator;
        return Vector3.Lerp(a, b, t);
    }

    static void AddTriangle(List<Vector3> vertices, List<int> triangles, Vector3 a, Vector3 b, Vector3 c)
    {
        if (IsDegenerate(a, b, c))
            return;

        int index = vertices.Count;
        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);
        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);
    }

    static bool IsDegenerate(Vector3 a, Vector3 b, Vector3 c)
    {
        return Vector3.Cross(b - a, c - a).sqrMagnitude < 0.00000001f;
    }
}
