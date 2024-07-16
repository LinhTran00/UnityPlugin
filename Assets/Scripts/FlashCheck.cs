using UnityEngine;
using System.IO;
using UnityEditor;

public class FlashingCheck
{
    private Camera mainCamera;
    private int sampleSize = 128; // Size of the sample texture
    private Texture2D screenTexture;
    private Color[] pixelColors;
    private Color[] previousPixelColors; // Array to store previous frame colors\

    private GUIStyle header;
    private GUIStyle labelStyle;

    public void OnEnable()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found. Please ensure there is a Camera tagged as MainCamera.");
            return;
        }

        screenTexture = new Texture2D(sampleSize, sampleSize, TextureFormat.RGB24, false);
        pixelColors = new Color[sampleSize * sampleSize];
        previousPixelColors = new Color[sampleSize * sampleSize]; // Initialize the previous frame array
    }
    public void OnGUI()
    {
        header = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 18
        };
        GUILayout.Label("Flashing Check", header, GUILayout.Height(18));
        // Check for flashing effect and display the result
         CheckFlashingEffect();
    }
    private void CheckFlashingEffect()
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
        GUILayout.Label($"Flashing effect detected: {flashPercentage.ToString("F2")}% of pixels changed significantly.");

        // Save the current frame colors for the next comparison
        pixelColors.CopyTo(previousPixelColors, 0);

        if (flashPercentage > 25) // Threshold for significant flashing effect
        {
            GUILayout.Label($"Suggestion: Reduce flashing effects to prevent discomfort or potential health risks for players.");
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
