using UnityEditor;
using UnityEngine;

public class FlashingEffectChecker : EditorWindow
{
    private Texture2D currentFrameTexture;
    private Texture2D previousFrameTexture;
    private Color[] pixelColors;
    private Color[] previousPixelColors;

    private Camera mainCamera;
    private RenderTexture renderTexture;

    [MenuItem("Window/Flashing Effect Checker")]
    public static void ShowWindow()
    {
        GetWindow<FlashingEffectChecker>("Flashing Effect Checker");
    }

    private void OnEnable()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found. Please ensure there is a camera tagged as 'MainCamera' in the scene.");
            return;
        }

        renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        currentFrameTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        previousFrameTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        pixelColors = new Color[Screen.width * Screen.height];
        previousPixelColors = new Color[Screen.width * Screen.height];
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Check Flashing Effect"))
        {
            CheckFlashingEffect();
        }

        if (previousPixelColors != null)
        {
            GUILayout.Label("Previous Frame:");
            GUILayout.Label(new GUIContent(previousFrameTexture), GUILayout.Width(200), GUILayout.Height(100));

            GUILayout.Label("Current Frame:");
            CaptureCameraFrame();
            GUILayout.Label(new GUIContent(currentFrameTexture), GUILayout.Width(200), GUILayout.Height(100));
        }
    }

    private void CaptureCameraFrame()
    {
        mainCamera.targetTexture = renderTexture;
        mainCamera.Render();

        RenderTexture.active = renderTexture;
        currentFrameTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        currentFrameTexture.Apply();

        mainCamera.targetTexture = null;
        RenderTexture.active = null;
    }

    private void CheckFlashingEffect()
    {
        CaptureCameraFrame();
        pixelColors = currentFrameTexture.GetPixels();

        float flashCount = 0;

        for (int i = 0; i < pixelColors.Length; i++)
        {
            float colorDifference = Mathf.Abs(pixelColors[i].r - previousPixelColors[i].r) +
                                    Mathf.Abs(pixelColors[i].g - previousPixelColors[i].g) +
                                    Mathf.Abs(pixelColors[i].b - previousPixelColors[i].b);

            if (colorDifference > 0.5f)
            {
                flashCount++;
            }
        }

        float flashPercentage = (flashCount / pixelColors.Length) * 100;
        GUILayout.Label($"Flashing effect detected: {flashPercentage.ToString("F2")}% of pixels changed significantly.");

        // Copy current frame pixels to previous frame texture and array
        currentFrameTexture.GetPixels().CopyTo(previousPixelColors, 0);
        Graphics.CopyTexture(currentFrameTexture, previousFrameTexture);

        if (flashPercentage > 25)
        {
            GUILayout.Label("Suggestion: Reduce flashing effects to prevent discomfort or potential health risks for players.");
        }
    }
}
