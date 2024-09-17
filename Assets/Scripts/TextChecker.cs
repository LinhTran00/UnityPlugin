using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class TextChecker
{
    private Vector2 scrollPosition;

    // Styles for GUI elements
    private GUIStyle wcagStyle;
    private GUIStyle passStyle;
    private GUIStyle failStyle;
    private GUIStyle headerStyle;
    private GUIStyle yellowStyle;
    private GUIStyle cyanStyle;
    private List<string> recommendedFonts = new List<string> { "Arial", "Verdana", "Tahoma", "Helvetica", "Roboto", "LiberationSans SDF" };

    private Camera mainCamera;
    public void OnEnable()
    {
        mainCamera = Camera.main;
        TryInitStyles();
    }

    public void OnGUI()
    {
        GUILayout.Space(12);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(12);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.BeginVertical(GUILayout.Width(400));
        EditorGUILayout.LabelField("Text Enhancer", headerStyle, GUILayout.Height(30));

        // Perform checks and display results
        CheckTextProperties(mainCamera);

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();
    }

    private void CheckTextProperties(Camera mainCamera)
    {
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera is not assigned.");
            return;
        }

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        List<GameObject> visibleObjects = new List<GameObject>();

        TMP_Text[] textMeshes = GameObject.FindObjectsOfType<TMP_Text>();
        TMP_InputField[] inputFields = GameObject.FindObjectsOfType<TMP_InputField>();
        Text[] uiTexts = GameObject.FindObjectsOfType<Text>();

        foreach (TMP_Text textMesh in textMeshes)
        {
            if (GeometryUtility.TestPlanesAABB(planes, textMesh.bounds))
            {
                visibleObjects.Add(textMesh.gameObject);
            }
        }

        foreach (TMP_InputField inputField in inputFields)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(inputField.GetComponent<RectTransform>(), mainCamera.WorldToScreenPoint(inputField.transform.position), mainCamera))
            {
                visibleObjects.Add(inputField.gameObject);
            }
        }

        foreach (Text uiText in uiTexts)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(uiText.GetComponent<RectTransform>(), mainCamera.WorldToScreenPoint(uiText.transform.position), mainCamera))
            {
                visibleObjects.Add(uiText.gameObject);
            }
        }

        DisplayTextProperties(visibleObjects);
    }

    private void DisplayTextProperties(List<GameObject> visibleObjects)
    {
        int count = 1;
        foreach (GameObject obj in visibleObjects)
        {
            DisplayTextComponentInfo(obj, count);
            count++;
        }
    }

    private void DisplayTextComponentInfo(GameObject obj, int count)
    {
        TMP_Text textMesh = obj.GetComponent<TMP_Text>();
        if (yellowStyle == null)
        {
            yellowStyle = new GUIStyle(wcagStyle);
            yellowStyle.normal.textColor = Color.yellow;
            yellowStyle.wordWrap = true; // Enable word wrapping
        }

        if (cyanStyle == null)
        {
            cyanStyle = new GUIStyle(wcagStyle);
            cyanStyle.normal.textColor = Color.cyan;
            cyanStyle.wordWrap = true; // Enable word wrapping
        }

        if (textMesh != null)
        {
            EditorGUILayout.LabelField($"{count}. Text: {textMesh.text}", yellowStyle, GUILayout.Height(24));
            EditorGUILayout.LabelField($"    Original Font: {textMesh.font.name}, Size: {textMesh.fontSize}", wcagStyle, GUILayout.Height(24));
            ValidateFontSize(textMesh.fontSize);
            ValidateFont(textMesh.font.name);
            if (IsMultiline(textMesh.text))
            {
                ValidateLongTextProperties(textMesh.text, textMesh.lineSpacing);
            }
        }

        TMP_InputField inputField = obj.GetComponent<TMP_InputField>();
        if (inputField != null)
        {
            EditorGUILayout.LabelField($"{count}. Object: {obj.name}", yellowStyle, GUILayout.Height(24));
            EditorGUILayout.LabelField($"    InputField Text: {inputField.text}", wcagStyle, GUILayout.Height(24));
            EditorGUILayout.LabelField($"    Original Font: {inputField.textComponent.font.name}, Size: {inputField.textComponent.fontSize}", wcagStyle, GUILayout.Height(24));
            ValidateFontSize(inputField.textComponent.fontSize);
            ValidateFont(inputField.textComponent.font.name);
            if (IsMultiline(inputField.text))
            {
                ValidateLongTextProperties(inputField.text, inputField.textComponent.lineSpacing);
            }
        }

        Text uiText = obj.GetComponent<Text>();
        if (uiText != null)
        {
            EditorGUILayout.LabelField($"{count}. Object: {obj.name}", yellowStyle, GUILayout.Height(24));
            EditorGUILayout.LabelField($"    UI Text: {uiText.text}", wcagStyle, GUILayout.Height(24));
            EditorGUILayout.LabelField($"    Original Font: {uiText.font.name}, Size: {uiText.fontSize}", wcagStyle, GUILayout.Height(24));
            ValidateFontSize(uiText.fontSize);
            ValidateFont(uiText.font.name);
            if (IsMultiline(uiText.text))
            {
                ValidateLongTextProperties(uiText.text, uiText.lineSpacing);
            }
        }
    }

    private bool IsMultiline(string text)
    {
        return text.Contains("\n");
    }

    private void ValidateFontSize(float fontSize)
    {
        bool isPass = fontSize >= 16; // Assuming desktop standard for simplicity
        string suggestion = isPass ? "Pass" : "Increase font size to at least 16.";

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("    Font Size Check:", wcagStyle, GUILayout.Width(200), GUILayout.Height(20));
        GUIStyle style = isPass ? passStyle : failStyle;
        string resultText = isPass ? "Pass" : "Fail";
        EditorGUILayout.LabelField(resultText, style, GUILayout.Width(50), GUILayout.Height(20));
        EditorGUILayout.EndHorizontal();

        if (!isPass)
        {
            EditorGUILayout.LabelField("          Suggestion:    " + suggestion, cyanStyle, GUILayout.Width(500), GUILayout.Height(30));
        }
    }

    private void ValidateFont(string fontName)
    {
        bool isPass = recommendedFonts.Contains(fontName);
        string suggestion = isPass ? "Pass" : "Consider using a more readable font like Arial, Verdana, or Roboto.";

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("    Font Check:", wcagStyle, GUILayout.Width(200), GUILayout.Height(20));
        GUIStyle style = isPass ? passStyle : failStyle;
        string resultText = isPass ? "Pass" : "Fail";
        EditorGUILayout.LabelField(resultText, style, GUILayout.Width(50), GUILayout.Height(20));
        EditorGUILayout.EndHorizontal();

        if (!isPass)
        {
            EditorGUILayout.LabelField("          Suggestion:    " + suggestion, cyanStyle, GUILayout.Width(500), GUILayout.Height(30));
        }
    }

    private void ValidateLongTextProperties(string text, float lineSpacing)
    {
        bool isMixedCase = !text.Equals(text.ToUpper()) && !text.Equals(text.ToLower());
        bool isLineSpacingCorrect = lineSpacing >= 1.5f;
        bool isCharactersPerLineCorrect = text.Length <= 70;

        string caseSuggestion = isMixedCase ? "Pass" : "Avoid using all caps.";
        string lineSpacingSuggestion = isLineSpacingCorrect ? "Pass" : "Increase line spacing to at least 1.5.";
        string charPerLineSuggestion = isCharactersPerLineCorrect ? "Pass" : "Reduce characters per line to around 70.";

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("    Mixed Case Check:", wcagStyle, GUILayout.Width(200), GUILayout.Height(20));
        GUIStyle style = isMixedCase ? passStyle : failStyle;
        string resultText = isMixedCase ? "Pass" : "Fail";
        EditorGUILayout.LabelField(resultText, style, GUILayout.Width(50), GUILayout.Height(20));
        EditorGUILayout.EndHorizontal();
        if (!isMixedCase)
        {
            EditorGUILayout.LabelField("          Suggestion:    " + caseSuggestion, cyanStyle, GUILayout.Width(500), GUILayout.Height(30));
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("    Line Spacing Check:", wcagStyle, GUILayout.Width(200), GUILayout.Height(20));
        style = isLineSpacingCorrect ? passStyle : failStyle;
        resultText = isLineSpacingCorrect ? "Pass" : "Fail";
        EditorGUILayout.LabelField(resultText, style, GUILayout.Width(50), GUILayout.Height(18));
        EditorGUILayout.EndHorizontal();
        if (!isLineSpacingCorrect)
        {
            EditorGUILayout.LabelField("          Suggestion:    " + lineSpacingSuggestion, cyanStyle, GUILayout.Width(500), GUILayout.Height(30));
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("    Characters Per Line Check:", wcagStyle, GUILayout.Width(200), GUILayout.Height(20));
        style = isCharactersPerLineCorrect ? passStyle : failStyle;
        resultText = isCharactersPerLineCorrect ? "Pass" : "Fail";
        EditorGUILayout.LabelField(resultText, style, GUILayout.Width(50), GUILayout.Height(20));
        EditorGUILayout.EndHorizontal();
        if (!isCharactersPerLineCorrect)
        {
            EditorGUILayout.LabelField("          Suggestion:    " + charPerLineSuggestion, cyanStyle, GUILayout.Width(500), GUILayout.Height(30));
        }
    }

    private void TryInitStyles()
    {
        if (wcagStyle == null)
        {
            wcagStyle = new GUIStyle(EditorStyles.boldLabel);
            wcagStyle.fontSize = 14;
        }

        if (passStyle == null)
        {
            passStyle = new GUIStyle(wcagStyle);
            passStyle.normal.textColor = Color.green;
        }

        if (failStyle == null)
        {
            failStyle = new GUIStyle(wcagStyle);
            failStyle.normal.textColor = Color.red;
        }

        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 20;
        }
    }
}
