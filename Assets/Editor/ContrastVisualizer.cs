using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using Unity.VisualScripting;

public class ContrastToolWindow : EditorWindow
{
    private bool ToolActive = false;
    private float InEditorOpacity = 1f;

    private Shader ContrastShader;
    [SerializeField] private Camera mainCamera;

    [MenuItem("Window/Contrast Analysis")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ContrastToolWindow));
        
    }

    private void OnGUI()
    {
        GUILayout.Label("Contrast Analysis Tool", EditorStyles.boldLabel);

        mainCamera = EditorGUILayout.ObjectField("Main Camera", mainCamera, typeof(Camera), true) as Camera;

        // On Off Toggle
        if ( GUILayout.Button("Toggle Contrast Visualization"))
        {
            ToolActive = !ToolActive;

            if (ToolActive) EnableTool();
            else DisableTool();
        }

        // place all other UI elements in this section
        EditorGUI.BeginDisabledGroup(ToolActive);
        GUILayout.BeginHorizontal();

        GUILayout.Label("Shader Opacity");
        InEditorOpacity = GUILayout.HorizontalSlider(InEditorOpacity, 0f, 1f);

        GUILayout.EndHorizontal();
        EditorGUI.EndDisabledGroup();
    }

    private void EnableTool()
    {

    }

    private void DisableTool()
    {

    }
}