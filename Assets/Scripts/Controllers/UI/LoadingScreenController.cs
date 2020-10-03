using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScreenController : MonoBehaviour
{
    #region Field Declarations

    // The asynchronous operation for loading the scene
    private AsyncOperation loadScene;

    // Visual elements of the loading screen
    [Header("Interface")]
    [SerializeField]
    [Tooltip("The loading bar of the loading screen")]
    private Image loadingBar;
    [SerializeField]
    [Tooltip("The text element containing space for the tips")]
    private Text tipsText;

    // Visual content to distract the player while the scene loads
    [Header("Visual Content")]
    [SerializeField]
    [Tooltip("The possible tips in the loading screen. Note the models and tips must be correlated.")]
    private string[] tips;
    [SerializeField]
    [Tooltip("The possible models in the loading screen. Note the models and tips must be correlated.")]
    private GameObject[] displayableModels;
    [SerializeField]
    [Tooltip("The place at which to create the displayable prefab")]
    private GameObject displayOrigin;
    [SerializeField]
    [Tooltip("The speed at which to spin the displayed object")]
    private Vector3 rotationSpeed = new Vector3(0f, 5f, 0f);

    // The model that was put on display for the duration loading screen
    private GameObject displayedObject;

    #endregion

    #region Unity Methods

    private void Start()
    {
        // Starts the loading process and unloads previous scene
        loadScene = SceneManager.LoadSceneAsync(LoadData.SceneToLoad, LoadSceneMode.Single);

        // Randomly picks an object and its correlated tip
        int randomPick = Random.Range(0, Mathf.Min(tips.Length, displayableModels.Length));

        // Sets the tip to that of the correlated object
        tipsText.text = tips[randomPick];
        // Instantiates the displayable model
        displayedObject = Instantiate(displayableModels[randomPick], displayedObject.transform.position, Quaternion.identity);

        // Clears LoadData SceneToLoad
        LoadData.SceneToLoad = null;
    }

    private void Update()
    {
        // Rotates the displayed object
        displayedObject.transform.Rotate(rotationSpeed * Time.deltaTime, Space.World);

        // Updates the fill amount of the loading bar
        // progess is being divided by a total of 0.9 instead of 1; at 1 the scene has finished loading, and the scene changes automatically
        loadingBar.fillAmount = Mathf.Clamp01(loadScene.progress / 0.9f);
    }

    #endregion
}
