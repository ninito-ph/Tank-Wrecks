using System;
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

    // Gets the system time and c
    private int GetUnixDay()
    {
        // Gets today's date
        DateTime today = DateTime.Now;
        // Gets the elapsed unix time of the date
        TimeSpan unixElapsedTime = new TimeSpan(today.Ticks);
        // Converts it to days and returns it
        return unixElapsedTime.Days;
    }

    // Creates preferences for the first time if they don't already exist
    private void CreatePreferences()
    {
        // If the startup key does not exist
        if (!PlayerPrefs.HasKey("First Startup"))
        {
            // Declare the first startup key as true
            PlayerPrefs.SetString("First Startup", "true");

            // Save the date at which the game was entered
            PlayerPrefs.SetInt("LastLoginDate", GetUnixDay());

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
        // Check if it has been more than 15 days since the last time the game was opened
        if (GetUnixDay() > PlayerPrefs.GetInt("LastLoginDate", GetUnixDay()) + 15)
        {
            // Mark the game as first startup, so that help shows again
            PlayerPrefs.SetString("First Startup", "true");
        }
        else
        {
            // Declare the first startup key as false, so that help doesn't annoy the player
            PlayerPrefs.SetString("First Startup", "false");
        }

        // Save the date at which the game was entered
        PlayerPrefs.SetInt("LastLoginDate", GetUnixDay());

        // Sets volume to saved volume values
        // Clamps volume values for safety
        mainMixer.SetFloat("masterVolume", Mathf.Log10(PlayerPrefs.GetFloat("Master Volume")) * 20);
        mainMixer.SetFloat("soundEffectsVolume", Mathf.Log10(PlayerPrefs.GetFloat("Sound Effects Volume")) * 20);
        mainMixer.SetFloat("musicVolume", Mathf.Log10(PlayerPrefs.GetFloat("Music Volume")) * 20);

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