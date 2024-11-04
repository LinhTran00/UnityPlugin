using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ColorChecker
{
    // Constants
    public const float WCAG_AAA_THRESHOLD = 7f;
    public const float WCAG_AA_THRESHOLD = 4.5f;
    public const int DEFAULT_SPACE_SIZE = 12;
    private Vector2 scrollPosition;
    // Colors to manipulate
    private Color c1 = Color.white;
    private Color c2 = Color.black;
    private float lightness1 = 1f;
    private float lightness2 = 0f;

    // Helper styles to make it look pretty :)
    private GUIStyle previewStyle;
    private GUIStyle wcagStyle;
    private GUIStyle passStyle;
    private GUIStyle failStyle;
    private GUIStyle contrastStyle;
    private GUIStyle headerStyle;
    Camera mainCamera;

    public void OnEnable()
    {
        mainCamera = Camera.main;
    }
    private Color AdjustLightness(Color color, float lightness)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        return Color.HSVToRGB(h, s, lightness);
    }
    public void OnGUI()
    {
        // Add some padding to the whole window (across the top)
        GUILayout.Space(DEFAULT_SPACE_SIZE);
        EditorGUILayout.BeginHorizontal();
        // Add some padding to the whole window (down the left)
        GUILayout.Space(DEFAULT_SPACE_SIZE);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Define the right column
        EditorGUILayout.BeginVertical(GUILayout.Width(250));  // Increased the width to 250
        // Set up GUIStyle appropriately
        TryInitStyles();
        EditorGUILayout.LabelField("Color Contrast Check", headerStyle, GUILayout.Height(30));

        // Draw headers for the color pickers
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Foreground", headerStyle, GUILayout.Width(120), GUILayout.Height(20));
        GUILayout.Space(DEFAULT_SPACE_SIZE);
        EditorGUILayout.LabelField("Background", headerStyle, GUILayout.Width(120), GUILayout.Height(20));
        EditorGUILayout.EndHorizontal();


        // Draw the Color Pickers
        EditorGUILayout.BeginHorizontal();

        c1 = EditorGUILayout.ColorField(new GUIContent(), c1, false, false, false, GUILayout.Width(120), GUILayout.Height(100));
        GUILayout.Space(DEFAULT_SPACE_SIZE);
        c2 = EditorGUILayout.ColorField(new GUIContent(), c2, false, false, false, GUILayout.Width(120), GUILayout.Height(100));
        EditorGUILayout.EndHorizontal();

        string hexColor1 = ColorUtility.ToHtmlStringRGB(c1);
        string hexColor2 = ColorUtility.ToHtmlStringRGB(c2);

        // Display the hex values below the color pickers
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TextField($"#{hexColor1}", wcagStyle, GUILayout.Width(100), GUILayout.Height(20));
        GUILayout.Space(32);
        EditorGUILayout.TextField($"#{hexColor2}", wcagStyle, GUILayout.Width(100), GUILayout.Height(20));
        EditorGUILayout.EndHorizontal();

        previewStyle.normal.textColor = new Color(c1.r, c1.g, c1.b, 1);
        previewStyle.normal.background = MakePreviewBackground(new Color(c2.r, c2.g, c2.b, 1));

        // Lightness sliders
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Lightness", GUILayout.Width(120));
        lightness1 = EditorGUILayout.Slider(lightness1, 0, 1, GUILayout.Width(120));
        c1 = AdjustLightness(c1, lightness1);
        EditorGUILayout.EndVertical();
        GUILayout.Space(DEFAULT_SPACE_SIZE);
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Lightness", GUILayout.Width(120));
        lightness2 = EditorGUILayout.Slider(lightness2, 0, 1, GUILayout.Width(120));
        c2 = AdjustLightness(c2, lightness2);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        // Draw the preview
        GUILayout.Space(DEFAULT_SPACE_SIZE);
        EditorGUILayout.LabelField("Lorem Ipsum", previewStyle, GUILayout.Height(30));

        GUILayout.Space(DEFAULT_SPACE_SIZE);

        float contrast = CalculateContrast(c1, c2);
        // Contrast Labels
        EditorGUILayout.LabelField("Contrast Ratio", contrastStyle, GUILayout.Height(24));
        EditorGUILayout.LabelField($"{contrast.ToString("n2")}:1", contrastStyle, GUILayout.Height(18));

        GUILayout.Space(DEFAULT_SPACE_SIZE);
        // WCAG Labels
        EditorGUILayout.BeginHorizontal();
        bool isAA = contrast > WCAG_AA_THRESHOLD;
        EditorGUILayout.LabelField("WCAG AA:", wcagStyle, GUILayout.Width(100), GUILayout.Height(18));
        EditorGUILayout.LabelField(isAA ? "Pass" : "Fail", isAA ? passStyle : failStyle, GUILayout.Width(100), GUILayout.Height(18));
        EditorGUILayout.EndHorizontal();

        /*  if (!isAA)
          {
              EditorGUILayout.LabelField("*.", failStyle, GUILayout.Width(300), GUILayout.Height(30));
          }*/

        EditorGUILayout.BeginHorizontal();
        bool isAAA = contrast > WCAG_AAA_THRESHOLD;
        EditorGUILayout.LabelField("WCAG AAA:", wcagStyle, GUILayout.Width(100), GUILayout.Height(18));
        EditorGUILayout.LabelField(isAAA ? "Pass" : "Fail", isAAA ? passStyle : failStyle, GUILayout.Width(100), GUILayout.Height(18));
        EditorGUILayout.EndHorizontal();

        /* if (!isAAA)
         {
             EditorGUILayout.LabelField("*", failStyle, GUILayout.Width(300), GUILayout.Height(30));
         }*/

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();
    }

    private float CalculateContrast(Color c1, Color c2)
    {
        float relativeLuminancec1 = 0.2126f * GetChannelForRelativeLuminance(c1.r) + 0.7152f * GetChannelForRelativeLuminance(c1.g) + 0.0722f * GetChannelForRelativeLuminance(c1.b);
        float relativeLuminancec2 = 0.2126f * GetChannelForRelativeLuminance(c2.r) + 0.7152f * GetChannelForRelativeLuminance(c2.g) + 0.0722f * GetChannelForRelativeLuminance(c2.b);

        float l1 = Mathf.Max(relativeLuminancec1, relativeLuminancec2);
        float l2 = Mathf.Min(relativeLuminancec1, relativeLuminancec2);

        return (l1 + 0.05f) / (l2 + 0.05f);
    }

    private float GetChannelForRelativeLuminance(float f)
    {
        if (f < 0.03928f)
            return f / 12.92f;
        else
            return Mathf.Pow((f + 0.055f) / 1.055f, 2.4f);
    }

    private void TryInitStyles()
    {
        if (previewStyle == null)
        {
            previewStyle = new GUIStyle(EditorStyles.boldLabel);
            previewStyle.alignment = TextAnchor.MiddleCenter;
            previewStyle.fontSize = 16;
        }

        if (wcagStyle == null)
        {
            wcagStyle = new GUIStyle(EditorStyles.boldLabel);
            wcagStyle.fontSize = 14;
        }

        if (passStyle == null)
        {
            passStyle = new GUIStyle(wcagStyle);
            passStyle.normal.textColor = Color.green;
        }

        if (failStyle == null)
        {
            failStyle = new GUIStyle(wcagStyle);
            failStyle.normal.textColor = Color.red;
        }

        if (contrastStyle == null)
        {
            contrastStyle = new GUIStyle(EditorStyles.boldLabel);
            contrastStyle.alignment = TextAnchor.MiddleCenter;
            contrastStyle.fontSize = 16;
        }
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.alignment = TextAnchor.MiddleLeft;
            headerStyle.fontSize = 18;
        }
    }

    private Texture2D MakePreviewBackground(Color color)
    {
        Color[] colors = new Color[] { color };

        Texture2D result = new Texture2D(1, 1);
        result.SetPixels(colors);
        result.Apply();

        return result;
    }
}