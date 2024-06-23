using UnityEngine;
using UnityEditor;

public class ColorBlindnessSimulationWindow : EditorWindow
{
    private Camera mainCamera;
    private RenderTexture renderTexture;
    private Texture2D simulatedTexture;
    private bool isSimulating;

    [MenuItem("Window/Color Blindness Simulation")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ColorBlindnessSimulationWindow));
    }

    private void OnGUI()
    {
        GUILayout.Label("Color Blindness Simulation", EditorStyles.boldLabel);

        mainCamera = EditorGUILayout.ObjectField("Main Camera", mainCamera, typeof(Camera), true) as Camera;

        EditorGUI.BeginDisabledGroup(isSimulating);
        if (GUILayout.Button("Simulate Color Blindness"))
        {
            StartSimulation();
        }
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Reset Simulation"))
        {
            ResetSimulation();
        }

        if (simulatedTexture != null)
        {
            GUILayout.Label("Simulated View");
            GUILayout.Label(simulatedTexture, GUILayout.Width(position.width), GUILayout.Height(position.height - 100));
        }
    }

    private void StartSimulation()
    {
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera is not assigned.");
            return;
        }

        // Create a RenderTexture with the same dimensions as the screen
        renderTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        mainCamera.targetTexture = renderTexture;

        isSimulating = true;
        EditorApplication.update += SimulateFrame;
    }

    private void SimulateFrame()
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
        ApplyColorBlindnessSimulation(simulatedTexture);

        RenderTexture.active = null;
        EditorApplication.update -= SimulateFrame;

        isSimulating = false;
        Repaint();
    }

    private void ApplyColorBlindnessSimulation(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            // Apply color blindness simulation logic to each pixel color
            // You can use a library or custom logic to simulate color blindness effects
            pixels[i] = SimulateColorBlindness(pixels[i]);
        }
        texture.SetPixels(pixels);
        texture.Apply();
    }

    private Color SimulateColorBlindness(Color color)
    {
        // Example color blindness simulation logic for Achromatopsia (complete color blindness)
        // Convert RGB color to grayscale
        float luminance = color.r * 0.2126f + color.g * 0.7152f + color.b * 0.0722f;
        return new Color(luminance, luminance, luminance, color.a);
    }

    private void ResetSimulation()
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

    private void OnDestroy()
    {
        ResetSimulation();
    }
}
