using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TextOnBackgroundCheck : EditorWindow
{
    private Camera mainCamera;
    private int textureWidth = 500; // Width of the sample texture
    private int textureHeight = 400; // Height of the sample texture
    private Texture2D screenTexture;

    private GUIStyle headerStyle, labelStyle, labelStyle1, sliderStyle, sliderThumbStyle, textAreaStyle;
    private Vector2 textPosition = new Vector2(10, 370); // Initial text position
    private bool isDraggingText = false; // To track if text is being dragged
    private Color overlayTextColor = Color.white; // Text color for the overlay text
    private int overlayTextSize = 24; // Initial text size
    private GUIStyle wcagStyle;
    private GUIStyle passStyle;
    private GUIStyle failStyle;

    [MenuItem("Window/Text on Background Check")]
    public static void ShowWindow()
    {
        GetWindow<TextOnBackgroundCheck>("Text on Background Check");
    }

    public void OnEnable()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            screenTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);
        }
        else
        {
            Debug.LogError("Main Camera not found. Please assign a Main Camera to the scene.");
        }
    }

    private void InitializeGUIStyles()
    {
        headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
        labelStyle = new GUIStyle(EditorStyles.label) { fontSize = 14, wordWrap = true, alignment = TextAnchor.MiddleCenter };
        labelStyle1 = new GUIStyle(EditorStyles.boldLabel) { fontSize = 18, wordWrap = true, alignment = TextAnchor.UpperLeft };
        sliderStyle = new GUIStyle(GUI.skin.horizontalSlider) { fixedHeight = 20 };
        sliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb) { fixedWidth = 20, fixedHeight = 20 };
        textAreaStyle = new GUIStyle(EditorStyles.label) { wordWrap = true, fontSize = 16, alignment = TextAnchor.UpperLeft };
        wcagStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 };
        passStyle = new GUIStyle(wcagStyle) { normal = { textColor = Color.green } };
        failStyle = new GUIStyle(wcagStyle) { normal = { textColor = Color.red } };
    }

    public void OnGUI()
    {
        if (mainCamera == null)
        {
            GUILayout.Label("No Main Camera found in the scene.");
            return;
        }

        InitializeGUIStyles();
        GUILayout.Label("Text on Background Check", headerStyle, GUILayout.Height(30));

        CaptureScreen();

        // Create a GUI area for displaying the image
        Rect imageRect = GUILayoutUtility.GetRect(textureWidth, textureHeight);
        GUI.DrawTexture(imageRect, screenTexture);

        // Handle dragging of the text
        HandleTextDragging(imageRect);

        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Text Color:");
        overlayTextColor = EditorGUILayout.ColorField(overlayTextColor);
        GUILayout.Label("Text Size:");
        overlayTextSize = EditorGUILayout.IntSlider(overlayTextSize, 10, 72);
        GUILayout.EndHorizontal();

        Color backgroundColor = GetBackgroundColorAtTextPosition(imageRect);
        Rect colorRect = GUILayoutUtility.GetRect(150, 50);
        EditorGUI.DrawRect(colorRect, backgroundColor);

        GUI.Label(colorRect, "Overlay Text", new GUIStyle
        {
            fontSize = overlayTextSize,
            normal = { textColor = overlayTextColor },
            alignment = TextAnchor.MiddleCenter
        });

        float contrastRatio = CalculateContrastRatio(overlayTextColor, backgroundColor);
        DisplayWCAGCompliance(contrastRatio, overlayTextSize);
    }

    private void DisplayWCAGCompliance(float contrastRatio, int fontSize)
    {
        bool wcagAAPass = contrastRatio >= 4.5f;
        bool wcagAALargePass = contrastRatio >= 3.0f && fontSize >= 18;
        bool wcagAAAPass = contrastRatio >= 7.0f;
        bool wcagAAALargePass = contrastRatio >= 4.5f && fontSize >= 18;

        GUILayout.BeginHorizontal();
        GUILayout.Label("WCAG AA:", wcagStyle, GUILayout.Width(200), GUILayout.Height(18));
        GUILayout.Label(wcagAAPass ? "Pass" : "Fail", wcagAAPass ? passStyle : failStyle, GUILayout.Width(100), GUILayout.Height(18));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("WCAG AA (Large Text):", wcagStyle, GUILayout.Width(200), GUILayout.Height(18));
        GUILayout.Label(wcagAALargePass ? "Pass" : "Fail", wcagAALargePass ? passStyle : failStyle, GUILayout.Width(100), GUILayout.Height(18));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("WCAG AAA:", wcagStyle, GUILayout.Width(200), GUILayout.Height(18));
        GUILayout.Label(wcagAAAPass ? "Pass" : "Fail", wcagAAAPass ? passStyle : failStyle, GUILayout.Width(100), GUILayout.Height(18));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("WCAG AAA (Large Text):", wcagStyle, GUILayout.Width(200), GUILayout.Height(18));
        GUILayout.Label(wcagAAALargePass ? "Pass" : "Fail", wcagAAALargePass ? passStyle : failStyle, GUILayout.Width(100), GUILayout.Height(18));
        GUILayout.EndHorizontal();
    }

    private void HandleTextDragging(Rect imageRect)
    {
        Rect textRect = new Rect(imageRect.x + textPosition.x, imageRect.y + textPosition.y, 150, 30);

        GUI.Label(textRect, "Overlay Text", new GUIStyle
        {
            fontSize = overlayTextSize,
            normal = { textColor = overlayTextColor },
            alignment = TextAnchor.MiddleCenter
        });

        if (Event.current.type == EventType.MouseDown && textRect.Contains(Event.current.mousePosition))
        {
            isDraggingText = true;
            Event.current.Use();
        }

        if (isDraggingText && Event.current.type == EventType.MouseDrag)
        {
            textPosition += Event.current.delta;
            Event.current.Use();
        }

        if (Event.current.type == EventType.MouseUp)
        {
            isDraggingText = false;
        }
    }

    private Color GetBackgroundColorAtTextPosition(Rect imageRect)
    {
        int regionWidth = 50;
        int regionHeight = 30;
        int startX = Mathf.FloorToInt(textPosition.x + imageRect.x);
        int startY = Mathf.FloorToInt(textPosition.y + imageRect.y);

        startX = Mathf.Clamp(startX, 0, textureWidth - regionWidth);
        startY = Mathf.Clamp(startY, 0, textureHeight - regionHeight);

        Dictionary<Color, int> colorCount = new Dictionary<Color, int>();

        for (int x = startX; x < startX + regionWidth; x++)
        {
            for (int y = startY; y < startY + regionHeight; y++)
            {
                Color pixelColor = screenTexture.GetPixel(x, textureHeight - y - 1); // Invert Y axis for GUI coordinates

                if (colorCount.ContainsKey(pixelColor))
                {
                    colorCount[pixelColor]++;
                }
                else
                {
                    colorCount[pixelColor] = 1;
                }
            }
        }

        Color mostFrequentColor = Color.black;
        int maxCount = 0;

        foreach (KeyValuePair<Color, int> kvp in colorCount)
        {
            if (kvp.Value > maxCount)
            {
                maxCount = kvp.Value;
                mostFrequentColor = kvp.Key;
            }
        }

        return mostFrequentColor;
    }

    private float CalculateContrastRatio(Color textColor, Color backgroundColor)
    {
        float luminance1 = 0.2126f * Mathf.Pow(textColor.r, 2.2f) + 0.7152f * Mathf.Pow(textColor.g, 2.2f) + 0.0722f * Mathf.Pow(textColor.b, 2.2f);
        float luminance2 = 0.2126f * Mathf.Pow(backgroundColor.r, 2.2f) + 0.7152f * Mathf.Pow(backgroundColor.g, 2.2f) + 0.0722f * Mathf.Pow(backgroundColor.b, 2.2f);

        return (luminance1 > luminance2)
            ? (luminance1 + 0.05f) / (luminance2 + 0.05f)
            : (luminance2 + 0.05f) / (luminance1 + 0.05f);
    }

    private void CaptureScreen()
    {
        RenderTexture renderTexture = new RenderTexture(textureWidth, textureHeight, 24, RenderTextureFormat.Default);
        mainCamera.targetTexture = renderTexture;
        mainCamera.Render();

        RenderTexture.active = renderTexture;
        screenTexture.ReadPixels(new Rect(0, 0, textureWidth, textureHeight), 0, 0);
        screenTexture.Apply();

        RenderTexture.active = null;
        mainCamera.targetTexture = null;
        renderTexture.Release();
    }

}
