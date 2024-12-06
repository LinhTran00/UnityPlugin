using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections;
using Unity.EditorCoroutines.Editor;

public class SimpleLanguageChecker
{
    private string inputText = "";
    private string feedback = "";
    private string feedbackConvert = "";
    private string apiKey = "AIzaSyDxcTyrAdgUwd-NZuIFpRHBA-B-5c6Ebc0"; // Replace with your API key safely
    private GUIStyle normal;
    private GUIStyle normalBold;
    private GUIStyle header1Style;
    private GUIStyle header2Style;
    private GUIStyle normalStyle;
    private GUIStyle header3Style;
    private GUIStyle textAreaStyle;
    private GUIStyle buttonStyle;
    private GUIStyle notify;
    private Vector2 scrollPosition = Vector2.zero;
    public void OnGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

        InitializeGUIStyles();

        GUILayout.Label("Simplify Language", header1Style);
        GUILayout.Space(15);

        GUILayout.Label("Objective:", normalBold);
        GUILayout.Label("This feature leverages AI to evaluate the simplicity of game instructions, ensuring they are clear and easy to understand, " +
            "particularly for individuals with cognitive impairments.", normalStyle);
        GUILayout.Space(15);

        GUILayout.Label("Enter Text to Simplify:", normalBold);

        // Input text area
        inputText = EditorGUILayout.TextArea(inputText, textAreaStyle, GUILayout.Height(200));

        // Simplify button
        if (GUILayout.Button("Simplify", buttonStyle))
        {
            SimplifyText(inputText);
        }
        GUILayout.Space(15);
        
        // Feedback area
        if (!string.IsNullOrEmpty(feedback))
        {
            GUILayout.Label("Simplified Text:", normalBold);
            GUILayout.Space(5);
            // Convert Markdown to Rich Text
            feedbackConvert = ConvertMarkdownToRichText(feedback);
            GUILayout.Label("**The portion below is AI generated.**", notify);
            GUILayout.Space(10);
            GUILayout.Label(feedbackConvert, normal);
        }
        GUILayout.EndScrollView();
    }

    private void InitializeGUIStyles()
    {
        normal = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            wordWrap = true,
            richText = true
        };
        normalBold = new(EditorStyles.boldLabel)
        {
            fontSize = 16,
            wordWrap = true
        };
        header1Style = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 18,
            wordWrap = true,
        };

        normalStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 14,
            wordWrap = true,
        };
        textAreaStyle = new GUIStyle(EditorStyles.textArea)
        {
            fontSize = 14, // Increase font size
            wordWrap = true, // Wrap long lines
            padding = new RectOffset(10, 10, 10, 10) // Add padding around text
        };
        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 20, // Set the font size
            fontStyle = FontStyle.Bold, // Optional: Make the font bold
            alignment = TextAnchor.MiddleCenter, // Optional: Center align the text
        };
        notify = new GUIStyle(EditorStyles.boldLabel)
        {
            fontStyle = FontStyle.Italic,
            fontSize = 12,
            normal = {textColor = Color.yellow},
            hover = {textColor = Color.yellow},  
        };
    }

    private string ConvertMarkdownToRichText(string markdownText)
    {
        // Replace Markdown-style **text** with Unity Rich Text <b>text</b>
        bool isBold = false;
        return System.Text.RegularExpressions.Regex.Replace(markdownText, @"\*\*", match =>
        {
            isBold = !isBold; // Toggle between opening and closing tags
            return isBold ? "<b>" : "</b>";
        });
    }

    private void SimplifyText(string text)
    {
        
        // Start coroutine to simplify text using Gemini API
        EditorCoroutineUtility.StartCoroutine(SendTextToGemini(text), this);
    }

    private IEnumerator SendTextToGemini(string text)
    {
        string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={apiKey}";

        RequestData requestData = new RequestData
        {
            contents = new[]
            {
                new Content
                {
                    parts = new[]
                    {
                        new Part { text = "Simplify this game instruction while maintaining clarity and context. Also provide 3 options with these categories - concise and direct, descriptive, playful and engaging - to choose from: " + text }
                    }
                }
            }
        };

        string jsonData = JsonUtility.ToJson(requestData);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + request.downloadHandler.text);

            var geminiResponse = JsonUtility.FromJson<GeminiResponse>(request.downloadHandler.text);

            if (geminiResponse.candidates != null && geminiResponse.candidates.Length > 0)
            {
                feedback = geminiResponse.candidates[0].content.parts[0].text.Trim();
            }
            else
            {
                feedback = "No response text received!";
            }
        }
        else
        {
            feedback = "Error: " + request.error;
            Debug.LogError("Request error: " + request.error);
        }

        // Request repaint for the focused editor window
        EditorWindow.focusedWindow.Repaint();
        GUI.changed = true;
    }
}

// Helper classes for Gemini API request and response
[System.Serializable]
public class RequestData
{
    public Content[] contents;
}

[System.Serializable]
public class GeminiResponse
{
    public Candidate[] candidates;
}

[System.Serializable]
public class Candidate
{
    public Content content;
}

[System.Serializable]
public class Content
{
    public Part[] parts;
}

[System.Serializable]
public class Part
{
    public string text;
}
