using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class CameraObjectReporterEditor : EditorWindow
{
    private string reportText = "Press the button to generate the report.";
    private Vector2 scrollPosition;

    [MenuItem("Window/Generate Object Report")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(CameraObjectReporterEditor));
    }

    private void OnGUI()
    {
        GUILayout.Label("Object Visibility Report Generator", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate Report"))
        {
            GenerateReport();
        }

        EditorGUILayout.Space();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        EditorGUILayout.TextArea(reportText, GUILayout.Height(400));
        EditorGUILayout.EndScrollView();
    }

    private void GenerateReport()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            reportText = "No main camera found!";
            return;
        }

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        List<GameObject> visibleObjects = new List<GameObject>();

        foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null && GeometryUtility.TestPlanesAABB(planes, renderer.bounds))
            {
                visibleObjects.Add(obj);
            }

            // Check for UI elements without Renderer
            if (renderer == null)
            {
                RectTransform rectTransform = obj.GetComponent<RectTransform>();
                if (rectTransform != null && RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mainCamera.WorldToScreenPoint(rectTransform.position), mainCamera))
                {
                    visibleObjects.Add(obj);
                }
            }
        }

        WriteReport(visibleObjects);
    }

    private float CalculateContrastRatio(Color color1, Color color2)
    {
        float luminance1 = CalculateLuminance(color1);
        float luminance2 = CalculateLuminance(color2);
        float contrastRatio = (Mathf.Max(luminance1, luminance2) + 0.05f) / (Mathf.Min(luminance1, luminance2) + 0.05f);
        return contrastRatio;
    }

    private float CalculateLuminance(Color color)
    {
        return 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b;
    }

    private void WriteReport(List<GameObject> visibleObjects)
    {
        // Initialize the report text with background color information
        Camera mainCamera = Camera.main;
        string backgroundColorInfo = "Background Color: ";
        if (mainCamera != null)
        {
            if (mainCamera.clearFlags == CameraClearFlags.SolidColor)
            {
                Color backgroundColor = mainCamera.backgroundColor;
                backgroundColorInfo += $"Solid Color - #{ColorUtility.ToHtmlStringRGBA(backgroundColor)}\n";
            }
            else if (mainCamera.clearFlags == CameraClearFlags.Skybox)
            {
                Material skyboxMaterial = RenderSettings.skybox;
                if (skyboxMaterial != null && skyboxMaterial.HasProperty("_Color"))
                {
                    Color backgroundColor = skyboxMaterial.GetColor("_Color");
                    backgroundColorInfo += $"Skybox Color - #{ColorUtility.ToHtmlStringRGBA(backgroundColor)}\n";
                }
            }
            else
            {
                backgroundColorInfo += "Unknown\n";
            }
        }
        else
        {
            backgroundColorInfo += "No main camera found!\n";
        }

        reportText = "Visible Objects Report:\n" + backgroundColorInfo;
        foreach (GameObject obj in visibleObjects)
        {

            string materialInfo = "No Material";
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                if (renderer.material.HasProperty("_Color"))
                {
                    Color color = renderer.material.GetColor("_Color");
                    materialInfo = $"Color: #{ColorUtility.ToHtmlStringRGBA(color)}";
                }
            }
            else if (renderer != null && renderer.sharedMaterial != null)
            {
                if (renderer.sharedMaterial.HasProperty("_Color"))
                {
                    Color color = renderer.sharedMaterial.GetColor("_Color");
                    materialInfo = $"Color: #{ColorUtility.ToHtmlStringRGBA(color)}";
                }
            }
            else
            {
                Color objectColor = Color.white; // Default color
                if (obj.TryGetComponent<Renderer>(out renderer))
                {
                    if (renderer.material.HasProperty("_Color"))
                    {
                        objectColor = renderer.material.GetColor("_Color");
                    }
                }
                materialInfo = $"Color: #{ColorUtility.ToHtmlStringRGBA(objectColor)}";
            }

            reportText += $"Object: {obj.name}, Position: {obj.transform.position},  {materialInfo}\n";

            // Extract and write TextMeshPro properties if available
            TMP_Text textMesh = obj.GetComponent<TMP_Text>();
            if (textMesh != null)
            {
                reportText += $"    Text: {textMesh.text}\n";
                reportText += $"    Font Size: {textMesh.fontSize}\n";
            }

            TMP_InputField inputField = obj.GetComponent<TMP_InputField>();
            if (inputField != null)
            {
                reportText += $"    InputField Text: {inputField.text}\n";
                reportText += $"    Font Size: {inputField.textComponent.fontSize}\n";
            }

            TMP_Dropdown dropdown = obj.GetComponent<TMP_Dropdown>();
            if (dropdown != null)
            {
                reportText += $"    Dropdown Value: {dropdown.options[dropdown.value].text}\n";
            }

            // Extract and write UI Text properties if available
            Text uiText = obj.GetComponent<Text>();
            if (uiText != null)
            {
                reportText += $"    UI Text: {uiText.text}\n";
                reportText += $"    Font Size: {uiText.fontSize}\n";
            }

            // Extract and write Button properties if available
            Button button = obj.GetComponent<Button>();
            if (button != null)
            {
                Text buttonText = button.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    reportText += $"    Button Text: {buttonText.text}\n";
                    reportText += $"    Font Size: {buttonText.fontSize}\n";
                }
            }
        }

        reportText += "\nWCAG Analysis:\n";
        foreach (GameObject obj in visibleObjects)
        {
            // Get the text component if available
            TextMeshProUGUI textMesh = obj.GetComponent<TextMeshProUGUI>();
            if (textMesh != null)
            {
                // Check the color contrast with the background color
                Color textColor = textMesh.color;
                float contrastRatio = CalculateContrastRatio(textColor, mainCamera.backgroundColor);
                bool meetsContrastRatio = contrastRatio >= 4.5f; // WCAG AA standard

                // Check the font size of the text
                float fontSize = textMesh.fontSize;
                bool meetsFontSizeRequirements = fontSize >= 14; // Example: Minimum font size for WCAG AA

                // Add WCAG analysis to the report
                string contrastInfo = meetsContrastRatio ? "Pass (WCAG AA)" : "Fail (WCAG AA)";
                string sizeInfo = meetsFontSizeRequirements ? "Pass (WCAG AA)" : "Fail (WCAG AA)";
                reportText += $"Text: {textMesh.text}, Contrast Ratio: {contrastRatio:F2}, WCAG Color Contrast: {contrastInfo}, WCAG Text Size: {sizeInfo}\n";

              
            }
        }


        // Update the editor window GUI with the generated report
        Repaint();
    }

}
