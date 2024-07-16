using UnityEditor;
using UnityEngine;

public class SimpleLanguageChecker
{
    private string inputText = "";
    private string feedback = "";
    public void OnGUI()
    {
        GUILayout.Label("Enter Text to Evaluate", EditorStyles.boldLabel);
        inputText = EditorGUILayout.TextArea(inputText, GUILayout.Height(200));

        if (GUILayout.Button("Evaluate"))
        {
            feedback = EvaluateText(inputText);
        }

        if (!string.IsNullOrEmpty(feedback))
        {
            GUILayout.Label("Feedback:", EditorStyles.boldLabel);
            GUILayout.Label(feedback, EditorStyles.wordWrappedLabel);
        }
    }

    private string EvaluateText(string text)
    {
        string feedback = "";
        string[] sentences = text.Split(new[] { '.', '!', '?' }, System.StringSplitOptions.RemoveEmptyEntries);
        bool isClear = true;

        foreach (string sentence in sentences)
        {
            if (sentence.Length > 100)
            {
                feedback += "Consider breaking down long sentences. ";
                isClear = false;
            }
            if (CountComplexWords(sentence) > 3)
            {
                feedback += "Consider simplifying complex words. ";
                isClear = false;
            }
            if (ContainsPassiveVoice(sentence))
            {
                feedback += "Consider using active voice. ";
                isClear = false;
            }
            if (ContainsComplexStructure(sentence))
            {
                feedback += "Consider simplifying the sentence structure. ";
                isClear = false;
            }
        }

        if (isClear)
        {
            feedback = "The text is clear and simple.";
        }

        return feedback;
    }

    private int CountComplexWords(string sentence)
    {
        string[] complexWords = { "preferences", "character", "continuation", "requires" }; // Add more complex words as needed
        string[] words = sentence.Split(' ');
        int complexWordCount = 0;

        foreach (string word in words)
        {
            if (word.Length > 7 || System.Array.Exists(complexWords, w => w.Equals(word, System.StringComparison.OrdinalIgnoreCase)))
            {
                complexWordCount++;
            }
        }

        return complexWordCount;
    }

    private bool ContainsPassiveVoice(string sentence)
    {
        // Basic check for passive voice using common patterns
        string[] passiveIndicators = { "is", "are", "was", "were", "be", "being", "been" };
        string[] words = sentence.Split(' ');

        for (int i = 0; i < words.Length - 1; i++)
        {
            if (System.Array.Exists(passiveIndicators, w => w.Equals(words[i], System.StringComparison.OrdinalIgnoreCase)) &&
                words[i + 1].EndsWith("ed"))
            {
                return true;
            }
        }

        return false;
    }

    private bool ContainsComplexStructure(string sentence)
    {
        // Check for complex structure patterns
        string[] complexPatterns = { "that you", "so that", "in order to", "as a result of", "due to the fact that" };
        foreach (string pattern in complexPatterns)
        {
            if (sentence.Contains(pattern))
            {
                return true;
            }
        }

        return false;
    }
}
