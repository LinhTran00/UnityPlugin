using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Linq;
using System.Runtime.Remoting.Messaging;

public class ContrastToolWindow : EditorWindow
{
    private bool ToolActive = false;
    private float InEditorOpacity = 1f;

    private Shader ContrastShader;
    [SerializeField] private Camera mainCamera;

    [MenuItem("Window/Contrast Analysis")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ContrastToolWindow));
        
    }

    private void OnGUI()
    {
        GUILayout.Label("Contrast Analysis Tool", EditorStyles.boldLabel);

        mainCamera = EditorGUILayout.ObjectField("Main Camera", mainCamera, typeof(Camera), true) as Camera;

        // On Off Toggle
        if ( GUILayout.Button("Toggle Contrast Visualization"))
        {
            StartAnalysis();
        }

        GUILayout.BeginHorizontal();

        GUILayout.Label("Shader Opacity");
        InEditorOpacity = GUILayout.HorizontalSlider(InEditorOpacity, 0f, 1f);

        GUILayout.EndHorizontal();
    }

    private void StartAnalysis()
    {

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

        // perform normal blur
        values = PerformBlur(values, kernal, true);

        // perform gausian blur
        values = PerformBlur(values, kernal, false);

        // perform Sobel operation
        texture.SetPixels(SobelOperation(pixels, values, kernal, true));
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
        float[] normalWeights =
            { 1, 1, 1,
              1, 1, 1,
              1, 1, 1 };
        float[] gausianWeights =
            { 1, 2, 1,
              2, 4, 2,
              1, 2, 1 };

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

    private Color[] SobelOperation(Color[] image, float[] values, int[] kernal, bool colorDirection)
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

            // calculate new value at pixel
            if (colorDirection)
            {
                // get value of each axis independently
                float X_Result = X_Total / X_Weight;
                float Y_Result = Y_Total / Y_Weight;

                // get hue angle
                Hue = Mathf.Lerp(Mathf.Atan(Y_Result / X_Result), 0f, 2 * Mathf.PI);

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