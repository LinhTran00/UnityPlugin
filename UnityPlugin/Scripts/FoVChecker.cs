using UnityEngine;
using UnityEditor;

public class FovChecker
{
    private Camera mainCamera;
    
    private string[] gameTypes = { "General", "FPS", "Third Person", "Racing/Simulation", "VR" };
    private int selectedGameType = 0;

    private GUIStyle header1Style;
    private GUIStyle header2Style;
    private GUIStyle normalStyle;
    private GUIStyle failStyle;
    private GUIStyle passStyle;

    public bool isFail = false;
    public float fovValue;
    public string gameType;
    public string result = "Pass";
    public string description = "";

    // Constructor to initialize the main camera
    public FovChecker()
    {
        
        mainCamera = Camera.main;
    }

    public void onEnable()
    {
        fovValue = mainCamera.fieldOfView;
        CheckFoVForGameType();
    }

    // Method to display FoV-related GUI
    public void OnGUI()
    {
        InitializeGUIStyles();

        // Section 1: FoV Check
        GUILayout.Label("FoV Check", header1Style);
        GUILayout.Space(5);

        GUILayout.Label("Select Game Type", header2Style);
        int newSelectedGameType = EditorGUILayout.Popup("Game Type", selectedGameType, gameTypes);

        GUILayout.Space(10);

        // If the game type selection has changed, update it
        if (newSelectedGameType != selectedGameType)
        {
            selectedGameType = newSelectedGameType;
            // Force a GUI refresh on game type change
            GUI.changed = true;
        }

        if (Is3DGame())
        {
            if (mainCamera != null)
            {
                fovValue = mainCamera.fieldOfView;

                // Show current FoV and check against game type thresholds
                CheckFoVForGameType();
            }
            else
            {
                GUILayout.Label("No camera found. Please ensure a main camera is active in the scene.");
            }
        }
        else
        {
            GUILayout.Label("FoV check is only for 3D engines.");
        }

        GUILayout.Space(20); // Add some space between the sections

        // Section 2: FoV Simulation
        GUILayout.Label("FoV Simulation", header1Style);
        GUILayout.Space(5);

        // Instruction for FoV Simulation
        GUILayout.Label("Note: The FoV Simulation is only usable after the game scene is paused during play mode.", normalStyle);
        GUILayout.Space(10);

        if (mainCamera != null)
        {
            // Display current FoV value
            GUILayout.Label($"Current FoV: {mainCamera.fieldOfView}", normalStyle);

            // Allow the user to adjust the FoV for testing purposes
            float newFoV = EditorGUILayout.Slider("Adjust FoV", mainCamera.fieldOfView, 1f, 179f);

            // Apply the new FoV if it's different from the current value
            if (!Mathf.Approximately(newFoV, mainCamera.fieldOfView))
            {
                mainCamera.fieldOfView = newFoV;
                // Force a GUI refresh on FoV change
                GUI.changed = true;
                Debug.Log($"FoV changed to: {mainCamera.fieldOfView}");
            }
        }
        else
        {
            GUILayout.Label("No camera found. Please ensure a main camera is active in the scene.");
        }

        GUILayout.Space(10);
        GUILayout.Label(description, normalStyle);
        GUILayout.Space(10);
        // Display FoV result
        GUILayout.BeginHorizontal();
        GUILayout.Label("Current FoV:", header2Style, GUILayout.Width(120));
        GUILayout.Label($"{fovValue}", isFail ? failStyle : passStyle);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Check:", header2Style, GUILayout.Width(120));
        GUILayout.Label(result, isFail ? failStyle : passStyle);
        GUILayout.EndHorizontal();
    }

    // Method to check FoV based on the selected game type and provide results
    private void CheckFoVForGameType()
    {
        // Reset isFail at the start of the method to recheck the conditions
        isFail = false;

        

        switch (gameTypes[selectedGameType])
        {
            case "FPS":
                if (fovValue < 90f || fovValue > 110f)
                {
                    result = "Fail";
                    isFail = true;
                }
                gameType = "FPS";
                description = "In FPS games, FoV is typically between 90° to 110° for optimal player awareness and comfort.";
                break;
            case "Third Person":
                if (fovValue < 60f || fovValue > 80f)
                {
                    result = "Fail";
                    isFail = true;
                }
                gameType = "Third Person";
                description = "Third-person games often use a FoV between 60° to 80°, providing balance between character visibility and spatial awareness.";
                break;
            case "Racing/Simulation":
                if (fovValue < 75f || fovValue > 120f)
                {
                    result = "Fail";
                    isFail = true;
                }
                gameType = "Racing/Simulation";
                description = "For racing or simulation games, FoV ranges from 75° to 120° to simulate peripheral vision and immersion.";
                break;
            case "VR":
                if (fovValue < 90f || fovValue > 120f)
                {
                    result = "Fail";
                    isFail = true;
                }
                description = "In VR games, a FoV between 90° and 120° provides a realistic and comfortable experience.";
                break;
            case "General":
                if (fovValue < 60f || fovValue > 120f)
                {
                    result = "Fail";
                    isFail = true;
                }
                gameType = "General";
                description = "For every game, extreme FoV ranges outside 60° to 120° are typically uncomfortable and can lead to visual distortion.";
                break;
        }
        
    }

    // Method to determine if the game is running in a 3D environment
    private bool Is3DGame()
    {
        return Camera.allCameras.Length > 0;
    }

    private void InitializeGUIStyles()
    {
        header1Style = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 18,
            wordWrap = true,
        };
        header2Style = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            wordWrap = true,
        };
        normalStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 14,
            wordWrap = true,
        };
        failStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            wordWrap = true,
            normal = { textColor = Color.red },
        };
        passStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            wordWrap = true,
            normal = { textColor = Color.green },
        };
    }
}
