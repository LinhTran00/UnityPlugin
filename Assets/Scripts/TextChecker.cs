using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class TextChecker
{
    private GUIStyle wcagStyle;
    private GUIStyle passStyle;
    private GUIStyle failStyle;
    private GUIStyle headerStyle;
    private Camera mainCamera;
    private Vector2 scrollPosition;

    public void OnEnable()
    {
        mainCamera = Camera.main;
    }

    public void OnGUI()
    {

        EditorGUILayout.BeginHorizontal();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.BeginVertical(GUILayout.Width(250));
        TryInitStyles();
        EditorGUILayout.LabelField("Text Size Check", headerStyle, GUILayout.Height(24));
        

        CheckTextSizes(Camera.main);

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();
    }

    private void CheckTextSizes(Camera mainCamera)
    {
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera is not assigned.");
            return;
        }

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        List<GameObject> visibleObjects = new List<GameObject>();

        // Find all TMP_Text, TMP_InputField, Text, and Button components in the scene
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

        DisplayTextSizes(visibleObjects);
    }

    private void DisplayTextSizes(List<GameObject> visibleObjects)
    {
        DeviceType deviceType = GetDeviceType();

        foreach (GameObject obj in visibleObjects)
        {
            DisplayTextComponentInfo(obj, deviceType);
        }
    }

    private void DisplayTextComponentInfo(GameObject obj, DeviceType deviceType)
    {
        TMP_Text textMesh = obj.GetComponent<TMP_Text>();
        if (textMesh != null)
        {
            EditorGUILayout.LabelField($"Object: {obj.name}", wcagStyle, GUILayout.Height(24));
            EditorGUILayout.LabelField($"    Text: {textMesh.text}", wcagStyle, GUILayout.Height(24));
            EditorGUILayout.LabelField($"    Font: {textMesh.font.name}, Size: {textMesh.fontSize}", wcagStyle, GUILayout.Height(24));
            ValidateFontSize(textMesh.fontSize, deviceType);
        }

        TMP_InputField inputField = obj.GetComponent<TMP_InputField>();
        if (inputField != null)
        {
            EditorGUILayout.LabelField($"Object: {obj.name}");
            EditorGUILayout.LabelField($"    InputField Text: {inputField.text}", wcagStyle, GUILayout.Height(24));
            EditorGUILayout.LabelField($"    Font: {inputField.textComponent.font.name}, Size: {inputField.textComponent.fontSize}", wcagStyle, GUILayout.Height(24));
            ValidateFontSize(inputField.textComponent.fontSize, deviceType);
        }

        TMP_Dropdown dropdown = obj.GetComponent<TMP_Dropdown>();
        if (dropdown != null)
        {
            EditorGUILayout.LabelField($"Object: {obj.name}", wcagStyle, GUILayout.Height(24));
            EditorGUILayout.LabelField($"    Dropdown Value: {dropdown.options[dropdown.value].text}", wcagStyle, GUILayout.Height(24));
        }

        Text uiText = obj.GetComponent<Text>();
        if (uiText != null)
        {
            EditorGUILayout.LabelField($"Object: {obj.name}", wcagStyle, GUILayout.Height(24));
            EditorGUILayout.LabelField($"    UI Text: {uiText.text}", wcagStyle, GUILayout.Height(24));
            EditorGUILayout.LabelField($"    Font: {uiText.font.name}, Size: {uiText.fontSize}", wcagStyle, GUILayout.Height(24));
            ValidateFontSize(uiText.fontSize, deviceType);
        }
    }

    private void ValidateFontSize(float fontSize, DeviceType deviceType)
    {
        bool isPass = false;
        string suggestion = string.Empty;

        switch (deviceType)
        {
            case DeviceType.Mobile:
                isPass = fontSize >= 12;
                suggestion = isPass ? "Pass" : "Increase font size to at least 12.";
                break;
            case DeviceType.Tablet:
                isPass = fontSize >= 15;
                suggestion = isPass ? "Pass" : "Increase font size to at least 15.";
                break;
            case DeviceType.Desktop:
                isPass = fontSize >= 16;
                suggestion = isPass ? "Pass" : "Increase font size to at least 16.";
                break;
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("    Font Size Check:", wcagStyle, GUILayout.Width(130), GUILayout.Height(18));
        GUIStyle style = isPass ? passStyle : failStyle;
        string resultText = isPass ? "Pass" : "Fail";
        EditorGUILayout.LabelField(resultText, style, GUILayout.Width(50), GUILayout.Height(18));
        EditorGUILayout.EndHorizontal();

        if (!isPass)
        {
            EditorGUILayout.LabelField('*' + suggestion, wcagStyle, GUILayout.Width(500), GUILayout.Height(30));
        }
    }

    private DeviceType GetDeviceType()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        if (screenWidth <= 800 && screenHeight <= 1280)
            return DeviceType.Mobile;
        else if (screenWidth <= 1280 && screenHeight <= 1920)
            return DeviceType.Tablet;
        else
            return DeviceType.Desktop;
    }

    private enum DeviceType
    {
        Mobile,
        Tablet,
        Desktop
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
            headerStyle.alignment = TextAnchor.MiddleLeft;
            headerStyle.fontSize = 18;
        }
    }
}
