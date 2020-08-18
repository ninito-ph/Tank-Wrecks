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
    private Image fireCooldownIcon;
    private float scoreLerpSpeed;
    private float healthLerpSpeed;

    // Images containing the backdrop for counters
    private Image healthBackdrop;
    private Image ammoBackdrop;

    // Useful object references
    private GameController gameController;
    private PlayerController playerController;

    #endregion

    #endregion

    #region Unity Methods

    private void Awake()
    {
        EventBroker.ShotFired += UpdateAmmo;
    }

    #endregion

    #region Custom Methods

    // Updates score number
    private void UpdateScore()
    {
        // The score number is linearly interpolated for a crisp score increase effect
        float interpolatedScore = Mathf.Lerp(displayedScore, gameController.Score, Time.deltaTime * scoreLerpSpeed);
        // The displayed number is then rounded to an integer, so that no decimal scores may appear
        displayedScore = Mathf.RoundToInt(interpolatedScore);
        scoreText.text = displayedScore.ToString();
    }

    //TODO: Implement new health system and UpdateHealth method
    private void UpdateHealth()
    {

    }

    //TODO: Implement player fire override method so you can trigger this method through an action
    private void UpdateAmmo()
    {
        ammoCountText.text = playerController.Ammo.ToString();
    }

    // Updates fire cooldown fill
    private void UpdateFireCooldown()
    {
        // Time.deltaTime is multiplied by 13 - a number reached through testing.
        fireCooldownIcon.fillAmount = Mathf.Lerp(fireCooldownIcon.fillAmount, playerController.FireCooldown / playerController.MaxFireCooldown, Time.deltaTime * 13);
    }


    #endregion
}
