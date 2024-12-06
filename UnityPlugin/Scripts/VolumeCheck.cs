using UnityEngine;
using UnityEngine.Audio;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class VolumeProperties
{
    public int masterVolume;
    public int voiceVolume;
    public int musicVolume;
    public int sfxVolume;

    // assign {1 - too quiet, 2 - too loud, 3 - safe}
    public int masterPass;
    public int voicePass;
    public int musicPass;
    public int sfxPass;

    public string masterSuggestion;
    public string voiceSuggestion;
    public string musicSuggestion;
    public string sfxSuggestion;

    public VolumeProperties(int masterVolume, int voiceVolume, int musicVolume, int sfxVolume,
                            int masterPass, int voicePass, int musicPass, int sfxPass,
                            string masterSuggestion, string voiceSuggestion, string musicSuggestion, string sfxSuggestion)
    {
        this.masterVolume = masterVolume;
        this.voiceVolume = voiceVolume;
        this.musicVolume = musicVolume;
        this.sfxVolume = sfxVolume;
        this.masterPass = masterPass;
        this.voicePass = voicePass;
        this.musicPass = musicPass;
        this.sfxPass = sfxPass;
        this.masterSuggestion = masterSuggestion;
        this.voiceSuggestion = voiceSuggestion;
        this.musicSuggestion = musicSuggestion;
        this.sfxSuggestion = sfxSuggestion;
    }

}
public class VolumeChecker
{
    public AudioMixer audioMixer;

    // Loudness thresholds based on recommendations
    private const float masterMinLevel = -12f;
    private const float masterMaxLevel = -6f;
    private const float voiceMinLevel = -12f;
    private const float voiceMaxLevel = -6f;
    private const float musicMinLevel = -20f;
    private const float musicMaxLevel = -12f;
    private const float sfxMinLevel = -12f;
    private const float sfxMaxLevel = -3f;

    // assign {0 - no sound found, 1 - too quiet, 2 - too loud, 3 - safe}
    private int masterPass;
    private int voicePass;
    private int musicPass;
    private int sfxPass;

    private string masterSuggestion;
    private string voiceSuggestion;
    private string musicSuggestion;
    private string sfxSuggestion;

    private GUIStyle failStyle;
    private GUIStyle passStyle;
    private GUIStyle cyanStyle;
    private GUIStyle header1;
    private GUIStyle header2;
    private GUIStyle normal;
    private GUIStyle boldnormal;
    private GUIStyle boldnormal1;
    private GUIStyle noticeStyle;

    public VolumeProperties volume;

    public void OnEnable()
    {

        AudioMixer[] mixers = Resources.FindObjectsOfTypeAll<AudioMixer>();
        foreach (var mixer in mixers)
        {
            if (mixer.name == "MainMixer")
            {
                audioMixer = mixer;
                break;
            }
        }

        if (audioMixer == null)
        {
            Debug.LogError("Main Mixer not found. Please ensure it's named correctly.");
        }

        CheckVolumeLevels();

    }

