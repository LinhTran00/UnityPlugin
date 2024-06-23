using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine.UI;

public class CameraObjectReporterWindow : EditorWindow
{
    private Camera mainCamera;
    private enum DeviceType { Mobile, Tablet, Desktop }
    public int sampleSize = 128; // Size of the sample texture
    private Texture2D screenTexture;
    private Color[] pixelColors;
    private string reportContent = "";

    [MenuItem("Window/Camera Object Reporter")]
    public static void ShowWindow()
    {
        GetWindow<CameraObjectReporterWindow>("Camera Object Reporter");
    }

    private void OnEnable()
    {
        mainCamera = Camera.main;
        screenTexture = new Texture2D(sampleSize, sampleSize, TextureFormat.RGB24, false);
        pixelColors = new Color[sampleSize * sampleSize];
    }

    private void OnGUI()
    {
        GUILayout.Label("Camera Object Reporter", EditorStyles.boldLabel);

        mainCamera = EditorGUILayout.ObjectField("Main Camera", mainCamera, typeof(Camera), true) as Camera;
        sampleSize = EditorGUILayout.IntField("Sample Size", sampleSize);

        if (GUILayout.Button("Generate Report"))
        {
            GenerateReport();
        }

        if (GUILayout.Button("Check Brightness"))
        {
            float averageBrightness = CalculateAverageBrightness();
            reportContent = $"Average Brightness: {averageBrightness}\n";
            using (StringWriter writer = new StringWriter())
            {
                PrintBrightnessSuggestion(writer, averageBrightness);
                reportContent += writer.ToString();
            }
        }

        GUILayout.Label("Report", EditorStyles.boldLabel);
        GUILayout.TextArea(reportContent, GUILayout.ExpandHeight(true));
    }

    private void PrintBrightnessSuggestion(TextWriter writer, float averageBrightness)
    {
        string suggestion;

        if (averageBrightness < 0.2f)
        {
            suggestion = "The game is very dark. Consider increasing the brightness for better visibility.";
        }
        else if (averageBrightness < 0.4f)
        {
            suggestion = "The game is moderately dark. This might be suitable for atmospheric settings, but ensure it is comfortable for players.";
        }
        else if (averageBrightness <= 0.6f)
        {
            suggestion = "The brightness level is well-balanced. It is generally suitable for most games.";
        }
        else if (averageBrightness <= 0.8f)
        {
            suggestion = "The game is quite bright. This is good for vibrant and cheerful settings, but ensure it is not too glaring for extended play.";
        }
        else
        {
            suggestion = "The game is very bright. Consider reducing the brightness to avoid potential eye strain for players.";
        }

        writer.WriteLine($"Average Brightness: {averageBrightness}\nSuggestion: {suggestion}");
    }

    private void GenerateReport()
    {
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera is not assigned.");
            return;
        }

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        List<GameObject> visibleObjects = new List<GameObject>();

        // Collect all renderers and UI elements
        Renderer[] renderers = GameObject.FindObjectsOfType<Renderer>();
        RectTransform[] uiElements = GameObject.FindObjectsOfType<RectTransform>();

        // Check visibility of renderers
        foreach (Renderer renderer in renderers)
        {
            if (GeometryUtility.TestPlanesAABB(planes, renderer.bounds))
            {
                visibleObjects.Add(renderer.gameObject);
            }
        }

