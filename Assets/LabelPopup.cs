using UnityEngine;
using TMPro;

public class LabelPopup : MonoBehaviour
{
    public static LabelPopup Instance;

    public TMP_InputField nameInput;

    GameObject currentLabel;

    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Open(GameObject label)
    {
        currentLabel = label;
        currentLabel.SetActive(true);
        nameInput.text = "";
        gameObject.SetActive(true);
    }

    public void OnConfirm()
    {
      if (currentLabel == null) return;

    TMP_Text txt = currentLabel.GetComponentInChildren<TMP_Text>();

    if (txt != null && !string.IsNullOrEmpty(nameInput.text))
        txt.text = nameInput.text;

    LabelMode.labelMode = false;
    Close();
    }

    public void OnCancel()
    {
        if (currentLabel != null)
            Destroy(currentLabel);

        // 🔒 Exit label mode on cancel as well
        LabelMode.labelMode = false;

        Close();
    }

    void Close()
    {
        currentLabel = null;
        gameObject.SetActive(false);
    }
}
