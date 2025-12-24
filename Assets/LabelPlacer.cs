using UnityEngine;
using TMPro;

public class LabelPlacer : MonoBehaviour
{
    public Camera cam;
    public Transform modelHolder;
    public GameObject labelPrefab;

    void Start()
    {
        // Auto-assign Main Camera if not set
        if (cam == null)
            cam = Camera.main;
    }

    void Update()
{
    // Must be in label mode
    if (!LabelMode.labelMode) return;

    // 🔒 Popup open → do NOT allow new labels
    if (LabelPopup.Instance != null &&
        LabelPopup.Instance.gameObject.activeSelf)
        return;

    if (Input.GetMouseButtonDown(0))
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            PlaceLabel(hit.point, hit.normal);
        }
    }
}

void PlaceLabel(Vector3 hitPoint, Vector3 hitNormal)
{
    GameObject label = Instantiate(labelPrefab, hitPoint, Quaternion.identity);
    LabelManager.Instance.RegisterLabel(label);
    label.transform.SetParent(modelHolder, true);

    label.transform.position = hitPoint + hitNormal *0.1f;
    label.transform.localScale = Vector3.one *0.2f;

    if (label.GetComponent<Billboard>() == null)
        label.AddComponent<Billboard>();

    // 🔥 CLEAR DEFAULT TEXT
    TMP_Text txt = label.GetComponentInChildren<TMP_Text>();
    if (txt != null)
        txt.text = "";

    LabelPopup.Instance.Open(label);
}




}
