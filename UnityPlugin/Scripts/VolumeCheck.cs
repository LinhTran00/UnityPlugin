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
    public void OnGUI()
    {
        InitializeGUIStyles();
        // Begin vertical layout with a fixed width for better UI organization
        EditorGUILayout.BeginVertical(GUILayout.Width(400));

        // Title
        EditorGUILayout.LabelField("Volume Checker", EditorStyles.boldLabel);

        // Spacing for better readability
        EditorGUILayout.Space(10);

        // Recommended levels section
        EditorGUILayout.LabelField("Recommended Loudness Levels:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Master Volume: -12 dB to -6 dB", GUILayout.Height(20));
        EditorGUILayout.LabelField("Voice Volume: -12 dB to -6 dB", GUILayout.Height(20));
        EditorGUILayout.LabelField("Music Volume: -20 dB to -12 dB", GUILayout.Height(20));
        EditorGUILayout.LabelField("SFX Volume: -12 dB to -3 dB", GUILayout.Height(20));

        EditorGUILayout.Space(10);

        // Displaying volume properties in Main Mixer
        EditorGUILayout.LabelField("In Main Mixer:", EditorStyles.boldLabel, GUILayout.Height(30));

        // Displaying volume levels with conditional styling
        RenderVolumeRow("Master Volume", volume.masterVolume, volume.masterPass == 3, volume.masterSuggestion);
        RenderVolumeRow("Voice Volume", volume.voiceVolume, volume.voicePass == 3, volume.voiceSuggestion);
        RenderVolumeRow("Music Volume", volume.musicVolume, volume.musicPass == 3, volume.musicSuggestion);
        RenderVolumeRow("SFX Volume", volume.sfxVolume, volume.sfxPass == 3, volume.sfxSuggestion);


        // Add some space between each volume entry for clarity
        EditorGUILayout.Space(5);


        // End of vertical layout
        EditorGUILayout.EndVertical();
    }

    private void RenderVolumeRow(string label, float volumeLevel, bool isPass, string suggestion)
    {


        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(100));
        GUIStyle style = isPass ? passStyle : failStyle;
        EditorGUILayout.LabelField($"{volumeLevel} dB", style);
        EditorGUILayout.EndHorizontal();
        if (!isPass)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(50); // Indent the suggestion for alignment
            EditorGUILayout.LabelField(suggestion, cyanStyle, GUILayout.Width(500));
            EditorGUILayout.EndHorizontal();
        }
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
            fontSize = 12,
            wordWrap = true,
            normal = { textColor = Color.red },
        };
        passStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 12,
            wordWrap = true,
            normal = { textColor = Color.green },
        };
        cyanStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 12,
            wordWrap = false,
            normal = { textColor = Color.cyan },
        };
    }
}