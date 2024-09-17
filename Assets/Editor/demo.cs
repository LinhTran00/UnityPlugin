using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

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
        Tab6
       
        // Add more tabs as needed
    }

    private Tab currentTab;
    private ColorChecker colorChecker; // Instance of ColorTextChecker class
    private ColorblindSimulator simulator; // Instance of ColorblindSimulator
    private FlashingCheck flashingCheck;
    private BrightnessChecker brightnessChecker;
    private TextChecker textChecker;
    private SimpleLanguageChecker simpleLanguageChecker;

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
    }

    private void OnGUI()
    {
        currentTab = (Tab)GUILayout.Toolbar((int)currentTab, new string[] { "*","Color Contrast", "Text", "Colorblind", "Brightness", "Flash", "Language"/* Add tab names */ });

        switch (currentTab)
        {
            case Tab.Tab0:
                DrawTab0();
                break;
            case Tab.Tab1:
                DrawTab1();
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

        GUILayout.FlexibleSpace(); // Add flexible space to push content to the top

        // Optionally, add a footer or any additional content at the bottom of Tab0
    }
    private void DrawTabDescription(string tabName, string description)
    {
        GUILayout.Label($"{tabName} - {description}");
        GUILayout.Space(5); // Add spacing between each tab description
    }
    private void DrawTab1()
    {
        colorChecker.OnGUI();
    }

    private void DrawTab2()
    {
        textChecker.OnGUI();

    }

    private void DrawTab3()
    {
        simulator.OnGUI();
    }

    private void DrawTab4()
    {
        brightnessChecker.OnGUI();
    }

    private void DrawTab5()
    {
        flashingCheck.OnGUI();
    }
    private void DrawTab6()
    {
        simpleLanguageChecker.OnGUI();
    }
}
