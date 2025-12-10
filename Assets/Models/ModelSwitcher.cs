using UnityEngine;

public class ModelSwitcher : MonoBehaviour
{
    public GameObject defaultModel;
    public GameObject myModel;

    public void ShowDefault()
    {
        defaultModel.SetActive(true);
        myModel.SetActive(false);
    }

    public void ShowMyModel()
    {
        defaultModel.SetActive(false);
        myModel.SetActive(true);
    }
}
