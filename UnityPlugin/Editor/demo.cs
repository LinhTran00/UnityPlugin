using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEditor;
using UnityEditor.ShaderGraph.Legacy;
using UnityEngine;

public class demo : EditorWindow
{
    private enum Tab
    {
        Tab0,
        Tab1,
        Tab2,
        Tab3,
        Tab4,
        Tab5,
        Tab6,
        Tab7,
        Tab8,
        Tab9,
        Tab10
       
        // Add more tabs as needed
    }

    private Tab currentTab;
    private ColorChecker colorChecker; // Instance of ColorTextChecker class
    private ColorblindSimulator simulator; // Instance of ColorblindSimulator
    private FlashingCheck flashingCheck;
    private BrightnessChecker brightnessChecker;
    private TextChecker textChecker;
    private SimpleLanguageChecker simpleLanguageChecker;
    private FovChecker fovChecker;
    private VolumeChecker volumeChecker;

    private string customFileName = ""; // To store the user-input file name


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
 

        currentTab = (Tab)GUILayout.Toolbar((int)currentTab, new string[] { "*","Report", "Color Contrast", "Text", "Colorblind", "Brightness", "Flash", "Language", "FoV", "Volume", "TTS"/* Add tab names */ });

        switch (currentTab)
        {
            case Tab.Tab0:
                DrawTab0();
                break;
            case Tab.Tab1:
                Report();
                break;
            case Tab.Tab2:
                DrawTab2();
                break;
            case Tab.Tab3:
                DrawTab3();
                break;
            case Tab.Tab4:
                DrawTab4();
                break;
            case Tab.Tab5:
                DrawTab5();
                break;
            case Tab.Tab6:
                DrawTab6();
                break;
            case Tab.Tab7:
                DrawTab7();
                break;
            case Tab.Tab8:
                DrawTab8();
                break;
            case Tab.Tab9:
                DrawTab9();
                break;
            case Tab.Tab10:
                DrawTab10();
                break;
            default:
                break;
        }
    }

    private void DrawTab0()
    {
        GUILayout.Label("Table of Contents", EditorStyles.boldLabel);

        GUILayout.Space(10); // Add some spacing between title and content

        // List all tabs and their descriptions
        DrawTabDescription("1", "Color Contrast");
        DrawTabDescription("2", "Text");
        DrawTabDescription("3", "Colorblind Simulator");
        DrawTabDescription("4", "Brightness Check");
        DrawTabDescription("5", "Flash Check");
        DrawTabDescription("6", "Simple Language Check");
        DrawTabDescription("7", "Field Of View");
        DrawTabDescription("8", "Volume Check");
        DrawTabDescription("9", "Text-to-speech");
        GUILayout.FlexibleSpace(); // Add flexible space to push content to the top

        // Optionally, add a footer or any additional content at the bottom of Tab0
    }
    private void DrawTab2()
    {
        colorChecker.OnGUI();
    }
    private void DrawTab3()
    {
        textChecker.OnGUI();
    }
    private void DrawTab4()
    {
        simulator.OnGUI();
    }
    private void DrawTab5()
    {
        brightnessChecker.OnGUI();
    }
    private void DrawTab6()
    {
        flashingCheck.OnGUI();
    }
    private void DrawTab7()
    {
        simpleLanguageChecker.OnGUI();
    }
    private void DrawTab8()
    {
        fovChecker.OnGUI(); 
    }
    private void DrawTab9()
    {
        volumeChecker.OnGUI();
    }
    
    private void DrawTab10()
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
        string csv_content = "";
        GUILayout.Label("Report", EditorStyles.boldLabel);

        // Example grid layout
        GUILayout.BeginVertical("box");

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
        
        GUILayout.EndVertical();

        // Text field for custom file name input
        GUILayout.Label("Enter custom file name to generate report:");
        customFileName = GUILayout.TextField(customFileName, GUILayout.Width(400));

        // Add a button to export the report as a CSV file
        if (GUILayout.Button("Export Report to CSV"))
        {
            ExportReportToCSV(customFileName, csv_content);
        }
    }

    private void ExportReportToCSV(string fileName, string content)
    {
        // If the file name is empty, use a default name
        if (string.IsNullOrEmpty(fileName))
        {
            fileName = "report"; // Default file name
        }

        // Define the folder path (Reports folder inside the project)
        string folderPath = Path.Combine(Application.dataPath,"Reports");

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
