using UnityEngine;
using System.Collections.Generic;

public static class LocalMeshEraser
{
    public static CutResult RemoveInsideStroke(Mesh mesh, Transform meshTransform, IReadOnlyList<Vector3> strokePointsWorld, Camera cam)
    {
        Vector3[] verts = mesh.vertices;
        int[] tris = mesh.triangles;

        List<Vector2> strokeScreen = BuildStrokeScreenPoints(strokePointsWorld, cam);
        if (strokeScreen.Count < 3)
            return new CutResult { kept = mesh, removed = new Mesh() };

        float averageStrokeDepth = ComputeAverageStrokeDepth(strokePointsWorld, cam);
        float depthTolerance = Mathf.Max(0.05f, averageStrokeDepth * 0.2f);

        List<Vector3> keepVerts = new List<Vector3>();
        List<int> keepTris = new List<int>();
        List<Vector3> removedVerts = new List<Vector3>();
        List<int> removedTris = new List<int>();

        Vector3 cameraForward = cam.transform.forward;

        for (int i = 0; i < tris.Length; i += 3)
        {
            Vector3 v0 = verts[tris[i]];
            Vector3 v1 = verts[tris[i + 1]];
            Vector3 v2 = verts[tris[i + 2]];

            if (ShouldRemoveTriangle(meshTransform, strokeScreen, cam, cameraForward, averageStrokeDepth, depthTolerance, v0, v1, v2))
                AddTriangle(removedVerts, removedTris, v0, v1, v2);
            else
                AddTriangle(keepVerts, keepTris, v0, v1, v2);
        }

        return new CutResult
        {
            kept = BuildMesh(keepVerts, keepTris),
            removed = BuildMesh(removedVerts, removedTris)
        };
    }

    private static List<Vector2> BuildStrokeScreenPoints(IReadOnlyList<Vector3> strokePointsWorld, Camera cam)
    {
        List<Vector2> result = new List<Vector2>(strokePointsWorld.Count);
        for (int i = 0; i < strokePointsWorld.Count; i++)
        {
            Vector3 screen = cam.WorldToScreenPoint(strokePointsWorld[i]);
            if (screen.z > 0f)
                result.Add(new Vector2(screen.x, screen.y));
        }
        return result;
    }

    private static float ComputeAverageStrokeDepth(IReadOnlyList<Vector3> strokePointsWorld, Camera cam)
    {
        float total = 0f;
        int count = 0;

        for (int i = 0; i < strokePointsWorld.Count; i++)
        {
            Vector3 screen = cam.WorldToScreenPoint(strokePointsWorld[i]);
            if (screen.z <= 0f)
                continue;

            total += screen.z;
            count++;
        }

        return count == 0 ? 0f : total / count;
    }

    private static bool ShouldRemoveTriangle(
        Transform meshTransform,
        IReadOnlyList<Vector2> strokeScreen,
        Camera cam,
        Vector3 cameraForward,
        float averageStrokeDepth,
        float depthTolerance,
        Vector3 v0,
        Vector3 v1,
        Vector3 v2)
    {
        Vector3 worldV0 = meshTransform.TransformPoint(v0);
        Vector3 worldV1 = meshTransform.TransformPoint(v1);
        Vector3 worldV2 = meshTransform.TransformPoint(v2);
        Vector3 centroid = (worldV0 + worldV1 + worldV2) / 3f;

        Vector3 screenCentroid = cam.WorldToScreenPoint(centroid);
        if (screenCentroid.z <= 0f)
            return false;

        if (Mathf.Abs(screenCentroid.z - averageStrokeDepth) > depthTolerance)
            return false;

        Vector3 normal = Vector3.Cross(worldV1 - worldV0, worldV2 - worldV0).normalized;
        if (Vector3.Dot(normal, cameraForward) >= 0f)
            return false;

        return IsPointInsidePolygon(new Vector2(screenCentroid.x, screenCentroid.y), strokeScreen);
    }

    private static bool IsPointInsidePolygon(Vector2 point, IReadOnlyList<Vector2> polygon)
    {
        bool inside = false;
        for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[j];

            bool intersects = ((a.y > point.y) != (b.y > point.y)) &&
                              (point.x < (b.x - a.x) * (point.y - a.y) / ((b.y - a.y) + Mathf.Epsilon) + a.x);
            if (intersects)
                inside = !inside;
        }

        return inside;
    }

    private static Mesh BuildMesh(List<Vector3> vertices, List<int> triangles)
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

    private static void AddTriangle(List<Vector3> vertices, List<int> triangles, Vector3 a, Vector3 b, Vector3 c)
    {
        int index = vertices.Count;
        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);
        triangles.Add(index);
        triangles.Add(index + 1);
        triangles.Add(index + 2);
    }
}