        // Check visibility of UI elements
        foreach (RectTransform rectTransform in uiElements)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mainCamera.WorldToScreenPoint(rectTransform.position), mainCamera))
            {
                visibleObjects.Add(rectTransform.gameObject);
            }
        }

        WriteReport(visibleObjects);
    }

    private void WriteReport(List<GameObject> visibleObjects)
    {
        StringWriter writer = new StringWriter();
        DeviceType deviceType = GetDeviceType();

        foreach (GameObject obj in visibleObjects)
        {
            string materialInfo = GetMaterialInfo(obj);
            writer.WriteLine($"Object: {obj.name}, Position: {obj.transform.position}, Material/Color: {materialInfo}");
            WriteTextComponentInfo(obj, writer, deviceType);
        }

        // Calculate and write brightness information
        float brightness = CalculateAverageBrightness();
        PrintBrightnessSuggestion(writer, brightness);

        reportContent = writer.ToString();
        writer.Close();
    }

    private string GetMaterialInfo(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (renderer.material != null)
            {
                string materialName = renderer.material.name.Replace("(Instance)", "").Trim();
                string baseColor = GetMaterialBaseColorHex(renderer.material);
                return $"{materialName}, Base Color: {baseColor}";
            }
            if (renderer.sharedMaterial != null)
            {
                string materialName = renderer.sharedMaterial.name.Replace("(Instance)", "").Trim();
                string baseColor = GetMaterialBaseColorHex(renderer.sharedMaterial);
                return $"{materialName}, Base Color: {baseColor}";
            }
        }

        return "No Material";
    }

    private string GetMaterialBaseColorHex(Material material)
    {
        Color baseColor = Color.white; // Default color if no base color is found
        if (material.HasProperty("_BaseColor"))
        {
            baseColor = material.GetColor("_BaseColor");
        }
        else if (material.HasProperty("_Color"))
        {
            baseColor = material.GetColor("_Color");
        }
        return "#" + ColorUtility.ToHtmlStringRGB(baseColor); // Only the RGB components
    }


    private void WriteTextComponentInfo(GameObject obj, TextWriter writer, DeviceType deviceType)
    {
        TMP_Text textMesh = obj.GetComponent<TMP_Text>();
        if (textMesh != null)
        {
            writer.WriteLine($"    Text: {textMesh.text}");
            writer.WriteLine($"    Font Size: {textMesh.fontSize}");
            ValidateFontSize(writer, textMesh.fontSize, deviceType);
        }

        TMP_InputField inputField = obj.GetComponent<TMP_InputField>();
        if (inputField != null)
        {
            writer.WriteLine($"    InputField Text: {inputField.text}");
            writer.WriteLine($"    Font Size: {inputField.textComponent.fontSize}");
            ValidateFontSize(writer, inputField.textComponent.fontSize, deviceType);
        }

        TMP_Dropdown dropdown = obj.GetComponent<TMP_Dropdown>();
        if (dropdown != null)
        {
            writer.WriteLine($"    Dropdown Value: {dropdown.options[dropdown.value].text}");
        }

        Text uiText = obj.GetComponent<Text>();
        if (uiText != null)
        {
            writer.WriteLine($"    UI Text: {uiText.text}");
            writer.WriteLine($"    Font Size: {uiText.fontSize}");
            ValidateFontSize(writer, uiText.fontSize, deviceType);
        }

        Button button = obj.GetComponent<Button>();
        if (button != null)
        {
            Text buttonText = button.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                writer.WriteLine($"    Button Text: {buttonText.text}");
                writer.WriteLine($"    Font Size: {buttonText.fontSize}");
                ValidateFontSize(writer, buttonText.fontSize, deviceType);
            }
        }
    }

    private void ValidateFontSize(TextWriter writer, float fontSize, DeviceType deviceType)
    {
        bool isPass = false;
        string suggestion = string.Empty;

        switch (deviceType)
        {
            case DeviceType.Mobile:
                isPass = fontSize >= 12;
                if (!isPass)
                {
                    suggestion = $"Fail: Increase font size to at least 12.";
                }
                else
                {
                    suggestion = "Pass";
                }
                break;
            case DeviceType.Tablet:
                isPass = fontSize >= 15;
                if (!isPass)
                {
                    suggestion = $"Fail: Increase font size to at least 15.";
                }
                else
                {
                    suggestion = "Pass";
                }
                break;
            case DeviceType.Desktop:
                isPass = fontSize >= 16;
                if (!isPass)
                {
                    suggestion = $"Fail: Increase font size to at least 16.";
                }
                else
                {
                    suggestion = "Pass";
                }
                break;
        }

        writer.WriteLine($"    Font Size Check: {suggestion}");
    }

    private DeviceType GetDeviceType()
    {
        // Simplistic device type detection based on screen resolution
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        if (screenWidth <= 800 && screenHeight <= 1280)
            return DeviceType.Mobile;
        else if (screenWidth <= 1280 && screenHeight <= 1920)
            return DeviceType.Tablet;
        else
            return DeviceType.Desktop;
    }

    private float CalculateAverageBrightness()
    {
        CaptureScreen();
        float totalBrightness = 0;

        pixelColors = screenTexture.GetPixels(); // Read pixel colors from the texture

        foreach (Color color in pixelColors)
        {
            totalBrightness += (color.r + color.g + color.b) / 3f; // Luminance approximation
        }

        return totalBrightness / pixelColors.Length;
    }

    private void CaptureScreen()
    {
        RenderTexture renderTexture = new RenderTexture(sampleSize, sampleSize, 24);
        mainCamera.targetTexture = renderTexture; // Set the render texture to the camera
        mainCamera.Render(); // Render the camera's view to the texture

        RenderTexture.active = renderTexture;
        screenTexture.ReadPixels(new Rect(0, 0, sampleSize, sampleSize), 0, 0);
        screenTexture.Apply();
        RenderTexture.active = null;
        mainCamera.targetTexture = null; // Reset the camera's render texture
        renderTexture.Release();
    }
}
