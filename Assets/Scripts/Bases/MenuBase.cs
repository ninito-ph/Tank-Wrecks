using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using Sirenix.OdinInspector;

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

    // The cache of the camera's position
    [SerializeField]
    [Tooltip("The transform of the main camera so that sounds can be played exactly where the audio listener is.")]
    private Transform cameraTransform;

    #endregion

    #region Custom Methods

    // Switches to the desired menu
    // Public because Unity needs to be able to detect the method for button functionality
    public virtual void SwitchToMenu(string desiredMenu, bool mouseEnabled = true)
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

    // Switches to a new scene
    public virtual void SwitchToScene(string sceneName, bool useLoadingScreen)
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
        float clickVolume;
        mainMixer.GetFloat("soundEffectsVolume", out clickVolume);
        AudioSource.PlayClipAtPoint(clickSound, cameraTransform.position, 1f);
    }

    #endregion

}
