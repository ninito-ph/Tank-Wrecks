using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenuController : MenuBase
{
    #region Core Values

    [Header("Internal")]
    [Tooltip("The main audio mixer of the game")]
    [SerializeField]
    private AudioMixer mainMixer;
    // The array containing screen resolutions
    private Resolution[] resolutions;
    [Header("UI Objects")]
    [Tooltip("The toggle for enabling and disabling fullscreen")]
    [SerializeField]
    private Toggle fullscreenToggle;
    [Tooltip("The dropdown component containing the resolution options")]
    [SerializeField]
    private Dropdown resolutionDropdown;
    [Tooltip("The dropdown component containing the graphical quality options")]
    [SerializeField]
    private Dropdown graphicalSettingsDropdown;
    [Tooltip("The slider that controls the master volume")]
    [SerializeField]
    private Slider masterVolumeSlider;
    [Tooltip("The slider that controls the sound effects volume")]
    [SerializeField]
    private Slider soundEffectsVolumeSlider;
    [Tooltip("The slider that controls the music volume")]
    [SerializeField]
    private Slider musicVolumeSlider;

    #endregion

    #region Unity Methods

    // Runs once before the first frame update
    private void Start()
    {
        // Generates the resolution options for the resolutions dropdown
        GenerateResolutionOptions(resolutionDropdown);

        // Loads player preferences
        LoadPreferences();
    }

    // Runs once as the object is disabled
    private void OnDisable()
    {
        // Saves the player prefs
        PlayerPrefs.Save();
    }

    #endregion

    #region Custom Methods

    public void LoadPreferences()
    {
        // Restores master volume functionality
        masterVolumeSlider.value = PlayerPrefs.GetFloat("Master Volume");
        soundEffectsVolumeSlider.value = PlayerPrefs.GetFloat("Sound Effects Volume");
        musicVolumeSlider.value = PlayerPrefs.GetFloat("Music Volume");

        // Restores graphical quality setting
        graphicalSettingsDropdown.value = QualitySettings.GetQualityLevel();
        graphicalSettingsDropdown.RefreshShownValue();

        // Restores fullscreen toggle state
        bool isFullscreen = true;
        if (PlayerPrefs.GetString("Fullscreen") == "false")
        {
            isFullscreen = false;
        }
        fullscreenToggle.isOn = isFullscreen;

        // Restoring resolution is not necessary as the GenerateResolutionOptions
        // already does that for us.
    }

    public void GenerateResolutionOptions(Dropdown resolutionDropdown)
    {
        // Clears the default options
        resolutionDropdown.ClearOptions();
        // Fills the resolutions array with the screen resolutions
        resolutions = Screen.resolutions;

        // Creates a new list to store the resolution options in
        // This is necessary because the dropdown component uses a list instead of an array
        List<string> options = new List<string>();

        // The index of the screen's current resolution
        int currenResolutionIndex = 0;

        // Iterates through the resolutions array
        for (int i = 0; i < resolutions.Length; i++)
        {
            // Makes a display text for the options
            string option = resolutions[i].width + "x" + resolutions[i].height;
            // Adds the option to the option list
            options.Add(option);

            // Checks if the current resolution is the current iterated resolution
            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                // Saves the resolution's index
                currenResolutionIndex = i;
            }
        }

        // Adds the options list to the dropdown component
        resolutionDropdown.AddOptions(options);
        // Equals the resolution value to the current resolution index
        resolutionDropdown.value = currenResolutionIndex;
        // Refreshes the shown values
        resolutionDropdown.RefreshShownValue();
    }

    // Sets the screen resolution to the desired one 
    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, FullScreenMode.FullScreenWindow);
        // Saves the resolution to player prefs
        PlayerPrefs.SetInt("Resolution Width", resolution.width);
        PlayerPrefs.SetInt("Resolution Height", resolution.height);

        Debug.Log(PlayerPrefs.GetInt("Resolution Width"));
    }

    // Sets the volume of the master mixer
    public void SetVolumeMaster(float volume)
    {
        mainMixer.SetFloat("masterVolume", volume);
        PlayerPrefs.SetFloat("Master Volume", volume);
    }

    // Sets the volume of the music mixer
    public void SetVolumeMusic(float volume)
    {
        mainMixer.SetFloat("musicVolume", volume);
        PlayerPrefs.SetFloat("Music Volume", volume);
    }

    // Sets the volume of the sound effects mixer
    public void SetVolumeSoundEffects(float volume)
    {
        mainMixer.SetFloat("soundEffectsVolume", volume);
        PlayerPrefs.SetFloat("Sound Effects Volume", volume);
    }

    // Sets the project's quality level
    public void SetGraphicsQuality(int qualityLevel)
    {
        QualitySettings.SetQualityLevel(qualityLevel);
        PlayerPrefs.SetInt("Graphical Settings", qualityLevel);
    }

    // Sets the screen to fullscreen option
    public void SetFullscreen(bool isFullScren)
    {
        Screen.fullScreen = isFullScren;

        // Saves fullscreen toggle state
        string saveFullscreenState = "true";
        if (!isFullScren)
        {
            saveFullscreenState = "false";
        }
        PlayerPrefs.SetString("Fullscreen", saveFullscreenState);

    }

    #endregion
}
