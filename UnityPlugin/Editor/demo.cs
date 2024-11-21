using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEditor;
using UnityEditor.ShaderGraph.Legacy;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Analytics;

public class demo : EditorWindow
{
    private enum Tab
    {
        Homepage,
        GenerateReport, // Comma added here
        ColorContrastChecker, // Comma added here
        TextChecker, // Comma added here
        ColorblindChecker, // Comma added here
        BrightnessChecker, // Comma added here
        FlashingChecker, // Comma added here
        FoVChecker, // Comma added here
        VolumeChecker, // Comma added here
        SimpleLanguageChecker, // Comma added here
        TextToSpeech, // Comma added here
    }

    private Tab currentTab = Tab.Homepage;
    private bool showMenu = false; // Toggle for the tab menu
    private ColorChecker colorChecker;
    private ColorblindSimulator simulator;
    private FlashingCheck flashingCheck;
    private BrightnessChecker brightnessChecker;
    private TextChecker textChecker;
    private SimpleLanguageChecker simpleLanguageChecker;
    private FovChecker fovChecker;
    private VolumeChecker volumeChecker;
    private string customFileName = "";

    [MenuItem("Window/demo")]
    public static void ShowWindow()
    {
        GetWindow<demo>("demo");
    }

    private void OnEnable()
    {
        colorChecker = new ColorChecker();
        colorChecker.OnEnable();

        textChecker = new TextChecker();
        textChecker.OnEnable();

        simulator = new ColorblindSimulator();
        simulator.OnEnable();

        brightnessChecker = new BrightnessChecker();
        brightnessChecker.OnEnable();

        flashingCheck = new FlashingCheck();
        flashingCheck.OnEnable();

        simpleLanguageChecker = new SimpleLanguageChecker();

        fovChecker = new FovChecker();
        fovChecker.onEnable();

        volumeChecker = new VolumeChecker();
        volumeChecker.OnEnable();
    }

private void OnGUI()
{
    GUILayout.BeginHorizontal();

    // Hamburger menu button
    if (GUILayout.Button(showMenu ? "X" : "☰", GUILayout.Width(30), GUILayout.Height(30)))
    {
        showMenu = !showMenu; // Toggle the menu visibility
    }

    if (showMenu)
    {
        // Sidebar with options and a dividing line, giving it a distinct box-like look
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.margin = new RectOffset(10, 10, 10, 10); // Add some margins for a clean look

        GUILayout.BeginVertical(boxStyle, GUILayout.Width(150), GUILayout.ExpandHeight(true));
        GUILayout.Space(10);

        // Change label text to "Features", set font size to 14 pt and center-align
        GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
        labelStyle.fontSize = 14;
        labelStyle.alignment = TextAnchor.MiddleCenter; // Center-align the label
        GUILayout.Label("Features", labelStyle);

        // Display tabs as a list of left-aligned labels instead of buttons
        foreach (var tab in Enum.GetValues(typeof(Tab)))
        {
            // Set a default style for the tab
            GUIStyle tabStyle = new GUIStyle();
            tabStyle.alignment = TextAnchor.MiddleLeft; // Align text to the left
            tabStyle.normal.textColor = Color.white; // Default color for the text

            // Create a rect for the current tab, to detect if it's being hovered
            Rect tabRect = GUILayoutUtility.GetLastRect();
            bool isHovered = tabRect.Contains(Event.current.mousePosition);

            // If hovering, change the text color to yellow
            if (isHovered)
            {
                tabStyle.normal.textColor = Color.yellow; // Highlight the tab name when hovered
            }

            // Create the tab as a "button" but styled as a label
            if (GUILayout.Button(tab.ToString(), tabStyle, GUILayout.Height(30)))
            {
                currentTab = (Tab)tab; // Set the selected tab
                showMenu = false; // Close the menu after selecting a tab
            }

            // Line separator between tabs
            GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true)); // Line after each tab
        }

