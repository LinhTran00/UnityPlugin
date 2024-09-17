using UnityEngine;
using UnityEditor;

public class BrightnessChecker
{
    private Camera mainCamera;
    private int textureWidth = 256; // Width of the sample texture
    private int textureHeight = 144; // Height of the sample texture
    private Texture2D screenTexture;
    private Texture2D adjustedTexture;
    private Color[] pixelColors;
    private Color[] adjustedColors;

    private GUIStyle headerStyle;
    private GUIStyle labelStyle;
    private GUIStyle labelStyle1;
    private GUIStyle sliderStyle;
    private GUIStyle sliderThumbStyle;
    private GUIStyle textAreaStyle;

    private float brightnessFactor = 1.0f; // Factor to adjust the brightness

    public void OnEnable()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            screenTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);
            adjustedTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);
            pixelColors = new Color[textureWidth * textureHeight];
            adjustedColors = new Color[textureWidth * textureHeight];
        }
    }

    private void InitializeGUIStyles()
    {
        headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleCenter
        };

        labelStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 14,
            wordWrap = true,
            alignment = TextAnchor.MiddleCenter
        };

        labelStyle1 = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 18,
            wordWrap = true,
            alignment = TextAnchor.UpperLeft
        };

        sliderStyle = new GUIStyle(GUI.skin.horizontalSlider)
        {
            fixedHeight = 20
        };

        sliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb)
        {
            fixedWidth = 20,
            fixedHeight = 20
        };

        textAreaStyle = new GUIStyle(EditorStyles.label)
        {
            wordWrap = true,
            fontSize = 16,
            alignment = TextAnchor.UpperLeft
        };
    }

    public void OnGUI()
    {
        if (mainCamera == null)
        {
            GUILayout.Label("No Main Camera found in the scene.");
            return;
        }
        InitializeGUIStyles();

        GUILayout.Label("Brightness Checker", headerStyle, GUILayout.Height(30));

        // Calculate and display brightness information
        CaptureScreen();
        float brightness = CalculateAverageBrightness(screenTexture);

        EditorGUILayout.BeginVertical();
        GUILayout.Label("Original Image", labelStyle1, GUILayout.Height(20));
        GUILayout.Box(screenTexture, GUILayout.Width(textureWidth), GUILayout.Height(textureHeight));
        GUILayout.Label($"Brightness: {brightness.ToString("F2")}", textAreaStyle);
        EditorGUILayout.EndVertical();

        PrintBrightnessSuggestion(brightness);
        GUILayout.Space(20);

        // Brightness adjustment slider
        GUILayout.Label("Adjust Brightness", labelStyle, GUILayout.Height(20));
        brightnessFactor = GUILayout.HorizontalSlider(brightnessFactor, 0.1f, 2.0f, sliderStyle, sliderThumbStyle);
        GUILayout.Space(20);

        // Adjust the screen image brightness
        AdjustBrightness();

        // Calculate adjusted brightness
        float adjustedBrightness = CalculateAverageBrightness(adjustedTexture);
        EditorGUILayout.BeginVertical();

        GUILayout.Label("Adjusted Image", labelStyle1, GUILayout.Height(20));
        GUILayout.Box(adjustedTexture, GUILayout.Width(textureWidth), GUILayout.Height(textureHeight));
        GUILayout.Label($"Brightness: {adjustedBrightness.ToString("F2")}", textAreaStyle);

        EditorGUILayout.EndVertical();

        PrintBrightnessSuggestion(adjustedBrightness);
    }

    private float CalculateAverageBrightness(Texture2D texture)
    {
        float totalBrightness = 0;

        Color[] colors = texture.GetPixels(); // Read pixel colors from the texture

        foreach (Color color in colors)
        {
            totalBrightness += (color.r + color.g + color.b) / 3f; // Luminance approximation
        }

        return totalBrightness / colors.Length;
    }

    private void PrintBrightnessSuggestion(float averageBrightness)
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

        GUILayout.Label($"Suggestion: {suggestion}", EditorStyles.wordWrappedLabel);
    }

    private void CaptureScreen()
    {
        RenderTexture renderTexture = new RenderTexture(textureWidth, textureHeight, 24);
        mainCamera.targetTexture = renderTexture; // Set the render texture to the camera
        mainCamera.Render(); // Render the camera's view to the texture

        RenderTexture.active = renderTexture;
        screenTexture.ReadPixels(new Rect(0, 0, textureWidth, textureHeight), 0, 0);
        screenTexture.Apply();
        RenderTexture.active = null;
        mainCamera.targetTexture = null; // Reset the camera's render texture
        renderTexture.Release();
    }

    private void AdjustBrightness()
    {
        adjustedColors = screenTexture.GetPixels();

        for (int i = 0; i < adjustedColors.Length; i++)
        {
            Color adjustedColor = adjustedColors[i] * brightnessFactor;

            // Normalize the adjusted color values to be within the 0-1 range
            adjustedColor.r = Mathf.Clamp01(adjustedColor.r);
            adjustedColor.g = Mathf.Clamp01(adjustedColor.g);
            adjustedColor.b = Mathf.Clamp01(adjustedColor.b);

            adjustedColors[i] = adjustedColor;
        }

        adjustedTexture.SetPixels(adjustedColors);
        adjustedTexture.Apply();
    }
}
