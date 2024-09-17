using UnityEngine;
using System.IO;
using UnityEditor;

public class FlashingCheck
{
    private Camera mainCamera;
    private int sampleSize = 128; // Size of the sample texture
    private Texture2D screenTexture;
    private Color[] pixelColors;
    private Color[] previousPixelColors; // Array to store previous frame colors

    // Flashing warning description
    private static string flashWarningDescription = "This portion of the video contains flashing lights, which can trigger seizures in individuals with photosensitive epilepsy. Be sure to include warnings about flashing lights or provide an option to disable them.";

    // Blue light warning description
    private static string blueLightWarningDescription = "This portion of the video contains excessive blue light, which can cause eye strain and discomfort. Consider reducing the blue light intensity or providing an option for blue light filtering.";

    private string flashingReportContent = "";
    private string blueLightReportContent = "";
    private GUIStyle headerStyle;
    private GUIStyle headerStyle1;
    private GUIStyle reportStyle;
    private GUIStyle largeFontStyle;

    public void OnEnable()
    {
        mainCamera = Camera.main;

        screenTexture = new Texture2D(sampleSize, sampleSize, TextureFormat.RGB24, false);
        pixelColors = new Color[sampleSize * sampleSize];
        previousPixelColors = new Color[sampleSize * sampleSize]; // Initialize the previous frame array

        // Automatically generate reports when the window is enabled
        GenerateFlashingReport();
        GenerateBlueLightReport();
    }

    public void OnGUI()
    {
        mainCamera = Camera.main; // Assigning the main camera to mainCamera

        InitializeStyles();

        // Flashing report display
        EditorGUILayout.LabelField("Flash Warning", headerStyle);
        GUILayout.Label(flashWarningDescription, largeFontStyle);

        GUILayout.Space(10);

        GUILayout.Label("Flashing Effect Report:", headerStyle);
        flashingReportContent = EditorGUILayout.TextArea(flashingReportContent, reportStyle, GUILayout.Height(200));

        GUILayout.Space(10);

        // Blue light report display
        EditorGUILayout.LabelField("Blue Light Warning", headerStyle1);
        GUILayout.Label(blueLightWarningDescription, largeFontStyle);

        GUILayout.Space(10);

        GUILayout.Label("Blue Light Effect Report:", headerStyle1);
        blueLightReportContent = EditorGUILayout.TextArea(blueLightReportContent, reportStyle, GUILayout.Height(200));
    }

    private void GenerateFlashingReport()
    {
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera is not assigned.");
            return;
        }

        StringWriter writer = new StringWriter();

        // Check for flashing effect and write the result
        CheckFlashingEffect(writer);

        flashingReportContent = writer.ToString(); // Update flashing report content
        writer.Close(); // Close the StringWriter
    }

    private void GenerateBlueLightReport()
    {
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera is not assigned.");
            return;
        }

        StringWriter writer = new StringWriter();

        // Check for blue light effect and write the result
        CheckBlueLightEffect(writer);

        blueLightReportContent = writer.ToString(); // Update blue light report content
        writer.Close(); // Close the StringWriter
    }

    private void InitializeStyles()
    {
        // Header style with increased font size for accessibility
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.normal.textColor = Color.yellow;
            headerStyle.fontSize = 16; // Increased font size for accessibility
        }
        if (headerStyle1 == null)
        {
            headerStyle1 = new GUIStyle(EditorStyles.boldLabel);
            headerStyle1.normal.textColor = Color.blue;
            headerStyle1.fontSize = 16; // Increased font size for accessibility
        }

        // Report content style with increased font size for accessibility
        if (reportStyle == null)
        {
            reportStyle = new GUIStyle(EditorStyles.textArea);
            reportStyle.fontSize = 14; // Increased font size for better readability
            reportStyle.wordWrap = true;
        }

        if (largeFontStyle == null)
        {
            largeFontStyle = new GUIStyle(EditorStyles.label); // Base it on the default label style
            largeFontStyle.fontSize = 16; // Set your desired font size
            largeFontStyle.wordWrap = true; // Enable word wrapping if the text is long
            largeFontStyle.normal.textColor = Color.white; // Set the text color if desired
        }
    }

    private void CheckFlashingEffect(StringWriter writer)
    {
        CaptureScreen();
        pixelColors = screenTexture.GetPixels(); // Get current frame pixel colors

        float flashCount = 0;

        // Compare the pixel colors of the current frame with the previous frame
        for (int i = 0; i < pixelColors.Length; i++)
        {
            float colorDifference = Mathf.Abs(pixelColors[i].r - previousPixelColors[i].r) +
                                    Mathf.Abs(pixelColors[i].g - previousPixelColors[i].g) +
                                    Mathf.Abs(pixelColors[i].b - previousPixelColors[i].b);

            // If the color difference is significant, consider it a flash
            if (colorDifference > 0.5f)
            {
                flashCount++;
            }
        }

        float flashPercentage = (flashCount / pixelColors.Length) * 100;
        writer.WriteLine($"Flashing effect detected: {flashPercentage}% of pixels changed significantly.");

        // Save the current frame colors for the next comparison
        pixelColors.CopyTo(previousPixelColors, 0);

        if (flashPercentage > 25) // Threshold for significant flashing effect
        {
            writer.WriteLine("Suggestion: Reduce flashing effects to prevent discomfort or potential health risks for players.");
            writer.WriteLine();

        }
    }

    private void CheckBlueLightEffect(StringWriter writer)
    {
        CaptureScreen();
        pixelColors = screenTexture.GetPixels(); // Get current frame pixel colors

        float blueLightCount = 0;

        // Check for blue light intensity in the current frame
        for (int i = 0; i < pixelColors.Length; i++)
        {
            if (pixelColors[i].b > 0.6f) // Threshold for detecting high blue light intensity
            {
                blueLightCount++;
            }
        }

        float blueLightPercentage = (blueLightCount / pixelColors.Length) * 100;
        writer.WriteLine($"Blue light detected: {blueLightPercentage}% of pixels have high blue light intensity.");

        if (blueLightPercentage > 30) // Threshold for significant blue light effect
        {
            writer.WriteLine("Suggestion: Reduce blue light intensity to prevent eye strain or provide a blue light filter option.");
            writer.WriteLine();
            writer.WriteLine("Blue Light Warning:");
            writer.WriteLine(blueLightWarningDescription);
        }
        else
        {
            writer.WriteLine("Blue light levels are within acceptable limits. No action needed.");
        }
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
