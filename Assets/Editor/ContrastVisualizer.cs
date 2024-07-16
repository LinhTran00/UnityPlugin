using UnityEngine;
using UnityEditor;
using System.Linq;

public class ContrastToolWindow : EditorWindow
{
    private bool isAnalyzing = false;
    private bool useColor = false;
    private float InEditorOpacity = 1f;

    private RenderTexture renderTexture;
    private Texture2D simulatedTexture;

    float[] normalWeights =
        { 1, 1, 1,
          1, 1, 1,
          1, 1, 1 };
    float[] gausianWeights =
        { 1, 2, 1,
          2, 4, 2,
          1, 2, 1 };

    private Camera mainCamera;
    private float Threshold = 0f;

    [MenuItem("Window/Contrast Analysis")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ContrastToolWindow));
    }

    private void OnGUI()
    {
        GUILayout.Label("Contrast Analysis Tool", EditorStyles.boldLabel);

        mainCamera = EditorGUILayout.ObjectField("Main Camera", mainCamera, typeof(Camera), true) as Camera;

        // a lot of this UI is admittedly copied from Linh's code
        EditorGUI.BeginDisabledGroup(isAnalyzing);
        if ( GUILayout.Button("Capture Contrast Visualization"))
        {
            StartAnalysis();
        }

        if (GUILayout.Button("Turn " + (useColor? "Off" : "On") +" Direction Coloring"))
        {
            useColor = !useColor;
            if (simulatedTexture != null)
                StartAnalysis();
            else
                Repaint();
        }
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Reset Simulation"))
        {
            ResetWindow();
        }

        if (simulatedTexture != null)
        {
            GUILayout.Label("Simulated View");
            GUILayout.Label(simulatedTexture, GUILayout.Width(position.width), GUILayout.Height(position.height - 100));
        }
    }

    // ripped directly from ColorBlindnessSimulationWindow
    private void StartAnalysis()
    {
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera is not assigned.");
            return;
        }

        // Create a RenderTexture with the same dimensions as the screen
        renderTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        mainCamera.targetTexture = renderTexture;

        isAnalyzing = true;
        EditorApplication.update += AnalyzeFrame;
    }
    private void ResetWindow()
    {
        if (mainCamera != null)
        {
            mainCamera.targetTexture = null;
        }

        renderTexture?.Release();
        renderTexture = null;

        simulatedTexture = null;

        Repaint();
    }

    private void AnalyzeFrame()
    {
        mainCamera.Render();
        RenderTexture.active = renderTexture;

        // Read pixels from the RenderTexture and apply color blindness simulation
        if (simulatedTexture == null)
        {
            simulatedTexture = new Texture2D(renderTexture.width, renderTexture.height);
        }
        simulatedTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        simulatedTexture.Apply();
        ApplyAnalysis(simulatedTexture);

        RenderTexture.active = null;
        EditorApplication.update -= AnalyzeFrame;

        isAnalyzing = false;
        Repaint();
    }

    private void ApplyAnalysis(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();
        float[] values = new float[pixels.Length];

        int[] kernal =
            { -texture.width - 1, -texture.width, -texture.width + 1,
            -1, 0, 1,
            texture.width - 1, texture.width, texture.width + 1};


        // use mostly the same process as Linh's Achromatopsia filter to grab luminance
        for (int i = 0; i < pixels.Length; i++)
        {
            // calculate luminance for each pixel
            values[i] = GetLuminocity(pixels[i]);
        }

        // Do these blurs even work?
        // perform normal blur
        values = PerformBlur(values, kernal, true);

        // perform gausian blur
        values = PerformBlur(values, kernal, false);

        // perform Sobel operation
        texture.SetPixels(SobelOperation(pixels, values, kernal));
        texture.Apply();
    }

    // modified from Linh's colorblindness tool to return luminance directly
    private float GetLuminocity(Color color)
    {
        // Example color blindness simulation logic for Achromatopsia (complete color blindness)
        // Convert RGB color to grayscale
        float luminance = color.r * 0.2126f + color.g * 0.7152f + color.b * 0.0722f;
        return luminance;
    }

    private float[] PerformBlur(float[] values, int[] kernal, bool isNormal)
    {
        float[] returnValues = new float[values.Length];

        // perform blur
        for (int i = 0; i < values.Length; i++)
        {
            // copy weights
            float[] currentWeights = isNormal? normalWeights : gausianWeights;
            float total = 0f;

            // process weights
            for (int j = 0; j < kernal.Length; j++)
            {
                if (i + kernal[j] < 0 || i + kernal[j] >= values.Length)
                    currentWeights[j] = 0f;
                else
                    total += values[i + kernal[j]] * currentWeights[j];
            }

            // calculate new value at pixel
            float weight = currentWeights.Sum();
            returnValues[i] = total / weight;
        }

        return returnValues;
    }

    private Color[] SobelOperation(Color[] image, float[] values, int[] kernal)
    {
        // get separate values for x and y axes
        // allows for atan(Y/X) for direction of edge
        float[] X_Detection = new float[values.Length];
        float[] Y_Detection = new float[values.Length];

        // Sobel operators
        float[] X_Operator =
            { -1, 0, 1,
              -2, 0, 2,
              -1, 0, 1 };
        float[] Y_Operator =
            { -1, -2, -1,
               0,  0,  0,
               1,  2,  1 };

        // find each pixel's color
        for (int i = 0; i < values.Length; i++)
        {
            float X_Weight = 0f;
            float X_Total = 0f;
            float Y_Weight = 0f;
            float Y_Total = 0f;

            // process weights
            for (int j = 0; j < kernal.Length; j++)
            {
                if (i + kernal[j] >= 0 && i + kernal[j] < values.Length)
                {
                    X_Weight += Mathf.Abs(X_Operator[j]);
                    X_Total += values[i + kernal[j]] * X_Operator[j];

                    Y_Weight += Mathf.Abs(Y_Operator[j]);
                    Y_Total += values[i + kernal[j]] * Y_Operator[j];
                }
            }

            // initialize variables
            float Value =  Mathf.Sqrt(Mathf.Pow(X_Total + Y_Total, 2) * Mathf.Pow(X_Weight + Y_Weight, 2));
            float Hue = 0f;

            // deal with Threshold
            if (Value < Threshold) Value = 0f;

            // calculate new value at pixel
            if (useColor && Value >= Threshold)
            {
                // get value of each axis independently
                float X_Result = X_Total / X_Weight;
                float Y_Result = Y_Total / Y_Weight;

                Vector2 Result = new Vector2(X_Result, Y_Result);
                float ResultAngle = Vector2.Angle(Vector2.up, Result);
                if (Result.x < 0f) ResultAngle = 360f - ResultAngle;

                // get hue angle
                Hue = ResultAngle / 360f;
                // Debug.Log("Hue Angle: " + Hue);

                // replace pixel with new color
                image[i] = Color.HSVToRGB(Hue, 1f, Value);
            }
            else
            {
                image[i] = Color.HSVToRGB(0f, 0f, Value);
            }
            
        }

        return image;
    }
}