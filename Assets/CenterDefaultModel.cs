using UnityEngine;

public class CenterDefaultModel : MonoBehaviour
{
    void Start()
    {
        MeshFilter mf = GetComponentInChildren<MeshFilter>();

        if (mf == null) return;

        Mesh mesh = Instantiate(mf.sharedMesh);
        mf.mesh = mesh;

        Vector3 center = mesh.bounds.center;
        Vector3 worldCenterOffset = mf.transform.TransformVector(center);

        // move mesh vertices
        Vector3[] verts = mesh.vertices;
        for (int i = 0; i < verts.Length; i++)
            verts[i] -= center;

        mesh.vertices = verts;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        // Keep initial visual placement without offsetting the mesh pivot.
        transform.position += worldCenterOffset;
    }
}
