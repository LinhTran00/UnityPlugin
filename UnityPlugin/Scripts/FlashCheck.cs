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
    private GUIStyle subheaderStyle;
    private GUIStyle subheaderStyle1;
    private GUIStyle largeFontStyle;
    private GUIStyle largeFontStyleNoWrap;
    private GUIStyle labelStyle;
    private GUIStyle normalStyle;
    private GUIStyle passStyle;
    private GUIStyle failStyle;
    private GUIStyle warningStyle;

    // for report
    public bool flashPass;
    public float flashValue;
    public bool bluelightPass;
    public float bluelightValue;

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
    private Vector2 scrollPosition = Vector2.zero;
    public void OnGUI()
    {
        mainCamera = Camera.main; // Assigning the main camera to mainCamera

        InitializeStyles();
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
        GUILayout.Space(10);
        // Flashing warning description
        EditorGUILayout.LabelField("Flash Warning", headerStyle, GUILayout.Height(50));
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();    
        GUILayout.Label("Description: ", largeFontStyleNoWrap);
        GUILayout.Label(flashWarningDescription, largeFontStyle);
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        // Flashing effect pass/fail
        GUILayout.Label("Flashing Effect Report", subheaderStyle);
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Flashing effect detected: ", labelStyle, GUILayout.ExpandWidth(false));
        GUILayout.Label($"{flashValue}% ", flashPass ? passStyle : failStyle, GUILayout.ExpandWidth(false));
        GUILayout.Label("of pixels changed significantly.", normalStyle);
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        if (flashPass)
        {
            GUILayout.Label("\tFlashing effect is within acceptable limits.", warningStyle);
        }
        else
        {
            GUILayout.Label("\tWarning: Flashing effect may cause discomfort.", warningStyle);
        }

        GUILayout.Space(50);

        // Blue light warning description
        EditorGUILayout.LabelField("Blue Light Warning", headerStyle1, GUILayout.Height(50));
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Description: ", largeFontStyleNoWrap);
        GUILayout.Label(blueLightWarningDescription, largeFontStyle);
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        // Blue light effect pass/fail
        GUILayout.Label("Blue Light Effect Report", subheaderStyle1);
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Blue light detected: ", labelStyle, GUILayout.ExpandWidth(false));
        GUILayout.Label($"{bluelightValue}% ", bluelightPass ? passStyle : failStyle, GUILayout.ExpandWidth(false));
        GUILayout.Label("of pixels have high blue light intensity.", normalStyle);
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        if (bluelightPass)
        {
            GUILayout.Label("\tBlue light levels are within acceptable limits.", warningStyle);
        }
        else
        {
            GUILayout.Label("\tWarning: Blue light intensity may cause eye strain.", warningStyle);
        }
        GUILayout.Space(30);
        GUILayout.EndScrollView();
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
            headerStyle.hover.textColor = Color.yellow;
            headerStyle.fontSize = 20; // Increased font size for accessibility
        }
        if (headerStyle1 == null)
        {
            headerStyle1 = new GUIStyle(EditorStyles.boldLabel);
            headerStyle1.normal.textColor = new Color(0f, 0.66f, 0.88f);
            headerStyle1.hover.textColor = new Color(0f, 0.66f, 0.88f);
            headerStyle1.fontSize = 20; // Increased font size for accessibility
        }
        if (subheaderStyle == null)
        {
            subheaderStyle = new GUIStyle(EditorStyles.boldLabel);
            subheaderStyle.normal.textColor = Color.yellow;
            subheaderStyle.hover.textColor = Color.yellow;
            subheaderStyle.fontSize = 16; // Increased font size for accessibility
        }
        if (subheaderStyle1 == null)
        {
            subheaderStyle1 = new GUIStyle(EditorStyles.boldLabel);
            subheaderStyle1.normal.textColor = new Color(0f, 0.66f, 0.88f);
            subheaderStyle1.hover.textColor = new Color(0f, 0.66f, 0.88f);
            subheaderStyle1.fontSize = 16; // Increased font size for accessibility
        }

        // Report content style with increased font size for accessibility
        if (largeFontStyle == null)
        {
            largeFontStyle = new GUIStyle(EditorStyles.label); // Base it on the default label style
            largeFontStyle.fontSize = 16; // Set your desired font size
            largeFontStyle.wordWrap = true; // Enable word wrapping if the text is long
        }

        if (largeFontStyleNoWrap == null)
        {
            largeFontStyleNoWrap = new GUIStyle(EditorStyles.boldLabel); // Base it on the default label style
            largeFontStyleNoWrap.fontSize = 16; // Set your desired font size
            largeFontStyleNoWrap.wordWrap = false; // Enable word wrapping if the text is long
        }

        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(EditorStyles.boldLabel);
            labelStyle.fontSize = 16;
            labelStyle.wordWrap = true;
        }

        if (normalStyle == null)
        {
            normalStyle = new GUIStyle(EditorStyles.label);
            normalStyle.fontSize = 16;
            normalStyle.wordWrap = true;
        }

        if (passStyle == null)
        {
            passStyle = new GUIStyle(EditorStyles.boldLabel);
            passStyle.fontSize = 16;
            passStyle.normal.textColor = Color.green;
            passStyle.hover.textColor = Color.green;    
        }

        if (failStyle == null)
        {
            failStyle = new GUIStyle(EditorStyles.boldLabel);
            failStyle.fontSize = 16;
            failStyle.normal.textColor = Color.red;
            failStyle.hover.textColor = Color.red;
        }

        if (warningStyle == null)
        {
            warningStyle = new GUIStyle(EditorStyles.boldLabel);
            warningStyle.fontSize = 16;
            warningStyle.wordWrap = true;
            warningStyle.normal.textColor = Color.cyan;
            warningStyle.hover.textColor = Color.cyan;
        }

    }

    private void CheckFlashingEffect(StringWriter writer)
    {
        CaptureScreen();
        pixelColors = screenTexture.GetPixels(); // Get current frame pixel colors

        if (previousPixelColors == null || previousPixelColors.Length != pixelColors.Length)
        {
            previousPixelColors = new Color[pixelColors.Length];
            pixelColors.CopyTo(previousPixelColors, 0);  // Initialize previous frame
            return;  // Skip flashing check for the first frame
        }

        float flashCount = 0;

        // Compare the pixel colors of the current frame with the previous frame
        for (int i = 0; i < pixelColors.Length; i++)
        {
            float colorDifference = 0.3f * Mathf.Abs(pixelColors[i].r - previousPixelColors[i].r) +
                                    0.59f * Mathf.Abs(pixelColors[i].g - previousPixelColors[i].g) +
                                    0.11f * Mathf.Abs(pixelColors[i].b - previousPixelColors[i].b);

            // Adjust sensitivity threshold to avoid false positives
            if (colorDifference > 0.8f)
            {
                flashCount++;
            }
        }

        float flashPercentage = (flashCount / pixelColors.Length) * 100;
        writer.WriteLine($"Flashing effect detected: {flashPercentage}% of pixels changed significantly.");

        // Increase threshold to reduce false positives
        flashPass = flashPercentage <= 30;  // Increase the pass threshold if necessary
        flashValue = flashPercentage;
        // Save the current frame colors for the next comparison
        pixelColors.CopyTo(previousPixelColors, 0);

        if (flashPercentage > 30)  // Adjust threshold
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

        // report
        bluelightPass = blueLightPercentage <= 30;
        bluelightValue = blueLightPercentage;

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
