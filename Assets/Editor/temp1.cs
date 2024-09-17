using UnityEngine;
using UnityEditor;
using System.Diagnostics; // For opening URLs

public class CustomChecklistWindow : EditorWindow
{
    private enum CheckBoxState
    {
        Done,
        NotDone,
        NotApplicable
    }

    private static string[] tabTitles = new string[]
    {
        "Motor",
        "Cognitive",
        "Vision",
        "Hearing",
        "Speech",
        "General"
    };

    private static string[][] checklistCategories = new string[][]
    {
        new string[] { "Control / Mobility" },
        new string[] { "Thought / Memory / Processing Information" },
        new string[] { "Sight-Related" },
        new string[] { "Hearing-Related" },
        new string[] { "Speech Input/Output" },
        new string[] { "General Accessibility" }
    };

    private static string[][][] checklistItems = new string[][][]
    {
        // Motor
        new string[][]
        {
            new string[] // Basic
            {
                "Include an option to adjust the game speed",
                "Include toggle/slider for any haptics",
                "Ensure interactive elements / virtual controls are large and well spaced",
                "Include an option to adjust the sensitivity of controls"
            },
            new string[] // Intermediate
            {
                "Do not rely on motion tracking of specific body types",
                "Provide a macro system",
                "Allow interfaces to be resized",
                "Avoid / provide alternatives to requiring buttons to be held down"
            },
            new string[] // Advanced
            {
                "Provide simple control schemes for assistive devices",
                "Include a cool-down period between inputs"
            }
        },
        // Cognitive
        new string[][]
        {
            new string[] // Basic
            {
                "Avoid flickering images and repetitive patterns",
                "Allow players to progress through text prompts at their own pace"
            },
            new string[] // Intermediate
            {
                "Highlight important words",
                "Provide a choice of text color, low/high contrast"
            },
            new string[] // Advanced
            {
                "Provide an option to disable blood and gore",
                "Provide pre-recorded voiceovers for all text"
            }
        },
        // Vision
        new string[][]
        {
            new string[] // Basic
            {
                "Provide high contrast between text/UI and background",
                "Use simple clear text formatting"
            },
            new string[] // Intermediate
            {
                "Provide a choice of cursor/crosshair colors/designs",
                "Ensure screen reader support for mobile devices"
            },
            new string[] // Advanced
            {
                "Provide an audio description track"
            }
        },
        // Hearing
        new string[][]
        {
            new string[] // Basic
            {
                "If any subtitles/captions are used, present them clearly",
                "Provide separate volume controls or mutes"
            },
            new string[] // Intermediate
            {
                "Provide a stereo/mono toggle",
                "Ensure subtitles/captions can be customized"
            },
            new string[] // Advanced
            {
                "Provide signing for key interactions"
            }
        },
        // Speech
        new string[][]
        {
            new string[] // Basic
            {
                "Ensure that speech input is not required"
            },
            new string[] // Intermediate
            {
                "Base speech recognition on individual words from a small vocabulary"
            }
        },
        // General
        new string[][]
        {
            new string[] // Basic
            {
                "Solicit accessibility feedback",
                "Ensure that all settings are saved/remembered"
            },
            new string[] // Intermediate
            {
                "Provide an autosave feature"
            },
            new string[] // Advanced
            {
                "Realtime text-to-speech transcription"
            }
        }
    };

    // Example URLs for checklist items (you can replace these with actual URLs)
    private static string[][][] checklistItemUrls = new string[][][]
    {
        // Motor
        new string[][]
        {
            new string[] // Basic
            {
                "https://gameaccessibilityguidelines.com/include-an-option-to-adjust-the-game-speed/",
                "https://gameaccessibilityguidelines.com/include-toggle-slider-for-any-haptics/",
                "https://gameaccessibilityguidelines.com/ensure-interactive-elements-virtual-controls-are-large-and-well-spaced-particularly-on-small-or-touch-screens/",
                "https://gameaccessibilityguidelines.com/include-an-option-to-adjust-the-sensitivity-of-controls/"
            },
            new string[] // Intermediate
            {
                "https://gameaccessibilityguidelines.com/do-not-rely-on-motion-tracking-of-specific-body-types/",
                "https://gameaccessibilityguidelines.com/provide-a-macro-system/",
                "https://gameaccessibilityguidelines.com/allow-interfaces-to-be-resized/",
                "https://gameaccessibilityguidelines.com/avoid-provide-alternatives-to-requiring-buttons-to-be-held-down/"
            },
            new string[] // Advanced
            {
                "https://gameaccessibilityguidelines.com/provide-very-simple-control-schemes-that-are-compatible-with-assistive-technology-devices-such-as-switch-or-eye-tracking/",
                "https://gameaccessibilityguidelines.com/include-a-cool-down-period-post-acceptance-delay-of-0-5-seconds-between-inputs/"
            }
        },
        // Add URLs for other categories following the same structure...
    };

