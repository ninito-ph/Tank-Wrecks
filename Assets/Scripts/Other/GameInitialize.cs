using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GameInitialize : MonoBehaviour
{
    [Header("Core Values")]
    [Tooltip("The main audio mixer of the game")]
    public AudioMixer mainMixer;

    // Runs once before start
    void Awake()
    {
        CreatePreferences();
        LoadPreferences();
    }

    // Creates preferences for the first time if they don't already exist
    private void CreatePreferences()
    {
        // If the startup key does not exist
        if (!PlayerPrefs.HasKey("First Startup"))
        {
            // Declare the first startup key as true
            PlayerPrefs.SetString("First Startup", "true");

            // Declare the volume keys
            PlayerPrefs.SetFloat("Master Volume", 0);
            PlayerPrefs.SetFloat("Sound Effects Volume", 0);
            PlayerPrefs.SetFloat("Music Volume", 0);

            // Declares the fullscreen and resolution key
            PlayerPrefs.SetString("Fullscreen", "true");
            PlayerPrefs.SetInt("Resolution Width", Screen.currentResolution.width);
            PlayerPrefs.SetInt("Resolution Height", Screen.currentResolution.height);

            // Declares the graphical settings
            PlayerPrefs.SetInt("Graphical Settings", 0);

            // Saves PlayerPrefs
            PlayerPrefs.Save();
        }
    }

    // Loads saved preferences
    private void LoadPreferences()
    {
        // Sets volume to saved volume values
        // Clamps volume values for safety
        mainMixer.SetFloat("masterVolume", Mathf.Clamp(PlayerPrefs.GetFloat("Master Volume"), -80f, 0f));
        mainMixer.SetFloat("masterVolume", Mathf.Clamp(PlayerPrefs.GetFloat("Sound Effects Volume"), -80f, 0f));
        mainMixer.SetFloat("masterVolume", Mathf.Clamp(PlayerPrefs.GetFloat("Music Volume"), -80f, 0f));
        // Sets the screen resolution
        Screen.SetResolution(PlayerPrefs.GetInt("Resolution Width"), PlayerPrefs.GetInt("Resolution Height"), FullScreenMode.FullScreenWindow);
        // Sets to fullscreen or windowed
        bool isFullscreen = true;
        if (PlayerPrefs.GetString("Fullscreen") == "false")
        {
            isFullscreen = false;
        }
        Screen.fullScreen = isFullscreen;
        // Sets the graphical quality
        QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("Graphical Settings"));
    }
}