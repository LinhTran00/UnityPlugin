using UnityEngine;
using UnityEngine.Audio;
using UnityEditor;
using System.Collections.Generic;

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
    private string result;

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
    int masterPass = 0;
    int voicePass = 0;
    int musicPass = 0;
    int sfxPass = 0;

    string masterSuggestion;
    string voiceSuggestion;
    string musicSuggestion;
    string sfxSuggestion;


    public List<VolumeProperties> volumePropertiesList = new List<VolumeProperties>();  

    public void OnEnable ()
    {
        CheckVolumeLevels();
    }
    public void OnGUI()
    {
        EditorGUILayout.LabelField("Volume Checker", EditorStyles.boldLabel);
        audioMixer = (AudioMixer)EditorGUILayout.ObjectField("Audio Mixer", audioMixer, typeof(AudioMixer), false);

        // Display loudness instructions
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Recommended Loudness Levels:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Master Volume: -12 dB to -6 dB");
        EditorGUILayout.LabelField("Voice Volume: -12 dB to -6 dB");
        EditorGUILayout.LabelField("Music Volume: -20 dB to -12 dB");
        EditorGUILayout.LabelField("SFX Volume: -12 dB to -3 dB");
        EditorGUILayout.Space();

        if (GUILayout.Button("Check Volume Levels"))
        {
            CheckVolumeLevels();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Results:", EditorStyles.boldLabel);
        EditorGUILayout.TextArea(result, GUILayout.Height(200));
    }

    private void CheckVolumeLevels()
    {
        if (audioMixer == null)
        {
            result = "Please assign an Audio Mixer.";
            return;
        }

        int masterVolume = Mathf.RoundToInt(GetAudioMixerVolume("MasterVolume"));
        int voiceVolume = Mathf.RoundToInt(GetAudioMixerVolume("VoiceVolume"));
        int musicVolume = Mathf.RoundToInt(GetAudioMixerVolume("MusicVolume"));
        int sfxVolume = Mathf.RoundToInt(GetAudioMixerVolume("SFXVolume"));

        result = $"Master Volume: {masterVolume} dB\n" +
                 $"Voice Volume: {voiceVolume} dB\n" +
                 $"Music Volume: {musicVolume} dB\n" +
                 $"SFX Volume: {sfxVolume} dB\n\n";

        result += GetLoudnessResult(masterVolume, "Master", Mathf.RoundToInt(masterMinLevel), Mathf.RoundToInt(masterMaxLevel));
        result += GetLoudnessResult(voiceVolume, "Voice", Mathf.RoundToInt(voiceMinLevel), Mathf.RoundToInt(voiceMaxLevel));
        result += GetLoudnessResult(musicVolume, "Music", Mathf.RoundToInt(musicMinLevel), Mathf.RoundToInt(musicMaxLevel));
        result += GetLoudnessResult(sfxVolume, "SFX", Mathf.RoundToInt(sfxMinLevel), Mathf.RoundToInt(sfxMaxLevel));

        masterSuggestion = GetLoudnessResult(masterVolume, "Master", Mathf.RoundToInt(masterMinLevel), Mathf.RoundToInt(masterMaxLevel));
        voiceSuggestion = GetLoudnessResult(voiceVolume, "Voice", Mathf.RoundToInt(voiceMinLevel), Mathf.RoundToInt(voiceMaxLevel));
        musicSuggestion = GetLoudnessResult(musicVolume, "Music", Mathf.RoundToInt(musicMinLevel), Mathf.RoundToInt(musicMaxLevel));
        sfxSuggestion = GetLoudnessResult(sfxVolume, "SFX", Mathf.RoundToInt(sfxMinLevel), Mathf.RoundToInt(sfxMaxLevel));

        volumePropertiesList.Add(new VolumeProperties(masterVolume, voiceVolume, musicVolume, sfxVolume, 
                                                      masterPass, voicePass, musicPass, sfxPass,
                                                      masterSuggestion, voiceSuggestion, musicSuggestion, sfxSuggestion));
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

    private string GetLoudnessResult(int volume, string type, int minLevel, int maxLevel)
    {
        if (volume < minLevel)
        {
            masterPass = 1;
            voicePass = 1;
            musicPass = 1;
            sfxPass = 1;
            return $"{type} volume is too quiet! Consider raising the volume.\n";
        }
        else if (volume > maxLevel)
        {
            masterPass = 2;
            voicePass = 2;
            musicPass = 2;
            sfxPass = 2;
            return $"{type} volume is too loud! Consider lowering the volume.\n";
        }
        else
        {
            masterPass = 3;
            voicePass = 3;
            musicPass = 3;
            sfxPass = 3;
            return $"{type} volume is at a safe level.\n";
        }
    }
}