        GUILayout.FlexibleSpace();
        GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true)); // Divider line
        if (GUILayout.Button("Back to Homepage"))
        {
            currentTab = Tab.Homepage;
            showMenu = false; // Close menu when going back to homepage
        }

        GUILayout.EndVertical();
    }

    // Main content area
    GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

    // Display the content of the current tab
    switch (currentTab)
    {
        case Tab.Homepage:
            DrawHomepage();
            break;
        case Tab.GenerateReport:
            Report();
            break;
        case Tab.ColorContrastChecker:
            DrawTabColorContrast();
            break;
        case Tab.TextChecker:
            DrawTabText();
            break;
        case Tab.ColorblindChecker:
            DrawTabColorblind();
            break;
        case Tab.BrightnessChecker:
            DrawTabBrightness();
            break;
        case Tab.FlashingChecker:
            DrawTabFlash();
            break;
        case Tab.FoVChecker:
            DrawTabFoV();
            break;
        case Tab.VolumeChecker:
            DrawTabVolume();
            break;
        case Tab.SimpleLanguageChecker:
            DrawTabLanguage();
            break;
        case Tab.TextToSpeech:
            DrawTabTTS();
            break;
        default:
            break;
    }

    GUILayout.EndVertical();
    GUILayout.EndHorizontal();
}

    private Vector2 scrollPosition; // To track the scroll position

    private void DrawHomepage()
    {
        // Set custom font style and color for the main heading
        GUIStyle mainHeadingStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 20, // Larger font for main title
            normal = { textColor = new Color(0.9f, 0.95f, 1.0f) }, // Light blue/white for dark backgrounds
            hover = { textColor = new Color(0.9f, 0.95f, 1.0f) }
        };

        // Set font style and color for numbered list heading
        GUIStyle featureHeadingStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 15, // Smaller font size for feature titles
            normal = { textColor = new Color(0.9f, 0.95f, 1.0f) }, // Same heading color
            hover = { textColor = new Color(0.9f, 0.95f, 1.0f) }
        };

        // Set style for the descriptions under each feature
        GUIStyle featureStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 15,
            wordWrap = true,
            normal = { textColor = new Color(0.8f, 0.8f, 0.85f) }, // Light grey for readability on dark backgrounds
            hover = { textColor = new Color(0.8f, 0.8f, 0.85f) }
        };

        // Set style for header of the instruction description
        GUIStyle headerParagraphInstruct = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 15,
            wordWrap = true,
            normal = { textColor = new Color(0.9f, 0.95f, 1.0f) },
            hover = { textColor = new Color(0.9f, 0.95f, 1.0f) }
        };

        // Set style for the bold/highlight instruction description
        GUIStyle boldParagraphInstruct = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 15,
            wordWrap = false,
            normal = { textColor = new Color(0.9f, 0.95f, 1.0f) },
            hover = { textColor = new Color(0.9f, 0.95f, 1.0f) }
        };

        // Set style for normal text instruction description
        GUIStyle normalParagraphInstruct = new GUIStyle(EditorStyles.label)
        {
            fontSize = 15,
            wordWrap = false,
        };

        // Start the scroll view
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

        GUILayout.Label("Welcome to the Accessibility Plugin", mainHeadingStyle); // Main heading
        GUILayout.Space(10);

        GUILayout.Label("To get the best results from our plugin, follow these steps to ensure optimal functionality and accurate information capture for your game scene:", headerParagraphInstruct);
        GUILayout.Space(5);

        GUILayout.Label("1. Start by playing the game", boldParagraphInstruct, GUILayout.ExpandWidth(false));
        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Label("2. Pause the game ", boldParagraphInstruct, GUILayout.ExpandWidth(false));
        GUILayout.Label("when you reach a specific scene you’d like to analyze.", normalParagraphInstruct, GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Label("3. Close and reopen this window ", boldParagraphInstruct, GUILayout.ExpandWidth(false));
        GUILayout.Label("to refresh the plugin and ensure all data is accurately loaded.", normalParagraphInstruct, GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Label("4. Use and explore the plugin’s features ", boldParagraphInstruct, GUILayout.ExpandWidth(false));
        GUILayout.Label("to gather and review detailed information for this scene.", normalParagraphInstruct, GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();


        GUILayout.Space(40);

        GUILayout.Label("Our plugin includes the following features to help make your application more accessible:", boldParagraphInstruct);
        GUILayout.Space(5);

        // Numbered list with heading-style color for each title
        DrawFeature("1. Color Contrast Checker", "      Ensure that text and background colors have sufficient contrast.", featureHeadingStyle, featureStyle);
        DrawFeature("2. Text Checker", "      Evaluate text readability and accessibility.", featureHeadingStyle, featureStyle);
        DrawFeature("3. Colorblind Simulator", "      View your application as it would appear to colorblind users.", featureHeadingStyle, featureStyle);
        DrawFeature("4. Brightness Checker", "      Analyze screen brightness levels for visibility.", featureHeadingStyle, featureStyle);
        DrawFeature("5. Flashing Check", "      Detect potentially harmful flashing elements.", featureHeadingStyle, featureStyle);
        DrawFeature("6. Field of View (FoV) Checker", "      Measure visible areas in a 3D scene.", featureHeadingStyle, featureStyle);
        DrawFeature("7. Volume Checker", "      Verify volume levels for user comfort.", featureHeadingStyle, featureStyle);
        DrawFeature("8. Language Checker", "      Assess text for simplicity and clarity.", featureHeadingStyle, featureStyle);
        DrawFeature("9. Text-To-Speech (TTS)", "      Integrate audio feedback using Amazon Polly.", featureHeadingStyle, featureStyle);

        // End the scroll view
        GUILayout.EndScrollView();
    }


    private void DrawFeature(string title, string description, GUIStyle titleStyle, GUIStyle featureStyle)
    {
        GUILayout.Label(title, titleStyle);  // Applying the feature title style with smaller font size
        GUILayout.Label(description, featureStyle);
        GUILayout.Space(5);
    }

    private void DrawTabColorContrast() => colorChecker.OnGUI();
    private void DrawTabText() => textChecker.OnGUI();
    private void DrawTabColorblind() => simulator.OnGUI();
    private void DrawTabBrightness() => brightnessChecker.OnGUI();
    private void DrawTabFlash() => flashingCheck.OnGUI();
    private void DrawTabFoV() => fovChecker.OnGUI();
    private void DrawTabVolume() => volumeChecker.OnGUI();
    private void DrawTabLanguage() => simpleLanguageChecker.OnGUI();
    private void DrawTabTTS()
    {
        GUILayout.Label("How to Use Text-To-Speech (TTS)", EditorStyles.boldLabel);
        GUILayout.Space(10);
        GUILayout.Label("1. Create an Amazon Polly account and get an access key and secret key.", EditorStyles.wordWrappedLabel);
        GUILayout.Space(5);
        GUILayout.Label("2. Copy and paste the keys into TextToSpeech.cs.", EditorStyles.wordWrappedLabel);
        GUILayout.Space(5);
        GUILayout.Label("3. Create a button in your Unity project.", EditorStyles.wordWrappedLabel);
        GUILayout.Space(5);
        GUILayout.Label("4. Click on the button -> Click 'Add Component' in the Inspector -> Choose 'HoverDetector'.", EditorStyles.wordWrappedLabel);
        GUILayout.Space(5);
        GUILayout.Label("5. Add a label in the 'Label To Speak' input field that you want a voiceover when the mouse hovers over it.", EditorStyles.wordWrappedLabel);
        GUILayout.Space(5);
        GUILayout.Label("6. Start the game and enjoy the voiceover functionality.", EditorStyles.wordWrappedLabel);
    }
    private void Report()
    {
        // initate style for report tab
        GUIStyle mainHeadingStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 20, // Larger font for main title
        };
        GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14, // Larger font for main title
        };
        GUIStyle highlightStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 12, // Larger font for main title
            normal = { textColor = Color.yellow },
            hover = { textColor = Color.yellow }
        };
        GUIStyle highlightStyle1 = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 12, // Larger font for main title
            normal = { textColor = Color.yellow },
            hover = { textColor = Color.yellow },
            fontStyle = FontStyle.Italic
        };

        string csv_content = "";
        GUILayout.Label("Report", mainHeadingStyle);
        GUILayout.Space(10);

        // flash check
        csv_content += AddReportRow
        (
            "Flashing", flashingCheck.flashValue.ToString("0.00") + "%",
            flashingCheck.flashPass ? "Pass" : "Fail", 
            flashingCheck.flashPass ? "<= 30%" : "> 30%", 
            flashingCheck.flashPass ? "" : "Reduce flashing effects to prevent discomfort or potential health risks for players"
        );

        // blue light check
        csv_content += AddReportRow
        (
            "Blue Light", flashingCheck.bluelightValue.ToString("0.00") + "%", 
            flashingCheck.bluelightPass ? "Pass" : "Fail", 
            flashingCheck.bluelightPass ? "<= 30%" : "> 30%", 
            flashingCheck.bluelightPass ? "" : "Reduce blue light intensity to prevent eye strain or provide a blue light filter option"
        );

        // text check (font, size, color contrast)
        foreach (TextProperties textProperty in textChecker.textPropertiesList)
        {
            bool passFont = textChecker.recommendedFonts.Contains(textProperty.fontName);
            bool passSize = textProperty.fontSize >= 16;
            // Join recommended fonts and enclose in quotes
            // Create a string of recommended fonts
            string recommendedFonts = string.Join(", ", textChecker.recommendedFonts.Take(textChecker.recommendedFonts.Count - 1));

            // Handle the last font separately if there are any recommended fonts
            if (textChecker.recommendedFonts.Count > 0)
            {
                recommendedFonts += " and " + textChecker.recommendedFonts.Last();
            }

            // Font check for each text object
            csv_content += AddReportRow
            (
                $"Font ({textProperty.content})",
                textProperty.fontName,
                passFont ? "Pass" : "Fail",
                passFont ? $"\"Use:\n{recommendedFonts}\"" : $"\"Not Use:\n{recommendedFonts}\"",
                passFont ? "" : $"\"Use:\n{recommendedFonts}\""
            );



            // Font size check for each text object
            csv_content += AddReportRow
            (
                $"Font Size ({textProperty.content})", 
                textProperty.fontSize.ToString("0.00"), 
                passSize ? "Pass" : "Fail", 
                passSize ? ">16" : "<16", 
                passSize ? "" : "Increase font size to at least 16"
            );


            // Color contrast
            string topass = "";
            string tofail = "";
            // WCAG AA
            bool passCriteriaAA = textProperty.fontSize >= 18 && textProperty.contrastRatio >= 3.0;
            bool failCriteriaAA = textProperty.fontSize >= 18 && textProperty.contrastRatio < 3.0;
            // WCAG AAA
            bool passCriteriaAAA = textProperty.fontSize >= 18 && textProperty.contrastRatio >= 4.5;
            bool failCriteriaAAA = textProperty.fontSize >= 18 && textProperty.contrastRatio < 7.0;

            topass = passCriteriaAA
                    ? "font size >= 18 & contrast ratio >= 3.0"
                    : "font size < 18 & contrast ratio >= 4.5";

            tofail = failCriteriaAA
                    ? "font size >= 18 & contrast ratio < 3.0"
                    : "font size < 18 & contrast ratio < 4.5";
        
            csv_content += AddReportRow
            (
                $"WCAG AA ({textProperty.content})",
                $"\"contrast ratio: {textProperty.contrastRatio}\ntext size: {textProperty.fontSize}\"",
                //$"\"foreground: #{ColorUtility.ToHtmlStringRGB(textProperty.text)}\nbackground: #{ColorUtility.ToHtmlStringRGB(textProperty.background)}\ntext size: {textProperty.fontSize}\"",
                textChecker.wcagaa ? "Pass" : "Fail",
                textChecker.wcagaa && passCriteriaAA ? topass : tofail,
                textChecker.wcagaa ? "" : "increase font size or increase contrast ratio"
            );

            topass = passCriteriaAAA
                    ? "font size >= 18 & contrast ratio >= 4.5"
                    : "font size < 18 & contrast ratio >= 7.0";

            tofail = failCriteriaAAA
                    ? "font size >= 18 & contrast ratio < 4.5"
                    : "font size < 18 & contrast ratio < 7.0";
                
            csv_content += AddReportRow
            (
                $"WCAG AAA ({textProperty.content})",
                $"\"contrast ratio: {textProperty.contrastRatio}\ntext size: {textProperty.fontSize}\"",
                //$"\"foreground: #{ColorUtility.ToHtmlStringRGB(textProperty.text)}\nbackground: #{ColorUtility.ToHtmlStringRGB(textProperty.background)}\ntext size: {textProperty.fontSize}\"",
                textChecker.wcagaaa ? "Pass" : "Fail",
                textChecker.wcagaaa && passCriteriaAAA ? topass : tofail,
                textChecker.wcagaaa ? "" : "increase font size or increase contrast ratio"
            );

               
        }
        

        // brightness check
        csv_content += AddReportRow
        (
            "Brightness", 
            brightnessChecker.brightnessReport.ToString("0.00") + " (" + brightnessChecker.shortSuggestion + ")",
            brightnessChecker.brightnessReport < 0.2f || brightnessChecker.brightnessReport > 0.8f ? "Fail" : "Pass",
            brightnessChecker.brightnessReport < 0.2f || brightnessChecker.brightnessReport > 0.8f ? "too dark or too bright" : "not too dark or too bright",
            brightnessChecker.brightnessReport < 0.2f || brightnessChecker.brightnessReport > 0.8f ? "increase the brightness or reduce the brightness" : "reference to the purpose of the setting of the game"
        );

        string suggestion = "";
        string reason = "";
        switch (fovChecker.gameType)
        {
            case "FPS":
                if (fovChecker.isFail)
                {
                    reason = "< 90 degree or > 110 degree";
                    suggestion = "In FPS games, FoV is typically between 90° to 110° for optimal player awareness and comfort.";
                }
                else
                {
                    reason = ">= 90 or <= 110 degree";
                }
                break;
            case "Third Person":
                if (fovChecker.isFail)
                {
                    reason = "< 60 degree or > 80 degree";
                    suggestion = "Third-person games often use a FoV between 60° to 80°, providing balance between character visibility and spatial awareness.";
                }
                else
                {
                    reason = ">= 60 or <= 80 degree";
                }
                break;
            case "Racing/Simulation":
                if (fovChecker.isFail)
                {
                    reason = "< 75 degree or > 120 degree";
                    suggestion = "For racing or simulation games, FoV ranges from 75° to 120° to simulate peripheral vision and immersion.";
                }
                else
                {
                    reason = ">= 75 or <= 120 degree";
                }
                break;
            case "VR":
                if (fovChecker.isFail)
                {
                    reason = "< 90 degree or > 120 degree";
                    suggestion = "In VR games, a FoV between 90° and 120° provides a realistic and comfortable experience.";
                }
                else
                {
                    reason = ">= 90 or <= 120 degree";
                }
                break;
            case "General":
                if (fovChecker.isFail)
                {
                    reason = "< 60 degree or > 120 degree";
                    suggestion = "For every game, extreme FoV ranges outside 60° to 120° are typically uncomfortable and can lead to visual distortion.";
                }
                else
                {
                    reason = ">= 60 or <= 120 degree";
                }
                break;
        }

        //fov check
        csv_content += AddReportRow
        (
            "FoV", 
            $"\"fov: {fovChecker.fovValue.ToString("0.00")}\ngame type: {fovChecker.gameType}\"", 
            fovChecker.isFail ? "Fail" : "Pass",
            reason,
            suggestion
        );

        // volume
        VolumeProperties volumeProperty = volumeChecker.volume;
        
        // master volume
        csv_content += AddReportRow
        (   
            "Master Volume",
            $"{volumeProperty.masterVolume} dB",
            volumeProperty.masterPass == 3 ? "Pass" : "Fail",
            volumeProperty.masterPass == 1 ? "< -12 dB" :
            volumeProperty.masterPass == 2 ? "> -6 dB" :
            "-12 dB to -6 dB",
            volumeProperty.masterSuggestion
        );
        // voice volume
        csv_content += AddReportRow
        (
            "Voice Volume",
            $"{volumeProperty.voiceVolume} dB",
            volumeProperty.voicePass == 3 ? "Pass" : "Fail",
            volumeProperty.voicePass == 1 ? "< -12 dB" :
            volumeProperty.voicePass == 2 ? "> -6 dB" :
            "-12 dB to -6 dB",
            volumeProperty.voiceSuggestion
        );
        // music volume
        csv_content += AddReportRow
        (
            "Music Volume",
            $"{volumeProperty.musicVolume} dB",
            volumeProperty.musicPass == 3 ? "Pass" : "Fail",
            volumeProperty.musicPass == 1 ? "< -20 dB" :
            volumeProperty.musicPass == 2 ? "> -12 dB" :
            "-20 dB to -12 dB",
            volumeProperty.musicSuggestion
        );
        // SFX volume
        csv_content += AddReportRow
        (
            "SFX Volume",
            $"{volumeProperty.sfxVolume} dB",
            volumeProperty.sfxPass == 3 ? "Pass" : "Fail",
            volumeProperty.sfxPass == 1 ? "< -12 dB" :
            volumeProperty.sfxPass == 2 ? "> -3 dB" :
            "-12 dB to -3 dB",
            volumeProperty.sfxSuggestion
        );
        

        // Text field for custom file name input
        GUILayout.Label("Enter custom file name to generate report: ", labelStyle);
        GUILayout.Space(5);
        customFileName = GUILayout.TextField(customFileName, GUILayout.Width(400));
        GUILayout.Space(5);

        // Add a button to export the report as a CSV file
        if (GUILayout.Button("Export Report to CSV", GUILayout.Width(400)))
        {
            ExportReportToCSV(customFileName, csv_content);
        }

        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        GUILayout.Label("**Find the file inside ", highlightStyle, GUILayout.ExpandWidth(false));
        GUILayout.Label("UnityPlugin/Reports", highlightStyle1, GUILayout.ExpandWidth(false));
        GUILayout.Label(" folder**", highlightStyle);
        GUILayout.EndHorizontal();
    }

    private void ExportReportToCSV(string fileName, string content)
    {
        // If the file name is empty, use a default name
        if (string.IsNullOrEmpty(fileName))
        {
            fileName = "report"; // Default file name
        }

        // Define the folder path (Reports folder inside the UnityPlugin folder inside the project)
        string folderPath = Path.Combine(Application.dataPath, "UnityPlugin", "Reports");

        // Ensure the directory exists
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Combine folder path and file name to create the full path
        string filePath = Path.Combine(folderPath, fileName + ".csv");


        try
        {
            // Write all lines to the CSV file
            File.WriteAllText(filePath, content);

            // Display a success message in the Unity Editor
            Debug.Log($"Report exported to {filePath}");
        }
        catch (System.Exception e)
        {
            // Handle any errors (e.g., invalid path or write permissions)
            Debug.LogError($"Error exporting report: {e.Message}");
        }
    }
    private string AddReportRow(string checkName, string value, string result, string reason, string suggestion)
    { 
        return $"{checkName},{value},{result},{reason},{suggestion}\n";
    }
    private void DrawTabDescription(string tabName, string description)
    {
        GUILayout.Label($"{tabName} - {description}");
        GUILayout.Space(5); // Add spacing between each tab description
    }

}
