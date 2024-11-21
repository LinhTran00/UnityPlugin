using UnityEngine;
using UnityEditor;

public class FovChecker
{
    private Camera mainCamera;
    
    private string[] gameTypes = { "General", "FPS", "Third Person", "Racing and Simulation", "VR" };
    private int selectedGameType = 0;
    private string gameTypeDetails  = "General games include a wide range of genres, offering flexible gameplay experiences without strict visual requirements.";
    private float adjustedFoV;

    private float initialFoV; // To store the original FoV
    private bool isFoVInitialized = false; // To ensure the initial FoV is set only once

    private bool isResultInitial = false;
    private bool initialResult;

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
        initialFoV = mainCamera.fieldOfView;
        CheckFoVForGameType();
    }

    // Method to display FoV-related GUI
    public void OnGUI()
    {
        InitializeGUIStyles();

        // Check for camera
        if (Is3DGame())
        {
            if (mainCamera != null)
            {
                // Initialize the default FoV only once
                if (!isFoVInitialized)
                {
                    initialFoV = mainCamera.fieldOfView; // Save the initial value
                    fovValue = initialFoV;
                    isFoVInitialized = true;
                }

                // Validate the default/current FoV against game type thresholds
                CheckFoVForGameType();

                if (!isResultInitial)
                {
                    initialResult = isFail;
                    isResultInitial = true;
                }
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

        // Section 1: FoV Check
        GUILayout.Label("FoV Check (Field of View)", header1Style);
        GUILayout.Space(10);

        // Game type selection
        GUILayout.Label("Select Game Type", header2Style);
        int newSelectedGameType = EditorGUILayout.Popup("Game Type", selectedGameType, gameTypes);
        GUILayout.Space(10);

        // Description of the game type

        GUILayout.Label("Description: ", header2Style);
        GUILayout.Label("   " + gameTypeDetails, normalStyle);
        GUILayout.Space(10);
        GUILayout.Label("Criteria: ", header2Style);
        GUILayout.Label("   " + description, normalStyle);
        GUILayout.Space(15);

        // Show the default/current FoV of the scene
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Default/Current FoV:", header2Style, GUILayout.Width(180));
        EditorGUILayout.LabelField($"{fovValue}", initialResult ? failStyle : passStyle);
        GUILayout.EndHorizontal();

        // Show the result of the FoV check
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("FoV Check:", header2Style, GUILayout.Width(180));
        EditorGUILayout.LabelField(result, initialResult ? failStyle : passStyle); // Result based on saved state
        GUILayout.EndHorizontal();

        // Update game type selection if changed
        if (newSelectedGameType != selectedGameType)
        {
            selectedGameType = newSelectedGameType;

            // re-initialize isFoVInitialized = false;
            isResultInitial = false;

            // Revalidate the default/current FoV against new thresholds
            CheckFoVForGameType();

            initialResult = isFail;
            isResultInitial = true;

            // Force GUI refresh for the check section
            GUI.changed = true;
        }

        GUILayout.Space(20); // Add space between sections
           
        /*
        // Section 2: FoV Simulation
        GUILayout.Label("FoV Simulation", header1Style);
        GUILayout.Space(5);


        
        // Instruction for simulation
        GUILayout.Label("Note: The FoV Simulation is only usable after the game scene is paused during play mode.", normalStyle);
        GUILayout.Space(10);
        
        // Adjust the simulation FoV without affecting the default/current FoV
        EditorGUILayout.Slider("Adjust FoV", mainCamera.fieldOfView, 1f, 180f);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Adjusted FoV:", header2Style, GUILayout.Width(120));
        GUILayout.Label($"{mainCamera.fieldOfView}", normalStyle);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Check:", header2Style, GUILayout.Width(120));
        GUILayout.Label(result, isFail ? failStyle : passStyle);
        GUILayout.EndHorizontal();
        */
    }



    // Method to check FoV based on the selected game type and provide results
    private void CheckFoVForGameType()
    {
        // Reset isFail at the start of the method to recheck the conditions
        isFail = false;
        result = "Pass";
        

        switch (gameTypes[selectedGameType])
        {
            case "FPS":
                if (initialFoV < 90f || initialFoV > 110f)
                {
                    result = "Fail";
                    isFail = true;
                }
                gameType = "FPS";
                description = "In <b>FPS games</b>, FoV is typically between <b>90° to 110°</b> for optimal player awareness and comfort.";
                gameTypeDetails = "<b>FPS (First-Person Shooter) games</b> are action games where players view the game world from the protagonist's perspective, often focused on precision and reaction speed.";
                break;
            case "Third Person":
                if (initialFoV < 60f || initialFoV > 80f)
                {
                    result = "Fail";
                    isFail = true;
                }
                gameType = "Third Person";
                description = "<b>Third-person games</b> often use a FoV between <b>60° to 80°</b>, providing balance between character visibility and spatial awareness.";
                gameTypeDetails = "<b>Third-person games</b> allow players to see their character on the screen, offering a broader view of the environment and character movement.";
                break;
            case "Racing and Simulation":
                if (initialFoV < 75f || initialFoV > 120f)
                {
                    result = "Fail";
                    isFail = true;
                }
                gameType = "Racing/Simulation";
                description = "For <b>racing or simulation games</b>, FoV ranges from <B>75° to 120°</b> to simulate peripheral vision and immersion.";
                gameTypeDetails = "<b>Racing/Simulation games</b> focus on realism and accuracy, often replicating real-world scenarios like driving or piloting.";
                break;
            case "VR":
                if (initialFoV < 90f || initialFoV > 120f)
                {
                    result = "Fail";
                    isFail = true;
                }
                gameType = "VR";
                description = "In <b>VR games</b>, a FoV between <b>90° and 120°</b> provides a realistic and comfortable experience.";
                gameTypeDetails = "<b>VR (Virtual Reality)</b> games immerse players in a fully 3D virtual environment, using specialized hardware like VR headsets for a realistic experience.";
                break;
            case "General":
                if (initialFoV < 60f ||   initialFoV > 120f)
                {
                    result = "Fail";
                    isFail = true;
                }
                gameType = "General";
                description = "For <b> general game </b>, the recommended FoV is typically between <b>60° and 120°</b>.This range is a general guideline that ensures player comfort and minimizes visual distortion, making it suitable for games across all genres.";
                gameTypeDetails = "<b>General games</b> include a wide range of genres, offering flexible gameplay experiences without strict visual requirements.";
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
        };
        normalStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            wordWrap = true,
            richText = true,
        };
        failStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            normal = { textColor = Color.red },
            hover = { textColor = Color.red },
        };
        passStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            wordWrap = true,
            normal = { textColor = Color.green },
            hover = { textColor = Color.green },
        };
    }
}
