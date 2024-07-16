using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

public class ColorAccessibilityChecker : EditorWindow
{
    private List<string> warnings = new List<string>();

    [MenuItem("Window/Accessibility/Color Accessibility Checker")]
    public static void ShowWindow()
    {
        GetWindow<ColorAccessibilityChecker>("Color Accessibility Checker");
    }

    private void OnGUI()
    {
        GUILayout.Label("Color Accessibility Checker", EditorStyles.boldLabel);

        if (GUILayout.Button("Check Scene"))
        {
            CheckSceneForColorAccessibility();
        }

        GUILayout.Space(10);

        if (warnings.Count > 0)
        {
            GUILayout.Label("Warnings:", EditorStyles.boldLabel);

            foreach (string warning in warnings)
            {
                GUILayout.Label(warning, EditorStyles.wordWrappedLabel);
            }
        }
        else
        {
            GUILayout.Label("No issues found.", EditorStyles.wordWrappedLabel);
        }
    }

    private void CheckSceneForColorAccessibility()
    {
        warnings.Clear();

        // Get all UI components in the scene
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        List<Graphic> uiElements = new List<Graphic>();

        foreach (var canvas in canvases)
        {
            uiElements.AddRange(canvas.GetComponentsInChildren<Graphic>(true));
        }

        foreach (var element in uiElements)
        {
            CheckElementForColorAccessibility(element);
        }

        Debug.Log("Color accessibility check completed.");
    }

    private void CheckElementForColorAccessibility(Graphic element)
    {
        if (element is Text textElement)
        {
            // Check if the Text element is conveying information solely through its color
            if (string.IsNullOrEmpty(textElement.text.Trim()))
            {
                warnings.Add($"Text element '{element.name}' may rely on color alone.");
            }
        }
        else if (element is Image imageElement)
        {
            // Check if the Image element has additional indicators
            if (!HasAdditionalIndicators(imageElement))
            {
                warnings.Add($"Image element '{element.name}' may rely on color alone.");
            }
        }
    }

    private bool HasAdditionalIndicators(Image imageElement)
    {
        // Check for text labels or icons near the image element
        Transform parent = imageElement.transform.parent;

        foreach (Transform sibling in parent)
        {
            if (sibling != imageElement.transform)
            {
                if (sibling.GetComponent<Text>() != null)
                {
                    return true;
                }
                if (sibling.GetComponent<Image>() != null && sibling.GetComponent<Image>().sprite != null)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
