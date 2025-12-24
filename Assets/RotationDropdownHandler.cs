using UnityEngine;
using TMPro;

public class RotationDropdownHandler : MonoBehaviour
{
    TMP_Dropdown dropdown;

    void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();

        if (dropdown == null)
        {
            Debug.LogError("TMP_Dropdown component not found on this GameObject");
            return;
        }

        dropdown.onValueChanged.AddListener(OnRotationChanged);
    }

    void OnRotationChanged(int value)
    {
        switch (value)
        {
            case 0:
                RotationMode.currentMode = RotationMode.Mode.Free;
                break;
            case 1:
                RotationMode.currentMode = RotationMode.Mode.XY;
                break;
            case 2:
                RotationMode.currentMode = RotationMode.Mode.XZ;
                break;
            case 3:
                RotationMode.currentMode = RotationMode.Mode.YZ;
                break;
        }
    }
}