    private CheckBoxState[][][] itemStates;
    private int selectedTab = 0;
    private GUIStyle clickableTextStyle;

    [MenuItem("Window/Custom Checklist")]
    public static void ShowWindow()
    {
        GetWindow<CustomChecklistWindow>("Custom Checklist");
    }

    private void OnEnable()
    {
        // Initialize the item states
        itemStates = new CheckBoxState[checklistItems.Length][][];
        for (int categoryIndex = 0; categoryIndex < checklistItems.Length; categoryIndex++)
        {
            itemStates[categoryIndex] = new CheckBoxState[checklistItems[categoryIndex].Length][];
            for (int difficultyIndex = 0; difficultyIndex < checklistItems[categoryIndex].Length; difficultyIndex++)
            {
                itemStates[categoryIndex][difficultyIndex] = new CheckBoxState[checklistItems[categoryIndex][difficultyIndex].Length];
            }
        }

        // Create a custom style for clickable text
        clickableTextStyle = new GUIStyle(EditorStyles.label);
        clickableTextStyle.normal.textColor = Color.white; // Set text color
        clickableTextStyle.hover.textColor = Color.blue;   // Set hover color
    }

    private void OnGUI()
    {
        GUILayout.Label("Accessibility Checklist", EditorStyles.boldLabel);

        // Tabs for categories
        selectedTab = GUILayout.Toolbar(selectedTab, tabTitles);

        GUILayout.Space(10);
        GUILayout.Label(tabTitles[selectedTab], EditorStyles.boldLabel);
        GUILayout.Label("(" + checklistCategories[selectedTab][0] + ")", EditorStyles.miniLabel);

        // Draw sections for Basic, Intermediate, Advanced
        DrawChecklistSection("Basic", selectedTab, 0);
        DrawChecklistSection("Intermediate", selectedTab, 1);
        DrawChecklistSection("Advanced", selectedTab, 2);
    }

    private void DrawChecklistSection(string label, int categoryIndex, int difficultyIndex)
    {
        GUILayout.Space(10);
        GUILayout.Label(label, EditorStyles.largeLabel);

        for (int i = 0; i < checklistItems[categoryIndex][difficultyIndex].Length; i++)
        {
            EditorGUILayout.BeginHorizontal();

            // Draw the custom checkbox
            itemStates[categoryIndex][difficultyIndex][i] = DrawCustomCheckbox(itemStates[categoryIndex][difficultyIndex][i]);

            // Draw clickable label with custom text style
            if (GUILayout.Button(checklistItems[categoryIndex][difficultyIndex][i], clickableTextStyle))
            {
                OpenURL(checklistItemUrls[categoryIndex][difficultyIndex][i]);
            }

            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);

            EditorGUILayout.EndHorizontal();
        }
    }

    private CheckBoxState DrawCustomCheckbox(CheckBoxState state)
    {
        Rect checkboxRect = GUILayoutUtility.GetRect(20, 20, GUILayout.Width(20), GUILayout.Height(20));

        EditorGUI.DrawRect(checkboxRect, Color.white);
        EditorGUI.DrawRect(new Rect(checkboxRect.x + 2, checkboxRect.y + 2, checkboxRect.width - 4, checkboxRect.height - 4), Color.black);

        switch (state)
        {
            case CheckBoxState.Done:
                EditorGUI.DrawRect(new Rect(checkboxRect.x + 5, checkboxRect.y + 5, checkboxRect.width - 10, checkboxRect.height - 10), Color.green);
                break;
            case CheckBoxState.NotDone:
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.red;
                style.fontSize = 14;
                EditorGUI.LabelField(checkboxRect, "X", style);
                break;
            case CheckBoxState.NotApplicable:
                EditorGUI.DrawRect(new Rect(checkboxRect.x + 5, checkboxRect.y + 5, checkboxRect.width - 10, checkboxRect.height - 10), Color.white);
                EditorGUI.DrawRect(new Rect(checkboxRect.x + 5, checkboxRect.y + 5, checkboxRect.width - 10, checkboxRect.height / 2), Color.black);
                break;
        }

        // Handle click event to cycle through states
        if (Event.current.type == EventType.MouseDown && checkboxRect.Contains(Event.current.mousePosition))
        {
            state = (CheckBoxState)(((int)state + 1) % 3);
            Event.current.Use(); // Consume the event to prevent it from propagating
        }

        return state;
    }

    private void OpenURL(string url)
    {
        // Open URL in the default browser
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
}
