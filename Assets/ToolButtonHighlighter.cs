using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class ToolButtonHighlighter : MonoBehaviour
{
    private readonly Dictionary<string, Button> buttons = new Dictionary<string, Button>();
    private readonly Dictionary<string, ColorBlock> defaultColors = new Dictionary<string, ColorBlock>();

    [SerializeField] private Color activeNormalColor = new Color(0.23f, 0.62f, 0.92f, 1f);
    [SerializeField] private Color activeHighlightedColor = new Color(0.30f, 0.70f, 1f, 1f);
    [SerializeField] private Color activePressedColor = new Color(0.18f, 0.52f, 0.82f, 1f);
    [SerializeField] private Color activeSelectedColor = new Color(0.23f, 0.62f, 0.92f, 1f);

    private CutRuntimeController cutController;
    private string currentActiveKey = string.Empty;

    void Awake()
    {
        cutController = GetComponent<CutRuntimeController>();
        RegisterButton("Label", "LabelButton");
        RegisterButton("FreeCut", "FreeCut");
        RegisterButton("PlanarX", "XPlaneButton");
        RegisterButton("PlanarY", "YPlaneButton");
        RegisterButton("PlanarZ", "ZPlaneButton");
        ApplyHighlights();
    }

    void Update()
    {
        TryRegisterMissingButtons();
        string nextActiveKey = ResolveActiveKey();
        if (nextActiveKey == currentActiveKey)
            return;

        currentActiveKey = nextActiveKey;
        ApplyHighlights();
    }

    private void TryRegisterMissingButtons()
    {
        RegisterButton("Label", "LabelButton");
        RegisterButton("FreeCut", "FreeCut");
        RegisterButton("PlanarX", "XPlaneButton");
        RegisterButton("PlanarY", "YPlaneButton");
        RegisterButton("PlanarZ", "ZPlaneButton");
    }
    private void RegisterButton(string key, string gameObjectName)
    {
        GameObject obj = GameObject.Find(gameObjectName);
        if (obj == null)
            return;

        Button button = obj.GetComponent<Button>();
        if (button == null)
            return;

        if (!buttons.ContainsKey(key))
            buttons[key] = button;

        if (!defaultColors.ContainsKey(key))
            defaultColors[key] = button.colors;
    }


    private string ResolveActiveKey()
    {
        if (LabelMode.labelMode)
            return "Label";

        if (InteractionMode.FreeCutActive)
            return "FreeCut";

        if (cutController == null)
            cutController = GetComponent<CutRuntimeController>();

        if (cutController == null)
            return string.Empty;

        switch (cutController.ActivePlanarTool)
        {
            case CutRuntimeController.PlanarTool.X:
                return "PlanarX";
            case CutRuntimeController.PlanarTool.Y:
                return "PlanarY";
            case CutRuntimeController.PlanarTool.Z:
                return "PlanarZ";
            default:
                return string.Empty;
        }
    }

    private void ApplyHighlights()
    {
        foreach (KeyValuePair<string, Button> pair in buttons)
        {
            if (pair.Value == null || !defaultColors.ContainsKey(pair.Key))
                continue;

            bool isActive = pair.Key == currentActiveKey;
            ColorBlock colors = isActive
                ? BuildActiveColors(defaultColors[pair.Key])
                : defaultColors[pair.Key];

            pair.Value.colors = colors;

            if (pair.Value.targetGraphic != null)
                pair.Value.targetGraphic.color = colors.normalColor;
        }
    }

    private ColorBlock BuildActiveColors(ColorBlock source)
    {
        source.normalColor = activeNormalColor;
        source.highlightedColor = activeHighlightedColor;
        source.pressedColor = activePressedColor;
        source.selectedColor = activeSelectedColor;
        return source;
    }
}