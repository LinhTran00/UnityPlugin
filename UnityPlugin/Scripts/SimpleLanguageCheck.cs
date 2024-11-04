using System.Linq;
using UnityEditor;
using UnityEngine;

public class SimpleLanguageChecker
{
    private string inputText = "";
    private string feedback = "";
    private string revisedText = "";
    private string suggestion = "";
    private GUIStyle boldStyle;
    private GUIStyle wordWrappedStyle;

    public void OnGUI()
    {
        // Initialize styles if they are null
        if (boldStyle == null)
        {
            boldStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold
            };
        }
        if (wordWrappedStyle == null)
        {
            wordWrappedStyle = new GUIStyle(GUI.skin.label)
            {
                wordWrap = true
            };
        }

        GUILayout.Label("Enter Text to Evaluate", EditorStyles.boldLabel);
        inputText = EditorGUILayout.TextArea(inputText, GUILayout.Height(200));

        if (GUILayout.Button("Evaluate"))
        {
            feedback = EvaluateText(inputText);
        }

        if (!string.IsNullOrEmpty(feedback))
        {
            GUILayout.Label("Feedback:", EditorStyles.boldLabel);
            GUILayout.Label(feedback, wordWrappedStyle);

            GUILayout.Label("Suggestion:", boldStyle);
            GUILayout.Label(suggestion, wordWrappedStyle);

            GUILayout.Label("Revised Text:", EditorStyles.boldLabel);
            GUILayout.Label(revisedText, wordWrappedStyle);
        }
    }

    private string EvaluateText(string text)
    {
        feedback = "";
        suggestion = "";
        revisedText = text;
        string[] sentences = text.Split(new[] { '.', '!', '?' }, System.StringSplitOptions.RemoveEmptyEntries);
        bool isClear = true;

        foreach (string sentence in sentences)
        {
            string trimmedSentence = sentence.Trim();
            if (trimmedSentence.Length > 100)
            {
                feedback += "Consider breaking down long sentences.\n";
                isClear = false;
            }
            if (CountComplexWords(trimmedSentence) > 3)
            {
                feedback += "Consider simplifying complex words.\n";
                isClear = false;
            }
            if (ContainsPassiveVoice(trimmedSentence))
            {
                string activeVoiceSentence = ConvertToActiveVoice(trimmedSentence);
                feedback += "Sentence uses passive voice.\n";
                suggestion += "Avoid using 'is', 'are', 'was', 'were', 'be', 'being', 'been'.\n";
                revisedText = revisedText.Replace(trimmedSentence, activeVoiceSentence);
                isClear = false;
            }
            if (ContainsComplexStructure(trimmedSentence))
            {
                feedback += "Consider simplifying the sentence structure.\n";
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
        string[] complexWords = { "preferences", "continuation", "requires" }; // Add more complex words as needed
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

    private string ConvertToActiveVoice(string sentence)
    {
        // A simple and not always accurate way to convert passive to active
        string[] passiveIndicators = { "is", "are", "was", "were", "be", "being", "been" };
        string[] words = sentence.Split(' ');
        for (int i = 0; i < words.Length - 1; i++)
        {
            if (System.Array.Exists(passiveIndicators, w => w.Equals(words[i], System.StringComparison.OrdinalIgnoreCase)) &&
                words[i + 1].EndsWith("ed"))
            {
                string subject = "Someone";
                string action = words[i + 1];
                string restOfSentence = string.Join(" ", words.Skip(i + 2));
                return $"{subject} {action} {restOfSentence}";
            }
        }

        // If no passive structure is found, return the original sentence
        return sentence;
    }
}
