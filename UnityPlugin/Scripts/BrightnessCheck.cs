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
    private GUIStyle subHeaderStyle;
    private GUIStyle labelStyle;
    private GUIStyle passStyle;
    private GUIStyle failStyle;
    private GUIStyle suggestionStyle;

    private GUIStyle sliderStyle;
    private GUIStyle sliderThumbStyle;
    // for report only
    public float brightnessReport;
    public string shortSuggestion;
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

        // Automatically generate the report when the window is enabled
        CaptureScreen();
        brightnessReport = CalculateAverageBrightness(screenTexture);
        shortSuggestion = ShortSuggestion(brightnessReport);
    }

    private void InitializeGUIStyles()
    {
        headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 18,
        };

        subHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            wordWrap = true,
        };

        labelStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            wordWrap= true,
        };
        passStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            normal = { textColor = Color.green },
            wordWrap = true,
        };
        failStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            normal = { textColor = Color.red },
            wordWrap = true,
        };
        suggestionStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            normal = { textColor = Color.cyan }, 
            wordWrap = true,
        };

        sliderStyle = new GUIStyle(GUI.skin.horizontalSlider)
        {
            fixedHeight = 18,
            fixedWidth = 500
        };

        sliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb)
        {
            fixedWidth = 15,
            fixedHeight = 18
        };

    }

    private Vector2 scrollPosition = Vector2.zero;

    public void OnGUI()
    {
        if (mainCamera == null)
        {
            GUILayout.Label("No Main Camera found in the scene.");
            return;
        }

        InitializeGUIStyles();
        GUIStyle style;

        // Start the scroll view
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

        GUILayout.Label("Brightness Checker", headerStyle);
        GUILayout.Space(20);

        // Display brightness report and suggestion
        if (brightnessReport > 0.1 && brightnessReport < 0.9)
        {
            style = passStyle;
        }
        else
        {
            style = failStyle;
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label("Brightness: ", labelStyle, GUILayout.ExpandWidth(false));
        GUILayout.Label($"{brightnessReport.ToString("F2")}", style);
        GUILayout.EndHorizontal();
        PrintBrightnessSuggestion(brightnessReport);
        GUILayout.Space(10);

        // Display original image
        GUILayout.Label("Original Image", subHeaderStyle);
        GUILayout.Space(5);
        GUILayout.Box(screenTexture, GUILayout.Width(textureWidth), GUILayout.Height(textureHeight));
        GUILayout.Space(20);

        // Brightness adjustment slider
        GUILayout.Label("Adjust Brightness Simulation", headerStyle);
        GUILayout.Space(20);
        GUILayout.Label("Use slider below to see brightness change in the adjusted image", labelStyle);
        brightnessFactor = GUILayout.HorizontalSlider(brightnessFactor, 0.0f, 30.0f, sliderStyle, sliderThumbStyle);
        GUILayout.Space(20);

        // Adjust the screen image brightness
        AdjustBrightness();

        // Display adjusted image and suggestion
        GUILayout.Label("Adjusted Image", subHeaderStyle);
        GUILayout.Box(adjustedTexture, GUILayout.Width(textureWidth), GUILayout.Height(textureHeight));
        float adjustedBrightness = CalculateAverageBrightness(adjustedTexture);
        if (adjustedBrightness > 0.1 && adjustedBrightness < 0.9)
        {
            style = passStyle;
        }
        else
        {
            style = failStyle;
        }
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Adjusted Brightness:", labelStyle, GUILayout.ExpandWidth(false));
        GUILayout.Label($"{adjustedBrightness.ToString("F2")}", style);
        GUILayout.EndHorizontal();
        PrintBrightnessSuggestion(adjustedBrightness);
        GUILayout.Space(20);


        // End the scroll view
        GUILayout.EndScrollView();
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

        GUILayout.Label($"Suggestion: {suggestion}", suggestionStyle);
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

    public string ShortSuggestion(float brightnessValue)
    {
        if (brightnessValue < 0.2f)
        {
            return "Very dark";
        }
        else if (brightnessValue < 0.4f)
        {
            return "Moderately dark";
        }
        else if (brightnessValue <= 0.6f)
        {
            return "Well-balanced";
        }
        else if (brightnessValue <= 0.8f)
        {
            return "Quite bright";
        }
        else
        {
            return "Very bright";
        }
    }

    }
