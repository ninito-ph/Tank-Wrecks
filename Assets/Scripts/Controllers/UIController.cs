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

    // Useful object references
    private GameController gameController;
    private PlayerController playerController;

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
        // Compares a float to a int using Approximately to prevent floating point comparison imprecision error
        // Checks if a change in the value has occurred to prevent the expensive lerp operations from running all the time
        
        
        if (!Mathf.Approximately(displayedScore, gameController.Score))
        {
            UpdateScore();
        }

        if (!Mathf.Approximately(fireCooldownIcon.fillAmount, playerController.FireCooldown / playerController.MaxFireCooldown))
        {
            UpdateFireCooldown();
        }

        /*  if (!Mathf.Approximately(heartIcons.fillAmount, playerController.Health))
         {
             UpdateHealth();
         } */
    }

    #endregion

    #region Custom Methods

    // Updates score number with a crisp lerped effect
    private void UpdateScore()
    {
        // The score number is linearly interpolated for a crisp score increase effect
        float interpolatedScore = Mathf.Lerp(displayedScore, gameController.Score, scoreLerpSpeed);

        // The displayed number is then ceiled to an integer, so that no decimal scores may appear
        displayedScore = Mathf.CeilToInt(interpolatedScore);
        scoreText.text = displayedScore.ToString();
    }

    // Increases and decreases heart icon fill
    private void UpdateHealth()
    {

        //heartIcons.fillAmount = Mathf.Lerp(heartIcons.fillAmount, playerController.Health / playerController.MaxHealth, Time.deltaTime * healthLerpSpeed);
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
        fireCooldownIcon.fillAmount = Mathf.Lerp(fireCooldownIcon.fillAmount, playerController.FireCooldown / playerController.MaxFireCooldown, Time.deltaTime * fireCooldownLerpSpeed);
    }

    #endregion
}
