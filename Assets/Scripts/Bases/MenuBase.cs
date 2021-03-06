﻿using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

// I don't usually enjoy using replacements to monobehaviour, but Odin Inspector is a proven tool used in very large-scale games.
public class MenuBase : SerializedMonoBehaviour
{
    #region Field Declarations

    // Dictionary containing all menus
    [SerializeField]
    [DictionaryDrawerSettings(KeyLabel = "Menu Name", ValueLabel = "Menu Parent GameObject")]
    private Dictionary<string, GameObject> menus;
    [SerializeField]
    [Tooltip("The click sound that happens on the UI")]
    private AudioClip clickSound;
    [SerializeField]
    [Tooltip("The audio mixer used for sound effect")]
    protected AudioMixer mainMixer;

    // The sound receiver for menu sounds
    private AudioSource menuAudioSource;

    #endregion

    #region Unity Methods

    protected virtual void Start()
    {
        // Loops through every audiosource in the camera
        foreach (AudioSource audioSource in Camera.main.gameObject.GetComponents<AudioSource>())
        {
            // Checks every audiosource until it finds the one with the menu
            // click sound. We identiy the right one by checking which has said
            // click sound
            if (audioSource.clip == clickSound)
            {
                menuAudioSource = audioSource;
            }

            // If it has found the 
            if (menuAudioSource != null)
            {
                // Makes menu sounds play even when the game is paused
                menuAudioSource.ignoreListenerPause = true;
                // Ends the loop early
                break;
            }
        }
    }

    #endregion

    #region Custom Methods

    // Public methods because Unity needs to be able to detect the method for button functionality

    // Switches to the desired menu
    public void SwitchToMenu(string desiredMenu, bool mouseEnabled = true)
    {
        // Sets the given menu active
        menus[desiredMenu].SetActive(true);

        // For each menu in the menus dictionary
        foreach (GameObject menu in menus.Values)
        {
            // Check if the menu is equal to the given menu
            if (menu != menus[desiredMenu])
            {
                // If it isn't, disable it
                menu.SetActive(false);
            }
        }

        // Disables mouse if mouseEnabled is false
        if (mouseEnabled == false)
        {
            // Locks the cursor and makes it invisible;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            // Unlocks the cursor and makes it visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        PlayClickSound();
    }

    // Opens a menu without disabling the others
    public void OverlayMenu(string desiredMenu, bool mouseEnabled = true)
    {
        // Sets the given menu active
        menus[desiredMenu].SetActive(true);

        // Disables mouse if mouseEnabled is false
        if (mouseEnabled == false)
        {
            // Locks the cursor and makes it invisible;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            // Unlocks the cursor and makes it visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Switches to a new scene
    public void SwitchToScene(string sceneName, bool useLoadingScreen)
    {
        // If we are to use a loading scene
        if (useLoadingScreen == true)
        {
            // Set the scene to load data to the desired scene
            GlobalData.SceneToLoad = sceneName;
            // Load the loading screen scene
            SceneManager.LoadScene("LoadingScene");
        }
        else
        {
            // Load the desired scene directly
            SceneManager.LoadScene(sceneName);
        }

        PlayClickSound();
    }

    // Gets UI volume and plays sound
    private void PlayClickSound()
    {
        menuAudioSource.Play();
    }

    #endregion
}
