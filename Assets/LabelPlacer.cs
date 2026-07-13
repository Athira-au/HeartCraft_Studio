using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class LabelPlacer : MonoBehaviour
{
    public Camera cam;
    public Transform modelHolder;
    public GameObject labelPrefab;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;
    }

    void Update()
    {
        if (!LabelMode.labelMode)
            return;

        if (LabelPopup.Instance != null && LabelPopup.Instance.gameObject.activeSelf)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        Transform activeModel = GetActiveModel();
        if (activeModel == null)
            return;

        MeshCollider activeCollider = activeModel.GetComponent<MeshCollider>();
        if (activeCollider == null)
            activeCollider = activeModel.gameObject.AddComponent<MeshCollider>();

        MeshFilter activeFilter = activeModel.GetComponent<MeshFilter>();
        if (activeFilter == null || activeFilter.sharedMesh == null)
            return;

        activeCollider.sharedMesh = null;
        activeCollider.sharedMesh = activeFilter.sharedMesh;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (activeCollider.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            PlaceLabel(hit.point, hit.normal);
    }

    private Transform GetActiveModel()
    {
        if (modelHolder == null)
        {
            GameObject holder = GameObject.Find("ModelHolder");
            if (holder != null)
                modelHolder = holder.transform;
        }

        if (modelHolder == null || modelHolder.childCount == 0)
            return null;

        return modelHolder.GetChild(0);
    }

    void PlaceLabel(Vector3 hitPoint, Vector3 hitNormal)
    {
        if (labelPrefab == null || LabelManager.Instance == null || LabelPopup.Instance == null)
            return;

        GameObject label = Instantiate(labelPrefab, hitPoint, Quaternion.identity);
        LabelManager.Instance.RegisterLabel(label);

        label.transform.SetParent(modelHolder, true);
        label.transform.position = hitPoint + hitNormal * 0.1f;
        label.transform.localScale = Vector3.one * 0.2f;

        if (label.GetComponent<Billboard>() == null)
            label.AddComponent<Billboard>();

        TMP_Text txt = label.GetComponentInChildren<TMP_Text>();
        if (txt != null)
            txt.text = "";

        label.SetActive(true);
        LabelPopup.Instance.Open(label);
    }
}
