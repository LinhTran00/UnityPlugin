using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

// Class to hold text properties (content, size, font)
public class TextProperties
{
    public string content;
    public float fontSize;
    public string fontName;
    public Color text;
    public Color background;
    public float contrastRatio;
    

    public TextProperties(string content, float fontSize, string fontName, Color text, Color background, float contrastRatio)
    {
        this.content = content;
        this.fontSize = fontSize;
        this.fontName = fontName;
        this.text = text;
        this.background = background;
        this.contrastRatio = contrastRatio;
    }
}

public class TextChecker
{
    private Vector2 scrollPosition;

    // Public list to hold the text properties of every visible text in the scene
    public List<TextProperties> textPropertiesList = new List<TextProperties>();

    // Styles for GUI elements
    private GUIStyle wcagStyle;
    private GUIStyle passStyle;
    private GUIStyle failStyle;
    private GUIStyle headerStyle;
    private GUIStyle yellowStyle;
    private GUIStyle cyanStyle;
    public List<string> recommendedFonts = new List<string> { "Arial", "Verdana", "Tahoma", "Helvetica", "Roboto", "LiberationSans SDF" };

    // for report 
    public bool textExist;
    public bool fontsizePass;
    public float fontsize;
    public bool fontPass;
    public string fontstyle;
    public bool wcagaa;
    public bool wcagaaa;

    private Camera mainCamera;

    public void OnEnable()
    {
        mainCamera = Camera.main;
        //CollectTextProperties();
    }

    public void OnGUI()
    {
        TryInitStyles();
        GUILayout.Space(12);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(12);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.BeginVertical(GUILayout.Width(400));
        EditorGUILayout.LabelField("Text Enhancer", headerStyle, GUILayout.Height(30));

        // Perform checks and display results
        CheckTextProperties();

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();
    }

