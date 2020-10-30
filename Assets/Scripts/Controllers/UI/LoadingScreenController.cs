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
    private Image loadingBarFill;
    [SerializeField]
    [Tooltip("The text element containing space for the tips")]
    private Text tipsText;
    [SerializeField]
    [Tooltip("The image element through which the displayed object is displayed")]
    private RawImage displayObjectImage;

    // Visual content to distract the player while the scene loads
    [Header("Visual Content")]
    [SerializeField]
    [Tooltip("The possible objects to be displayed in the loading screen")]
    private GameObject[] displayableObjects;
    [SerializeField]
    [Tooltip("The place at which to create the displayable prefab")]
    private GameObject displayOrigin;
    [SerializeField]
    [Tooltip("The speed at which to spin the displayed object")]
    private Vector3 rotationSpeed = new Vector3(0f, 5f, 0f);

    // Stores cache of mainCamera
    private Camera mainCamera;

    // The model that was put on display for the duration loading screen
    private GameObject displayedObject;
    private Texture2D displayedObjectTexture;

    #endregion

    #region Unity Methods

    private void Start()
    {

        // TODO: Remove this null check once testing is done
        if (GlobalData.SceneToLoad != null)
        {
            // Starts the loading process and unloads previous scene
            loadScene = SceneManager.LoadSceneAsync(GlobalData.SceneToLoad, LoadSceneMode.Single);
        }

        // Randomly picks an object and its correlated tip
        int randomPick = Random.Range(0, displayableObjects.Length);

        // Instantiates the displayable model
        displayedObject = Instantiate(displayableObjects[randomPick], displayOrigin.transform.position, Quaternion.identity);
        // Enables the display mode on an object, which returns a tip
        tipsText.text = displayedObject.GetComponent<DisplayableObjectController>().LoadingTip;

        // Caches main camera
        mainCamera = Camera.main;

        // Sets up display texture
        SetupRenderTexture(1024);

        // Clears GlobalData SceneToLoad
        GlobalData.SceneToLoad = null;
    }

    private void Update()
    {
        // Updates the fill amount of the loading bar
        // progess is being divided by a total of 0.9 instead of 1; at 1 the scene has finished loading, and the scene changes automatically
        // TODO: Remove this null check once testing is done
        if (loadScene != null)
        {
            loadingBarFill.fillAmount = Mathf.Clamp01(loadScene.progress / 0.9f);
        }

        // Rotates the displayed object
        displayedObject.transform.Rotate(rotationSpeed * Time.deltaTime, Space.World);

        // Updates the display texture
        displayObjectImage.texture = DisplayObjectToTexture(displayedObjectTexture.height);
    }

    #region Custom Methods

    // Sets up the camera to render the render texture
    private void SetupRenderTexture(int textureSize)
    {
        // Initializes texture
        displayedObjectTexture = new Texture2D(textureSize, textureSize);

        // Sets the target render texture of the camera to Unity's temporary texture
        // temporary textures usually aren't used this way, but doing this causes no issues
        RenderTexture temporaryRenderTexture = RenderTexture.GetTemporary(textureSize, textureSize, 16, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 2);
        mainCamera.targetTexture = temporaryRenderTexture;
    }

    // Renders the display object to a texture
    private Texture2D DisplayObjectToTexture(int textureSize)
    {
        // Renders the camera's view
        mainCamera.Render();

        // Sets the camera's target texture to the active render texture
        RenderTexture.active = mainCamera.targetTexture;

        // Saves render texture into texture
        displayedObjectTexture.ReadPixels(new Rect(0f, 0f, displayedObjectTexture.width, displayedObjectTexture.height), 0, 0);
        displayedObjectTexture.Apply();

        return displayedObjectTexture;
    }

    #endregion

    #endregion
}
