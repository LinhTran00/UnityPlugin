using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections;
using Unity.EditorCoroutines.Editor;

public class SimpleLanguageChecker
{
    private string inputText = "";
    private string feedback = "";
    private string apiKey = "AIzaSyD0MFUzXp82G3_PtmJYomDzxpawIGrw1JA"; // Set your API key here
    private EditorWindow parentWindow;

   

    public void OnGUI()
    {
        GUILayout.Label("Enter Text to Simplify", EditorStyles.boldLabel);
        inputText = EditorGUILayout.TextArea(inputText, GUILayout.Height(200));

        if (GUILayout.Button("Simplify"))
        {
            // Call the method to simplify text using Gemini API
            SimplifyText(inputText);
        }

        if (!string.IsNullOrEmpty(feedback))
        {
            GUILayout.Label("Simplified Text:", EditorStyles.boldLabel);
            GUILayout.Label(feedback, EditorStyles.wordWrappedLabel);
        }
    }

    private void SimplifyText(string text)
    {
        // Start coroutine to call Gemini API for text simplification
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
                        new Part { text = "Simplify this text: " + text }
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

        // Refresh the window to display the result
        parentWindow.Repaint();
    }
}

// Helper classes to parse Gemini API response
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