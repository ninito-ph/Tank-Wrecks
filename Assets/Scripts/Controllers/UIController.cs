using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    #region Field Declarations

    #region Core Values

    // Text and other variables used for display in the UI
    private int displayedScore = 1;
    [Header("Text")]
    [SerializeField]
    [Tooltip("Wave counter text")]
    private Text waveCountText;
    [SerializeField]
    [Tooltip("Ammo counter text")]
    private Text ammoCountText;
    [SerializeField]
    [Tooltip("Score counter text")]
    private Text scoreText;
    [Header("Icons")]
    [SerializeField]
    [Tooltip("Fire cooldown icon sprite")]
    private Image fireCooldownIcon;
    [SerializeField]
    [Tooltip("Heart fill icon sprite")]
    private Image[] heartIcons;
    [Header("Linear interpolation speeds")]
    [SerializeField]
    [Tooltip("The linear interpolation speed at which the score number is updated")]
    [Range(0f, 1f)]
    private float scoreLerpSpeed = 0.5f;
    [Tooltip("The linear interpolation speed at which the health fill is updated")]
    [SerializeField]
    private float healthLerpSpeed = 13f;
    [Tooltip("The linear interpolation speed at which the fire readiness icon is updated")]
    [SerializeField]
    private float fireCooldownLerpSpeed = 13f;

    // Necessary object references
    private GameController gameController;
    private PlayerController playerController;

    // UI object object references
    [SerializeField]
    [Tooltip("The empty gameObject containing the HUD UI elements")]
    private GameObject hudUIGroup;
    [SerializeField]
    [Tooltip("The empty gameObject containing the Pause Menu UI elements")]
    private GameObject pauseMenuUIGroup;

    #endregion

    #endregion

    #region Unity Methods

    // Awake runs before the first frame
    private void Awake()
    {
        // Subscribes ammo update to shot fired by player event
        EventBroker.ShotFired += UpdateAmmo;
        // Subscribes wave update to wave over event
        EventBroker.WaveStarted += UpdateWave;
        // Subscribes hide UI components to game pause toggle event
        EventBroker.PauseToggled += HideUIComponents;
    }

    private void Start()
    {
        // Caches reference to the gamecontroller
        gameController = GameObject.Find("Game Controller").GetComponent<GameController>();

        // Retrieves the player reference from the gamecontroller
        playerController = gameController.PlayerReference.GetComponent<PlayerController>();
    }

    // Update runs every frame
    private void Update()
    {
        // Updates UI values
        UpdateScore();
        UpdateFireCooldown();
        UpdateHealth();

        // Checks for pause Input
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Pauses/Resumes game
            TogglePause();
        }
    }

    #endregion

    #region Custom Methods

    // Updates score number with a crisp lerped effect
    private void UpdateScore()
    {
        if (!Mathf.Approximately(fireCooldownIcon.fillAmount, playerController.FireCooldown / playerController.MaxFireCooldown))
        {
            // The score number is linearly interpolated for a crisp score increase effect
            float interpolatedScore = Mathf.Lerp(displayedScore, gameController.Score, scoreLerpSpeed);

            // The displayed number is then ceiled to an integer, so that no decimal scores may appear
            displayedScore = Mathf.CeilToInt(interpolatedScore);
            scoreText.text = displayedScore.ToString();
        }
    }

    // Increases and decreases heart icon fill
    private void UpdateHealth()
    {

        // FIXME: This code is so abominably bad it hurts to look at. Make a more elegant solution later.
        // If the player health is 2, and the third heart's fill amount is not 0
        if (playerController.Health == 3)
        {
            foreach (Image heart in heartIcons)
            {
                if (!Mathf.Approximately(heart.fillAmount, 1f))
                {
                    heart.fillAmount = Mathf.Lerp(heart.fillAmount, 1, Time.deltaTime * healthLerpSpeed);
                }
            }
        }

        // If the player health is 2, and the third heart's fill amount is not 0
        if (playerController.Health <= 2 && !Mathf.Approximately(heartIcons[2].fillAmount, 0f))
        {
            // Linearly interpolate the fill amount of the third heart down to 0
            heartIcons[2].fillAmount = Mathf.Lerp(heartIcons[2].fillAmount, 0, Time.deltaTime * healthLerpSpeed);
        }

        // If the player health is 1, and the second heart's fill amount is not 0
        if (playerController.Health <= 1 && !Mathf.Approximately(heartIcons[1].fillAmount, 0f))
        {
            // Linearly interpolate the fill amount of the second heart down to 0
            heartIcons[1].fillAmount = Mathf.Lerp(heartIcons[1].fillAmount, 0, Time.deltaTime * healthLerpSpeed);
        }

        // If the player health is 0, and the first heart's fill amount is not 0
        if (playerController.Health <= 0 && !Mathf.Approximately(heartIcons[0].fillAmount, 0f))
        {
            // Linearly interpolate the fill amount of the first heart down to 0
            heartIcons[0].fillAmount = Mathf.Lerp(heartIcons[0].fillAmount, 0, Time.deltaTime * healthLerpSpeed);
        }
    }

    // Updates ammo count text
    private void UpdateAmmo()
    {
        ammoCountText.text = playerController.Ammo.ToString();
    }

    // Updates wave counter text
    private void UpdateWave()
    {
        waveCountText.text = gameController.Wave.ToString();
    }

    // Updates fire cooldown fill
    private void UpdateFireCooldown()
    {
        if (!Mathf.Approximately(fireCooldownIcon.fillAmount, playerController.FireCooldown / playerController.MaxFireCooldown))
        {
            fireCooldownIcon.fillAmount = Mathf.Lerp(fireCooldownIcon.fillAmount, playerController.FireCooldown / playerController.MaxFireCooldown, Time.deltaTime * fireCooldownLerpSpeed);
        }
    }

    private void HideUIComponents()
    {
        // Checks if the game is paused, to decide whether to show the UI or the Pause Menu
        if (gameController.IsPaused == true)
        {
            // Disables the HUD UI group
            hudUIGroup.SetActive(false);
            // Enables the Pause Menu UI group
            pauseMenuUIGroup.SetActive(true);

            // Unlocks the cursor and makes it visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Enables the HUD UI group
            hudUIGroup.SetActive(true);
            // Enables the Pause Menu UI group
            pauseMenuUIGroup.SetActive(false);

            // Unlocks the cursor and makes it visible;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Toggles the game pause state
    // This exists as a method because Unity requires a method for a button to toggle
    public void TogglePause()
    {
        gameController.IsPaused = !gameController.IsPaused;
    }

    #endregion
}