    private Vector2 scrollPosition = Vector2.zero;
    public void OnGUI()
    {
        // Start the scroll view
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
        InitializeGUIStyles();

        // Title
        EditorGUILayout.LabelField("Volume Checker", header1);

        EditorGUILayout.Space(10);

        // Instruction on allowing this feature to work properly
        EditorGUILayout.LabelField("<b>Important Notice!!</b>", noticeStyle);
        EditorGUILayout.LabelField("   Ensure there is an AudioMixer object named <b>'MainMixer'</b> that includes volume controls for Master, Music, Voice, and SFX.", noticeStyle);
        EditorGUILayout.Space(10);

        // Informational Section about dB and importance
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("What is dB (Decibels)?", header2);
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("   - Decibels (dB)", boldnormal, GUILayout.ExpandWidth(false));
        EditorGUILayout.LabelField("are a logarithmic unit used to measure sound intensity.", normal);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("   - A lower dB", boldnormal, GUILayout.ExpandWidth(false));
        GUILayout.Label("level means quieter sounds while", normal, GUILayout.ExpandWidth(false));
        GUILayout.Label("higher dB", boldnormal, GUILayout.ExpandWidth(false));
        GUILayout.Label("levels represent louder sounds.", normal, GUILayout.ExpandWidth(false));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("   - It is crucial to", normal, GUILayout.ExpandWidth(false));
        GUILayout.Label("balance audio levels", boldnormal, GUILayout.ExpandWidth(false));
        GUILayout.Label("to avoid", normal, GUILayout.ExpandWidth(false));
        GUILayout.Label("distortion listener fatigue, or inaudible elements", boldnormal, GUILayout.ExpandWidth(false));
        GUILayout.Label("in your game or application.", normal, GUILayout.ExpandWidth(false));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        GUILayout.Space(10);

        EditorGUILayout.LabelField("Why is Volume Monitoring Important?", header2);
        EditorGUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label("   - Proper volume levels ensure", normal, GUILayout.ExpandWidth(false));
        GUILayout.Label("a consistent and enjoyable audio experience", boldnormal, GUILayout.ExpandWidth(false));
        GUILayout.Label("for players.", normal, GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("   - Maintaining recommended loudness levels",normal, GUILayout.ExpandWidth(false));
        GUILayout.Label("prevents clipping (audio distortion due to high volume)", boldnormal, GUILayout.ExpandWidth(false));
        GUILayout.Label("and", normal, GUILayout.ExpandWidth(false));
        GUILayout.Label("ensures clarity for speech, music and sound effects.", boldnormal, GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("   - Balanced audio contributes to", normal, GUILayout.ExpandWidth(false));
        GUILayout.Label("better immersion and accessibility", boldnormal, GUILayout.ExpandWidth(false));
        GUILayout.Label("in your game.", normal, GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();


        // Spacing for better readability
        EditorGUILayout.Space(20);

        // Recommended levels section
        EditorGUILayout.LabelField("Recommended Loudness Levels", header2);
        GUILayout.Space(5);

        // Adjusted spacing using GUILayoutOption
        RenderLabeledVolumeRow("Master Volume:", "-12 dB to -6 dB");
        RenderLabeledVolumeRow("Voice Volume:", "-12 dB to -6 dB");
        RenderLabeledVolumeRow("Music Volume:", "-20 dB to -12 dB");
        RenderLabeledVolumeRow("SFX Volume:", "-12 dB to -3 dB");

        EditorGUILayout.Space(20);

        // Displaying volume properties in Main Mixer
        EditorGUILayout.LabelField("Volume Check (MainMixer)", header2);
        GUILayout.Space(10);

        // Displaying volume levels with conditional styling
        RenderVolumeRow("Master Volume", volume.masterVolume, volume.masterPass == 3, volume.masterSuggestion);
        RenderVolumeRow("Voice Volume", volume.voiceVolume, volume.voicePass == 3, volume.voiceSuggestion);
        RenderVolumeRow("Music Volume", volume.musicVolume, volume.musicPass == 3, volume.musicSuggestion);
        RenderVolumeRow("SFX Volume", volume.sfxVolume, volume.sfxPass == 3, volume.sfxSuggestion);
        EditorGUILayout.Space(20);

        // End the scroll view
        GUILayout.EndScrollView();
    }


    // Helper method for rendering rows with aligned labels and values
    private void RenderLabeledVolumeRow(string label, string value)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(30); // Indent for better alignment
        EditorGUILayout.LabelField(label, boldnormal, GUILayout.Width(150)); // Fixed width for the label
        EditorGUILayout.LabelField(value, normal); // Value adjusts dynamically
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(5);

    }


    private void RenderVolumeRow(string label, float volumeLevel, bool isPass, string suggestion)
    {


        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(30);
        EditorGUILayout.LabelField($"{label}: ", boldnormal, GUILayout.Width(150));
        GUIStyle style = isPass ? passStyle : failStyle;
        EditorGUILayout.LabelField($"{volumeLevel} dB", style);
        EditorGUILayout.EndHorizontal();
        if (!isPass)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(50); // Indent the suggestion for alignment
            EditorGUILayout.LabelField(suggestion, cyanStyle);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space(5);
    }

    private void CheckVolumeLevels()
    {
        int masterVolume = Mathf.RoundToInt(GetAudioMixerVolume("MasterVolume"));
        int voiceVolume = Mathf.RoundToInt(GetAudioMixerVolume("VoiceVolume"));
        int musicVolume = Mathf.RoundToInt(GetAudioMixerVolume("MusicVolume"));
        int sfxVolume = Mathf.RoundToInt(GetAudioMixerVolume("SFXVolume"));


        masterPass = GetLoudnessVerifyNum(masterVolume, Mathf.RoundToInt(masterMinLevel), Mathf.RoundToInt(masterMaxLevel));
        voicePass = GetLoudnessVerifyNum(voiceVolume, Mathf.RoundToInt(voiceMinLevel), Mathf.RoundToInt(voiceMaxLevel));
        musicPass = GetLoudnessVerifyNum(musicVolume, Mathf.RoundToInt(musicMinLevel), Mathf.RoundToInt(musicMaxLevel));
        sfxPass = GetLoudnessVerifyNum(sfxVolume, Mathf.RoundToInt(sfxMinLevel), Mathf.RoundToInt(sfxMaxLevel));

        masterSuggestion = GetSuggestion("Master", masterPass);
        voiceSuggestion = GetSuggestion("Voice", voicePass);
        musicSuggestion = GetSuggestion("Music", musicPass);
        sfxSuggestion = GetSuggestion("Voice", sfxPass);

        volume = new VolumeProperties(masterVolume, voiceVolume, musicVolume, sfxVolume,
                                                      masterPass, voicePass, musicPass, sfxPass,
                                                      masterSuggestion, voiceSuggestion, musicSuggestion, sfxSuggestion);

        Debug.Log($"mastervolume:{masterVolume}\n" +
            $"voicevolume: {voiceVolume}\n" +
            $"musicvolume: {musicVolume}\n" +
            $"sfxvolume: {sfxVolume}\n" +
            $"masterPass: {masterPass}\n" +
            $"voicePass: {voicePass}\n" +
            $"musicPass: {musicPass}\n" +
            $"minLevelforMusic: {Mathf.RoundToInt(musicMinLevel)}\n" +
            $"maxLevelforMusic: {Mathf.RoundToInt(musicMaxLevel)}\n" +
            $"sfxPass: {sfxPass}\n" +
            $"masterSuggestion: {masterSuggestion}\n" +
            $"voieSugestion: {voiceSuggestion}\n" +
            $"musicSuggestion: {musicSuggestion}\n" +
            $"sfxSuggestion: {sfxSuggestion}\n");
    }
    private string GetSuggestion(string type, int verifynum)
    {
        if (verifynum == 1)
        {
            return $"{type} volume is too quiet! Consider raising the volume.";
        }
        else if (verifynum == 2)
        {
            return $"{type} volume is too loud! Consider lowering the volume.";
        }
        else
        {
            return $"{type} volume is at a safe level.";
        }
    }
    private int GetLoudnessVerifyNum(int volume, int minLevel, int maxLevel)
    {
        if (volume < minLevel)
        {
            return 1;
        }
        else if (volume > maxLevel)
        {
            return 2;
        }
        else
        {
            return 3;
        }
    }
    private float GetAudioMixerVolume(string exposedParameter)
    {
        float value;
        if (audioMixer.GetFloat(exposedParameter, out value))
        {
            return value;
        }
        else
        {
            Debug.LogWarning($"Parameter {exposedParameter} not found in AudioMixer.");
            return -80f; // Assuming -80 dB as the minimum volume level
        }
    }

    private void InitializeGUIStyles()
    {
        failStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            wordWrap = true,
            normal = { textColor = Color.red },
        };
        passStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            wordWrap = true,
            normal = { textColor = Color.green },
        };
        cyanStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            wordWrap = false,
            normal = { textColor = Color.cyan },
        };
        header1 = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 18,
            wordWrap = true,
        };
        header2 = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            wordWrap = true
        };
        normal = new GUIStyle(EditorStyles.label)
        {
            fontSize = 15
        };
        boldnormal = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 15,
            wordWrap  = false,
            richText = true
        };
        noticeStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            normal = { textColor = Color.yellow },
            hover = { textColor = Color.yellow },
            fontStyle = FontStyle.Italic,
            richText = true 
        };

    }
}