using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUDController : MenuBase
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
    [Tooltip("The the achievements texts. WARNING: The achievement count and message MUST be at indexes 0 and 1, respectively.")]
    [SerializeField]
    private Text[] achievementTexts;

    // Necessary object references
    private GameManager GameManager;
    private PlayerController playerController;

    // Other coroutines
    private Coroutine achievementRoutine;

    #endregion

    #endregion

    #region Unity Methods

    protected override void Start()
    {
        // Calls the bases' start
        base.Start();

        // Subscribes ammo update to shot fired by player event
        EventBroker.ShotFired += UpdateAmmo;
        // Subscribes wave update to wave over event
        EventBroker.WaveStarted += UpdateWave;
        // Subscribes goto game over screen to player destroyed event
        EventBroker.GameEnded += GotoGameOverScreen;
        // Subscribes achievement display to notify achievement event
        EventBroker.NotifyAchievement += ShowAchievement;

        // Caches reference to the GameManager
        GameManager = GameObject.Find("Game Controller").GetComponent<GameManager>();

        // Retrieves the player reference from the GameManager
        playerController = GameManager.PlayerReference.GetComponent<PlayerController>();

        // Triggers help screen if it is the first time the player has entered the game
        if (PlayerPrefs.GetString("First Startup", "false") == "true")
        {
            // Enables Help Screen
            OverlayMenu("Help Screen", false);
        }
    }

    // Update runs every frame
    private void Update()
    {
        // Updates UI values
        UpdateScore();
        UpdateFireCooldown();
        UpdateHealth();

        // Checks for pause Input
        if (Input.GetKeyDown(KeyCode.Escape) && playerController != null)
        {
            // Pauses/Resumes game
            TogglePause();
        }
    }

    // Runs once object is destroyed
    private void OnDestroy()
    {
        // Unsubscribes from all subscribed events to prevent memory leaks and other odd behaviour
        // Unsubscribes ammo update to shot fired by player event
        EventBroker.ShotFired -= UpdateAmmo;
        // Unsubscribes wave update to wave over event
        EventBroker.WaveStarted -= UpdateWave;
        // Unsubscribes achievement display to notify achievement event
        EventBroker.NotifyAchievement -= ShowAchievement;
        // Unsubscribes from game ended event
        EventBroker.GameEnded -= GotoGameOverScreen;
    }

    #endregion

    #region Custom Methods

    // Updates score number with a crisp lerped effect
    private void UpdateScore()
    {
        if (!Mathf.Approximately(fireCooldownIcon.fillAmount, playerController.FireCooldown / playerController.MaxFireCooldown))
        {
            // The score number is linearly interpolated for a crisp score increase effect
            float interpolatedScore = Mathf.Lerp(displayedScore, GameManager.Score, scoreLerpSpeed);

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
        waveCountText.text = GameManager.Wave.ToString();
    }

    // Updates fire cooldown fill
    private void UpdateFireCooldown()
    {
        if (!Mathf.Approximately(fireCooldownIcon.fillAmount, playerController.FireCooldown / playerController.MaxFireCooldown))
        {
            fireCooldownIcon.fillAmount = Mathf.Lerp(fireCooldownIcon.fillAmount, playerController.FireCooldown / playerController.MaxFireCooldown, Time.deltaTime * fireCooldownLerpSpeed);
        }
    }

    // Toggles the game pause state
    // This exists as a method because Unity requires a method for a button to toggle
    public void TogglePause()
    {
        // Pauses/resumes the game
        GameManager.IsPaused = !GameManager.IsPaused;

        // Checks if the game is paused, to decide whether to show the HUD or the Pause Menu
        if (GameManager.IsPaused == true)
        {
            // Switches to pause menu
            SwitchToMenu("Pause Menu");
        }
        else
        {
            // Switches to HUD
            SwitchToMenu("HUD", false);
        }
    }

    // Displays an achievement
    public void ShowAchievement(string achievementCount, string unlockMessage)
    {
        // Sets the UI text
        achievementTexts[0].text = achievementCount;
        achievementTexts[1].text = unlockMessage;
        // Fades in the achievement
        achievementRoutine = StartCoroutine(FadeInAchievement(4f, achievementTexts));
    }

    // Switches to the options menu
    public void GotoOptions()
    {
        SwitchToMenu("Options Menu");
    }

    // Switches to the pause menu
    public void GotoPauseMenu()
    {
        SwitchToMenu("Pause Menu");
    }

    // Switches to the game over screen This method takes in an input because it
    // needs to match the Event signature. I now know about EventArgs and other
    // mannners of making the code more extensible and reusable. I am, however,
    // running out of time as I write this, so I cannot go back and perform
    // another refactoring on the code. May this be a lesson for the next
    // project. 
    public void GotoGameOverScreen(LeaderboardEntry discard)
    {
        SwitchToMenu("Game Over Screen");
    }

    // Switches to the help screen
    public void GotoHelpScreen()
    {
        SwitchToMenu("HUD", false);
        OverlayMenu("Help Screen");
    }

    #region Coroutines

    // Fades text to a given transparency
    private IEnumerator FadeInAchievement(float duration, Text[] textsToFade)
    {
        // Updates text color until it has faded in completely
        for (float time = 0; time <= duration * 0.2f; time += Time.deltaTime)
        {
            // Sets the color to the lerp operation's result color
            foreach (Text achievementText in textsToFade)
            {
                // Fade in text
                achievementText.color = Color.Lerp(achievementText.color, new Color(achievementText.color.r, achievementText.color.g, achievementText.color.b, 1f), Mathf.Min(1, (time / duration * 0.2f)));
            }

            // Waits for the next frame
            yield return null;
        }

        // Waits 
        yield return new WaitForSeconds(duration * 0.6f);

        // Updates text color until it has faded in completely
        for (float time = 0; time <= duration * 0.2f; time += Time.deltaTime)
        {
            // Sets the color to the lerp operation's result color
            foreach (Text achievementText in textsToFade)
            {
                // Fade in text
                achievementText.color = Color.Lerp(achievementText.color, new Color(achievementText.color.r, achievementText.color.g, achievementText.color.b, 0.0f), Mathf.Min(1, (time / duration * 0.2f)));
            }

            // Waits for the next frame
            yield return null;
        }

        // Sets the color alpha to zero
        foreach (Text achievementText in textsToFade)
        {
            achievementText.color = new Color(achievementText.color.r, achievementText.color.g, achievementText.color.b, 0.0f);
        }

        // Ends coroutine
        yield break;
    }

    #endregion

    #endregion
}
