using UnityEngine;
using System.Collections.Generic;

public class FreeCutDrawer : MonoBehaviour
{
    public Camera cam;
    public LineRenderer line;
    public FreeCutProcessor processor;

    public bool freeCutMode = false;

    public List<Vector3> points = new List<Vector3>();

    void Update()
    {
        if (!InteractionMode.FreeCutActive)
            return;

        if (Input.GetMouseButton(0))
        {
            if (TryGetActiveHeartHit(out RaycastHit hit))
            {
                if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], hit.point) > 0.01f)
                {
                    points.Add(hit.point);
                    DrawPreview(points, hit.normal);
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector3 viewDirection = cam != null ? cam.transform.forward : Vector3.forward;
            processor.ProcessCut(points, viewDirection);
            freeCutMode = false;
            InteractionMode.FreeCutActive = false;
            ModelRotateZoom.rotationLocked = false;

            points.Clear();
            line.positionCount = 0;
        }
    }

    public void EnableFreeCut()
    {
        freeCutMode = true;
        InteractionMode.FreeCutActive = true;
        ModelRotateZoom.rotationLocked = true;

        processor.allowCut = true;

        points.Clear();
        line.positionCount = 0;
    }

    void OnDisable()
    {
        InteractionMode.FreeCutActive = false;
        ModelRotateZoom.rotationLocked = false;
    }

    private bool TryGetActiveHeartHit(out RaycastHit hit)
    {
        hit = default;

        if (cam == null || processor == null)
            return false;

        GameObject activeHeart = processor.GetCurrentTargetHeart();
        if (activeHeart == null)
            return false;

        MeshCollider collider = activeHeart.GetComponent<MeshCollider>();
        MeshFilter filter = activeHeart.GetComponent<MeshFilter>();
        if (collider == null || filter == null || filter.sharedMesh == null)
            return false;

        collider.sharedMesh = null;
        collider.sharedMesh = filter.sharedMesh;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        return collider.Raycast(ray, out hit, Mathf.Infinity);
    }

    private void DrawPreview(List<Vector3> surfacePoints, Vector3 surfaceNormal)
    {
        if (line == null)
            return;

        Vector3[] previewPoints = new Vector3[surfacePoints.Count];
        Vector3 offset = surfaceNormal * 0.01f;

        for (int i = 0; i < surfacePoints.Count; i++)
            previewPoints[i] = surfacePoints[i] + offset;

        line.positionCount = previewPoints.Length;
        line.SetPositions(previewPoints);
    }
}