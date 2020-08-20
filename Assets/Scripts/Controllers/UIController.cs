using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    #region Field Declarations

    #region Core Values

    // Text and other variables used for display in the UI
    private int displayedScore;
    private Text ammoCountText;
    private Text scoreText;
    [Header("Icons")]
    [SerializeField]
    [Tooltip("Fire cooldown icon sprite")]
    private Image fireCooldownIcon;
    [SerializeField]
    [Tooltip("Heart fill icon sprite")]
    private Image heartIcons;
    [Header("Linear interpolation speeds")]
    [SerializeField]
    [Tooltip("The linear interpolation speed at which the score number is updated")]
    private float scoreLerpSpeed;
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
    }

    private void Start() 
    {
        gameController = FindObjectOfType<GameController>();
        playerController = gameController.PlayerReference;
    }

    // Update runs every frame
    private void Update()
    {
        UpdateScore();
        UpdateFireCooldown();
        
        // Checks if a change in health has occurred to prevent the expensive lerp operation from running all the time
        // Compares a float to a int using Approximately to prevent floating point comparison imprecision error
        if (Mathf.Approximately(heartIcons.fillAmount, playerController.Health))
        {
            UpdateHealth();
        }
    }

    #endregion

    #region Custom Methods

    // Updates score number with a crisp lerped effect
    private void UpdateScore()
    {
        // The score number is linearly interpolated for a crisp score increase effect
        float interpolatedScore = Mathf.Lerp(displayedScore, gameController.Score, Time.deltaTime * scoreLerpSpeed);
        // The displayed number is then rounded to an integer, so that no decimal scores may appear
        displayedScore = Mathf.RoundToInt(interpolatedScore);
        scoreText.text = displayedScore.ToString();
    }

    // Increases and decreases heart icon fill with a cris
    private void UpdateHealth()
    {
        // Time.deltaTime is multiplied by 13 - a number reached through testing.
        heartIcons.fillAmount = Mathf.Lerp(heartIcons.fillAmount, playerController.Health / playerController.MaxHealth, Time.deltaTime * healthLerpSpeed);
    }

    // Updates ammo count text
    private void UpdateAmmo()
    {
        ammoCountText.text = playerController.Ammo.ToString();
    }

    // Updates fire cooldown fill
    private void UpdateFireCooldown()
    {
        // Time.deltaTime is multiplied by 13 - a number reached through testing.
        fireCooldownIcon.fillAmount = Mathf.Lerp(fireCooldownIcon.fillAmount, playerController.FireCooldown / playerController.MaxFireCooldown, Time.deltaTime * fireCooldownLerpSpeed);
    }

    #endregion
}