    private void CollectTextProperties()
    { 

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
                textExist = true;
            }
        }

        foreach (TMP_InputField inputField in inputFields)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(inputField.GetComponent<RectTransform>(), mainCamera.WorldToScreenPoint(inputField.transform.position), mainCamera))
            {
                visibleObjects.Add(inputField.gameObject);
                textExist = true;
            }
        }

        foreach (Text uiText in uiTexts)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(uiText.GetComponent<RectTransform>(), mainCamera.WorldToScreenPoint(uiText.transform.position), mainCamera))
            {
                visibleObjects.Add(uiText.gameObject);
                textExist = true;
            }
        }

        CheckTextProperties1(visibleObjects);

    }


    private void CheckTextProperties1(List<GameObject> visibleObjects)
    {
        foreach (GameObject obj in visibleObjects)
        {
            // text mesh
            TMP_Text textMesh = obj.GetComponent<TMP_Text>();
            Color textColor = textMesh.color;
            Color backgroundColor = obj.GetComponentInParent<Image>()?.color ?? Color.white;
            if (textMesh != null)
            {
            
                textPropertiesList.Add(new TextProperties(textMesh.text, textMesh.fontSize, textMesh.font.name, textColor, backgroundColor, CalculateContrastRatio(textColor, backgroundColor)));
            }

            // input field
            TMP_InputField inputField = obj.GetComponent<TMP_InputField>();
            textColor = inputField.textComponent.color;
            backgroundColor = obj.GetComponentInParent<Image>()?.color ?? Color.white;

            if (inputField != null)
            {
                textPropertiesList.Add(new TextProperties(inputField.text, inputField.textComponent.fontSize, inputField.textComponent.font.name, textColor, backgroundColor, CalculateContrastRatio(textColor, backgroundColor)));
            }

            // ui Text
            Text uiText = obj.GetComponent<Text>();
            textColor = uiText.color;
            backgroundColor = obj.GetComponentInParent<Image>()?.color ?? Color.white;

            if (uiText != null)
            {
                textPropertiesList.Add(new TextProperties(uiText.text, uiText.fontSize, uiText.font.name, textColor, backgroundColor, CalculateContrastRatio(textColor, backgroundColor)));
            }
        }
    }
    private void CheckTextProperties()
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
                textExist = true;
            }
        }

        foreach (TMP_InputField inputField in inputFields)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(inputField.GetComponent<RectTransform>(), mainCamera.WorldToScreenPoint(inputField.transform.position), mainCamera))
            {
                visibleObjects.Add(inputField.gameObject);
                textExist = true;
            }
        }

        foreach (Text uiText in uiTexts)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(uiText.GetComponent<RectTransform>(), mainCamera.WorldToScreenPoint(uiText.transform.position), mainCamera))
            {
                visibleObjects.Add(uiText.gameObject);
                textExist = true;
            }
        }

        DisplayTextProperties(visibleObjects);
    }

    private void DisplayTextProperties(List<GameObject> visibleObjects)
    {
        textPropertiesList.Clear(); // Clear the list before adding new entries
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

            // Get the text color and background color
            Color textColor = textMesh.color;
            Color backgroundColor = obj.GetComponentInParent<Image>()?.color ?? Color.white; // Assuming the text is on a UI image background
            

            // Validate color contrast
            ValidateColorContrast(textColor, backgroundColor, textMesh.fontSize);


            // Add text properties to the list
            //textPropertiesList.Add(new TextProperties(textMesh.text, textMesh.fontSize, textMesh.font.name, textColor, backgroundColor, CalculateContrastRatio(textColor, backgroundColor)));
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


            Color textColor = inputField.textComponent.color;
            Color backgroundColor = obj.GetComponentInParent<Image>()?.color ?? Color.white;

            ValidateColorContrast(textColor, backgroundColor, inputField.textComponent.fontSize);

            // Add input field properties to the list
            //textPropertiesList.Add(new TextProperties(inputField.text, inputField.textComponent.fontSize, inputField.textComponent.font.name, textColor, backgroundColor, CalculateContrastRatio(textColor, backgroundColor)));
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

            Color textColor = uiText.color;
            Color backgroundColor = obj.GetComponentInParent<Image>()?.color ?? Color.white;

            ValidateColorContrast(textColor, backgroundColor, uiText.fontSize);

            // Add UI Text properties to the list
            //textPropertiesList.Add(new TextProperties(uiText.text, uiText.fontSize, uiText.font.name, textColor, backgroundColor, CalculateContrastRatio(textColor, backgroundColor)));
        }
    }

    private bool IsMultiline(string text)
    {
        return text.Contains("\n");
    }

    private void ValidateFontSize(float fontSize)
    {
        bool isPass = fontSize >= 16; // Assuming desktop standard for simplicity

        fontsizePass = fontSize >= 16;
        fontsize = fontSize;

        string suggestion = isPass ? "Pass" : "Increase font size to at least 16.";

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("    Font Size Check:", wcagStyle, GUILayout.Width(200), GUILayout.Height(20));
        GUIStyle style = isPass ? passStyle : failStyle;
        string resultText = isPass ? "Pass" : "Fail";
        EditorGUILayout.LabelField(resultText, style, GUILayout.Width(50), GUILayout.Height(20));
        EditorGUILayout.EndHorizontal();

        if (!isPass)
        {
            EditorGUILayout.LabelField("          Suggestion:    " + suggestion, cyanStyle, GUILayout.Width(550), GUILayout.Height(30));
        }
    }

    private void ValidateFont(string fontName)
    {
        bool isPass = recommendedFonts.Contains(fontName);

        fontPass = recommendedFonts.Contains(fontName);
        fontstyle = fontName;

        string suggestion = isPass ? "Pass" : "Consider using a more readable and accessible font like Arial, Verdana, or Roboto.";

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("    Font Check:", wcagStyle, GUILayout.Width(200), GUILayout.Height(20));
        GUIStyle style = isPass ? passStyle : failStyle;
        string resultText = isPass ? "Pass" : "Fail";
        EditorGUILayout.LabelField(resultText, style, GUILayout.Width(50), GUILayout.Height(20));
        EditorGUILayout.EndHorizontal();

        if (!isPass)
        {
            EditorGUILayout.LabelField("          Suggestion:    " + suggestion, cyanStyle, GUILayout.Width(550), GUILayout.Height(30));
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
            EditorGUILayout.LabelField("          Suggestion:    " + caseSuggestion, cyanStyle, GUILayout.Width(550), GUILayout.Height(30));
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("    Line Spacing Check:", wcagStyle, GUILayout.Width(200), GUILayout.Height(20));
        style = isLineSpacingCorrect ? passStyle : failStyle;
        resultText = isLineSpacingCorrect ? "Pass" : "Fail";
        EditorGUILayout.LabelField(resultText, style, GUILayout.Width(50), GUILayout.Height(18));
        EditorGUILayout.EndHorizontal();
        if (!isLineSpacingCorrect)
        {
            EditorGUILayout.LabelField("          Suggestion:    " + lineSpacingSuggestion, cyanStyle, GUILayout.Width(550), GUILayout.Height(30));
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("    Characters Per Line Check:", wcagStyle, GUILayout.Width(200), GUILayout.Height(20));
        style = isCharactersPerLineCorrect ? passStyle : failStyle;
        resultText = isCharactersPerLineCorrect ? "Pass" : "Fail";
        EditorGUILayout.LabelField(resultText, style, GUILayout.Width(50), GUILayout.Height(20));
        EditorGUILayout.EndHorizontal();
        if (!isCharactersPerLineCorrect)
        {
            EditorGUILayout.LabelField("          Suggestion:    " + charPerLineSuggestion, cyanStyle, GUILayout.Width(550), GUILayout.Height(30));
     
        }
    }

    private void ValidateColorContrast(Color textColor, Color backgroundColor, float fontSize)
    {
        float contrastRatio = CalculateContrastRatio(textColor, backgroundColor);

        // Check for WCAG AA
        bool aaPass = fontSize >= 18 ? contrastRatio >= 3.0f : contrastRatio >= 4.5f;
        if (aaPass) { wcagaa = aaPass; }
        // Check for WcAG AAA
        bool aaaPass = fontSize >= 18 ? contrastRatio >= 4.5f : contrastRatio >= 7.0f;
        if (aaaPass) { wcagaaa = aaaPass; }
        GUILayout.Space(10);

        // Display the text and background color
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("    Text Color:", wcagStyle, GUILayout.Width(100), GUILayout.Height(20));
        textColor = EditorGUILayout.ColorField(new GUIContent(), textColor, false, false, false, GUILayout.Width(120), GUILayout.Height(18));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("    Background Color:", wcagStyle, GUILayout.Width(150), GUILayout.Height(20));
        backgroundColor = EditorGUILayout.ColorField(new GUIContent(), backgroundColor, false, false, false, GUILayout.Width(120), GUILayout.Height(18));
        EditorGUILayout.EndHorizontal();

        string aaSuggestion = aaPass ? "Pass" : $"Fail - Contrast ratio is {contrastRatio:0.00}. Minimum is 4.5 for normal text and 3.0 for large text.";
        string aaaSuggestion = aaaPass ? "Pass" : $"Fail - Contrast ratio is {contrastRatio:0.00}. Minimum is 7.0 for normal text and 4.5 for large text.";

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("    Contrast Ratio Check (AA):", wcagStyle, GUILayout.Width(250), GUILayout.Height(20));
        GUIStyle styleAA = aaPass ? passStyle : failStyle;
        EditorGUILayout.LabelField(aaSuggestion, styleAA, GUILayout.Width(275), GUILayout.Height(20));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("    Contrast Ratio Check (AAA):", wcagStyle, GUILayout.Width(250), GUILayout.Height(20));
        GUIStyle styleAAA = aaaPass ? passStyle : failStyle;
        EditorGUILayout.LabelField(aaaSuggestion, styleAAA, GUILayout.Width(275), GUILayout.Height(20));
        EditorGUILayout.EndHorizontal();
    }

    private float CalculateLuminance(Color color)
    {
        float R = color.r <= 0.03928f ? color.r / 12.92f : Mathf.Pow((color.r + 0.055f) / 1.055f, 2.4f);
        float G = color.g <= 0.03928f ? color.g / 12.92f : Mathf.Pow((color.g + 0.055f) / 1.055f, 2.4f);
        float B = color.b <= 0.03928f ? color.b / 12.92f : Mathf.Pow((color.b + 0.055f) / 1.055f, 2.4f);
        return 0.2126f * R + 0.7152f * G + 0.0722f * B;
    }

    private float CalculateContrastRatio(Color textColor, Color backgroundColor)
    {
        float textLuminance = CalculateLuminance(textColor) + 0.05f;
        float backgroundLuminance = CalculateLuminance(backgroundColor) + 0.05f;

        return textLuminance > backgroundLuminance
            ? textLuminance / backgroundLuminance
            : backgroundLuminance / textLuminance;
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
