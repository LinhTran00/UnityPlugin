using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class ColorblindSimulator
{
    private Camera mainCamera;
    private RenderTexture renderTexture;
    private Texture2D simulatedTexture;

    // all color blind texture
    private Texture2D protanopiaTexture;
    private Texture2D deuteranopiaTexture;
    private Texture2D deuteranomalyTexture;
    private Texture2D protanomalyTexture;
    private Texture2D tritanopiaTexture;
    private Texture2D tritanomalyTexture;
    private Texture2D achromatopsiaTexture;
    private Texture2D achromatomalyTexture;
    private Texture2D normalVisionTexture;



    private Vector2 scrollPosition;
    private GUIStyle header;
    private GUIStyle labelStyle;
    private const int imageWidth = 256;
    private const int imageHeight = 144;

    private ColorBlindnessType selectedType;
    private bool showColorField;
    private Color[] colorPalette;
    private bool closeButton = false;


    public void OnEnable()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            StartSimulations();
        }
    }

    public void OnGUI()
    {
        header = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 18
        };


        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        GUILayout.Label("Normal vision", header, GUILayout.Height(24));
        DrawSimulatedImage(normalVisionTexture, "Normal Vision", ColorBlindnessType.NormalVision);


        GUILayout.Label("Red-green color vision deficiency", header, GUILayout.Height(24));

        GUILayout.BeginHorizontal();
        DrawSimulatedImage(protanopiaTexture, "Protanopia (red-blind)", ColorBlindnessType.Protanopia);
        DrawSimulatedImage(protanomalyTexture, "Protanomaly (red-weak)", ColorBlindnessType.Protanomaly);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        DrawSimulatedImage(deuteranopiaTexture, "Deuteranopia (green-blind)", ColorBlindnessType.Deuteranopia);
        DrawSimulatedImage(deuteranomalyTexture, "Deuteranomaly (green-weak)", ColorBlindnessType.Deuteranomaly);
        GUILayout.EndHorizontal();



        GUILayout.Label("Blue-yellow color vision deficiency", header, GUILayout.Height(24));
        GUILayout.BeginHorizontal();
        DrawSimulatedImage(tritanopiaTexture, "Tritanopia (blue-blind)", ColorBlindnessType.Tritanopia);
        DrawSimulatedImage(tritanomalyTexture, "Tritanomaly (blue-weak)", ColorBlindnessType.Tritanomaly);
        GUILayout.EndHorizontal();

        GUILayout.Label("Complete color vision deficiency", header, GUILayout.Height(24));
        GUILayout.BeginHorizontal();
        DrawSimulatedImage(achromatopsiaTexture, "Achromatopsia (monochromacy)", ColorBlindnessType.Achromatopsia);
        DrawSimulatedImage(achromatomalyTexture, "Achromatomaly (blue-cone monochromacy)", ColorBlindnessType.Achromatomaly);
        GUILayout.EndHorizontal();

        GUILayout.EndScrollView();

        if (showColorField)
        {
            GUILayout.BeginHorizontal();
            if (closeButton) // "X" close button
            {
                showColorField = false; // Hide the color field when clicked
                closeButton = false;
            }
            GUILayout.EndHorizontal();

            DrawColorField();
        }
    }

    private void DrawSimulatedImage(Texture2D texture, string label, ColorBlindnessType type)
    {
        if (texture != null)
        {
            GUILayout.BeginVertical();
            // Define a GUI style for the label
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.fontStyle = FontStyle.Bold; // Make it bold
                labelStyle.fontSize = 14; // Increase font size
            }

            GUILayout.Label(label, labelStyle); // Use the custom style for the label
            if (GUILayout.Button(texture, GUILayout.Width(imageWidth), GUILayout.Height(imageHeight)))
            {
                selectedType = type;
                showColorField = true;
                GenerateColorPalette();
            }
            GUILayout.EndVertical();
        }
    }

    private void DrawColorField()
    { 
        GUIStyle bold = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 15
        };

        GUILayout.BeginHorizontal();
        GUILayout.Label("Color Range for " + selectedType, bold);
        GUILayout.FlexibleSpace();
        closeButton = GUILayout.Button("X", bold);
        GUILayout.EndHorizontal();

        foreach (Color color in colorPalette)
        {
            EditorGUILayout.ColorField(GUIContent.none, color, false, false, false);
        }
    }


    private void GenerateColorPalette()
    {
        colorPalette = new Color[8];

        float step = (700f - 400f) / (colorPalette.Length - 1);
        for (int i = 0; i < colorPalette.Length; i++)
        {
            float wavelength = 400f + i * step;
            colorPalette[i] = WavelengthToRGB(wavelength, selectedType);
        }
    }

    private Color WavelengthToRGB(float wavelength, ColorBlindnessType type)
    {
        float R, G, B;

        if (wavelength >= 380 && wavelength < 440)
        {
            R = -(wavelength - 440) / (440 - 380);
            G = 0.0f;
            B = 1.0f;
        }
        else if (wavelength >= 440 && wavelength < 490)
        {
            R = 0.0f;
            G = (wavelength - 440) / (490 - 440);
            B = 1.0f;
        }
        else if (wavelength >= 490 && wavelength < 510)
        {
            R = 0.0f;
            G = 1.0f;
            B = -(wavelength - 510) / (510 - 490);
        }
        else if (wavelength >= 510 && wavelength < 580)
        {
            R = (wavelength - 510) / (580 - 510);
            G = 1.0f;
            B = 0.0f;
        }
        else if (wavelength >= 580 && wavelength < 645)
        {
            R = 1.0f;
            G = -(wavelength - 645) / (645 - 580);
            B = 0.0f;
        }
        else if (wavelength >= 645 && wavelength <= 780)
        {
            R = 1.0f;
            G = 0.0f;
            B = 0.0f;
        }
        else
        {
            R = 0.0f;
            G = 0.0f;
            B = 0.0f;
        }

        // Adjust intensities for different wavelengths
        if (wavelength >= 380 && wavelength < 420)
        {
            float factor = 0.3f + 0.7f * (wavelength - 380) / (420 - 380);
            R *= factor;
            G *= factor;
            B *= factor;
        }
        else if (wavelength >= 645 && wavelength <= 780)
        {
            float factor = 0.3f + 0.7f * (780 - wavelength) / (780 - 645);
            R *= factor;
            G *= factor;
            B *= factor;
        }

        // Apply color blindness simulation
        return SimulateColorBlindness(new Color(R, G, B), type);
    }

    private void StartSimulations()
    {
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera is not assigned.");
            return;
        }

        renderTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        mainCamera.targetTexture = renderTexture;

        SimulateNormalVision();
        SimulateProtanopia();
        SimulateProtanomaly();
        SimulateDeuteranopia();
        SimulateDeuteranomaly();
        SimulateTritanopia();
        SimulateTritanomaly();
        SimulateAchromatopsia();
        SimulateAchromatomaly();

        mainCamera.targetTexture = null;
    }
    private void SimulateNormalVision()
    {
        mainCamera.Render();
        RenderTexture.active = renderTexture;

        normalVisionTexture = new Texture2D(renderTexture.width, renderTexture.height);
        normalVisionTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        normalVisionTexture.Apply();

        RenderTexture.active = null;
    }
    private void SimulateProtanopia()
    {
        CaptureSimulatedTexture(ColorBlindnessType.Protanopia, ref protanopiaTexture);
    }

    private void SimulateProtanomaly()
    {
        CaptureSimulatedTexture(ColorBlindnessType.Protanomaly, ref protanomalyTexture);
    }

    private void SimulateDeuteranopia()
    {
        CaptureSimulatedTexture(ColorBlindnessType.Deuteranopia, ref deuteranopiaTexture);
    }

    private void SimulateDeuteranomaly()
    {
        CaptureSimulatedTexture(ColorBlindnessType.Deuteranomaly, ref deuteranomalyTexture);
    }

    private void SimulateTritanopia()
    {
        CaptureSimulatedTexture(ColorBlindnessType.Tritanopia, ref tritanopiaTexture);
    }

    private void SimulateTritanomaly()
    {
        CaptureSimulatedTexture(ColorBlindnessType.Tritanomaly, ref tritanomalyTexture);
    }

    private void SimulateAchromatopsia()
    {
        CaptureSimulatedTexture(ColorBlindnessType.Achromatopsia, ref achromatopsiaTexture);
    }

    private void SimulateAchromatomaly()
    {
        CaptureSimulatedTexture(ColorBlindnessType.Achromatomaly, ref achromatomalyTexture);
    }

    private void CaptureSimulatedTexture(ColorBlindnessType type, ref Texture2D targetTexture)
    {
        mainCamera.Render();
        RenderTexture.active = renderTexture;

        simulatedTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        simulatedTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);

        for (int x = 0; x < simulatedTexture.width; x++)
        {
            for (int y = 0; y < simulatedTexture.height; y++)
            {
                Color originalColor = simulatedTexture.GetPixel(x, y);
                Color simulatedColor = SimulateColorBlindness(originalColor, type);
                simulatedTexture.SetPixel(x, y, simulatedColor);
            }
        }

        simulatedTexture.Apply();
        targetTexture = simulatedTexture;

        RenderTexture.active = null;
    }

    private void ApplyColorBlindnessSimulation(Texture2D texture, ColorBlindnessType type)
    {
        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = SimulateColorBlindness(pixels[i], type);
        }
        texture.SetPixels(pixels);
        texture.Apply();
    }

    private Color SimulateColorBlindness(Color color, ColorBlindnessType type)
    {
        switch (type)
        {
            case ColorBlindnessType.Protanopia:
                return SimulateProtanopia(color);
            case ColorBlindnessType.Protanomaly:
                return SimulateProtanomaly(color);
            case ColorBlindnessType.Deuteranopia:
                return SimulateDeuteranopia(color);
            case ColorBlindnessType.Deuteranomaly:
                return SimulateDeuteranomaly(color);
            case ColorBlindnessType.Tritanopia:
                return SimulateTritanopia(color);
            case ColorBlindnessType.Tritanomaly:
                return SimulateTritanomaly(color);
            case ColorBlindnessType.Achromatopsia:
                return SimulateAchromatopsia(color);
            case ColorBlindnessType.Achromatomaly:
                return SimulateAchromatomaly(color);
            default:
                return color;
        }
    }
    private Color SimulateNormalVision(Color color)
    {
        return color;
    }

    // red blind
    private Color SimulateProtanopia(Color color)
    {
        float r = color.r * 0.567f + color.g * 0.433f + color.b * 0.0f;
        float g = color.r * 0.558f + color.g * 0.442f + color.b * 0.0f;
        float b = color.r * 0.0f + color.g * 0.242f + color.b * 0.758f;
        return new Color(r, g, b, color.a);
    }

    // red-weak
    private Color SimulateProtanomaly(Color color)
    {
        float r = color.r * 0.817f + color.g * 0.183f + color.b * 0.0f;
        float g = color.r * 0.333f + color.g * 0.667f + color.b * 0.0f;
        float b = color.r * 0.0f + color.g * 0.125f + color.b * 0.875f;
        return new Color(r, g, b, color.a);
    }


    // green-blind
    private Color SimulateDeuteranopia(Color color)
    {
        float r = color.r * 0.625f + color.g * 0.375f + color.b * 0.0f;
        float g = color.r * 0.7f + color.g * 0.3f + color.b * 0.0f;
        float b = color.r * 0.0f + color.g * 0.3f + color.b * 0.7f;
        return new Color(r, g, b, color.a);
    }

    //green-weak
    private Color SimulateDeuteranomaly(Color color)
    {
        float r = color.r * 0.8f + color.g * 0.2f + color.b * 0.0f;
        float g = color.r * 0.258f + color.g * 0.742f + color.b * 0.0f;
        float b = color.r * 0.0f + color.g * 0.142f + color.b * 0.858f;
        return new Color(r, g, b, color.a);
    }

    // blue-blind
    private Color SimulateTritanopia(Color color)
    {
        float r = color.r * 0.95f + color.g * 0.05f + color.b * 0.0f;
        float g = color.r * 0.0f + color.g * 0.433f + color.b * 0.567f;
        float b = color.r * 0.0f + color.g * 0.475f + color.b * 0.525f;
        return new Color(r, g, b, color.a);
    }

    //blue-weak
    private Color SimulateTritanomaly(Color color)
    {
        float r = color.r * 0.967f + color.g * 0.033f + color.b * 0.0f;
        float g = color.r * 0.0f + color.g * 0.733f + color.b * 0.267f;
        float b = color.r * 0.0f + color.g * 0.183f + color.b * 0.817f;
        return new Color(r, g, b, color.a);
    }

    // monochromacy
    private Color SimulateAchromatopsia(Color color)
    {
        float luminance = color.r * 0.299f + color.g * 0.587f + color.b * 0.114f;
        return new Color(luminance, luminance, luminance, color.a);
    }

    // blue-cone monochromacy
    private Color SimulateAchromatomaly(Color color)
    {
        float r = color.r * 0.618f + color.g * 0.320f + color.b * 0.062f;
        float g = color.r * 0.163f + color.g * 0.775f + color.b * 0.062f;
        float b = color.r * 0.163f + color.g * 0.320f + color.b * 0.516f;
        return new Color(r, g, b, color.a);
    }

    private enum ColorBlindnessType
    {
        NormalVision,
        Protanopia,
        Deuteranopia,
        Deuteranomaly,
        Protanomaly,
        Tritanopia,
        Tritanomaly,
        Achromatopsia,
        Achromatomaly

    }
}


