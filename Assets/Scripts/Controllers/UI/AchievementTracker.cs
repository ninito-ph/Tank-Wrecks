using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AchievementTracker : MonoBehaviour
{
    #region Field Declarations

    // The reward for each achievement
    [Header("Achievement Rewards")]
    [SerializeField]
    [Tooltip("The reward for the enemy killed achievement")]
    private int killsAchievementReward;
    [SerializeField]
    [Tooltip("The reward for the wave survival achievement")]
    private int survivalAchievementReward;
    [SerializeField]
    [Tooltip("The reward for the new game plus achievement")]
    private int newGamesAchievementReward;
    [SerializeField]
    [Tooltip("The reward for the shots fired achievement")]
    private int shotsAchievementReward;
    [SerializeField]
    [Tooltip("The reward for the powerup collection achievement")]
    private int powerupAchievementReward;

    // The metrics being tracked
    private int enemiesKilled = 0;
    private int wavesSurvived = 0;
    private int newGamesStarted = 0;
    private int shotsFired = 0;
    private int powerupsCollected = 0;

    // Sets or gets the achievement metrics of the match
    public AchievementMetrics AchievementMetrics
    {
        get
        {
            // Generates a new achievement metrics struct and returns it
            return new AchievementMetrics(enemiesKilled, wavesSurvived, newGamesStarted, shotsFired, powerupsCollected);
        }

        set
        {
            Debug.Log("yes");
            // Loads the set metrics
            LoadMetrics(value);
        }
    }

    #endregion

    #region Unity Methods

    // Start is called before the first frame update
    private void Start()
    {
        EventBroker.KillAchieve += HandleAchievement;
        EventBroker.WaveAchieve += HandleAchievement;
        EventBroker.ShotAchieve += HandleAchievement;
        EventBroker.PowerupAchieve += HandleAchievement;
        EventBroker.NewGamePlusStarted += HandleAchievement;
    }

    // Runs once the component is destroyed
    private void OnDestroy()
    {
        // Is unsubscribing from empty handlers necessary? Experimentation so
        // far seems to suggest that no, it is not necessary. UPDATE: Further
        // research on the topic reveals that YES, we DO need to unsubscribe the
        // handlers - which is impossible using empty handlers like that. I
        // would have used EventArgs and other more extensible and reusable
        // means to make the publisher-subscriber pattern would I have known
        // them when I started the project. However, now is far too late to
        // refactor the event broker. May this serve as a lesson for future
        // projects.

        EventBroker.KillAchieve -= HandleAchievement;
        EventBroker.WaveAchieve -= HandleAchievement;
        EventBroker.ShotAchieve -= HandleAchievement;
        EventBroker.PowerupAchieve -= HandleAchievement;
        EventBroker.NewGamePlusStarted -= HandleAchievement;
    }

    #endregion

    #region Event Handlers

    private void HandleAchievement(Achievement achievementType)
    {
        switch (achievementType)
        {
            case Achievement.KillAchievement:
                TrackAchievement(ref enemiesKilled, 10, 30, killsAchievementReward, "enemies destroyed! Score bonus awarded!");
                break;
            case Achievement.WaveAchievement:
                TrackAchievement(ref wavesSurvived, 5, 10, survivalAchievementReward, "waves survived! Score bonus awarded!");
                break;
            case Achievement.ShotAchievement:
                TrackAchievement(ref shotsFired, 10, 25, shotsAchievementReward, "shots fired! Score bonus awarded!");
                break;
            case Achievement.PowerupAchievement:
                TrackAchievement(ref powerupsCollected, 5, 10, powerupAchievementReward, "powerups collected! Score bonus awarded!");
                break;
            case Achievement.NewGameAchievement:
                TrackAchievement(ref newGamesStarted, 1, 1, powerupAchievementReward, "powerups collected! Score bonus awarded!");
                break;
        }
    }

    #endregion

    #region Custom Methods

    // Loads previous achievement metrics
    private void LoadMetrics(AchievementMetrics metrics)
    {
        // Sets all variables to the same value included in the metrics
        enemiesKilled = metrics.enemiesKilled;
        wavesSurvived = metrics.wavesSurvived;
        newGamesStarted = metrics.newGamesStarted;
        shotsFired = metrics.shotsFired;
        powerupsCollected = metrics.powerupsCollected;
    }

    // Tracks the progress of an achievement
    private void TrackAchievement(ref int achievementCounter, int firstTrigger, int nthTrigger, int achievementReward, string unlockMessage)
    {
        // Adds to the achievement counter
        achievementCounter++;

        // The first achievement is achieved when the first trigger amount is met
        if (achievementCounter == firstTrigger)
        {
            EventBroker.CallAddScore(achievementReward);
            EventBroker.CallNotifyAchievement(achievementCounter.ToString(), unlockMessage);
        }
        else // Else the achievement only triggers on every nth trigger
        {
            if (achievementCounter % nthTrigger == 0)
            {
                EventBroker.CallAddScore(achievementReward * (achievementCounter / nthTrigger));
                EventBroker.CallNotifyAchievement(achievementCounter.ToString(), unlockMessage);
            }
        }
    }

    #endregion
}

// A enum that defines achievement types
public enum Achievement
{
    KillAchievement,
    WaveAchievement,
    ShotAchievement,
    PowerupAchievement,
    NewGameAchievement
}

// A struct to store the achievement metrics
public struct AchievementMetrics
{
    // The metrics being tracked
    public int enemiesKilled;
    public int wavesSurvived;
    public int newGamesStarted;
    public int shotsFired;
    public int powerupsCollected;

    // Constructor method
    public AchievementMetrics(int enemiesKilled, int wavesSurvived, int newGamesStarted, int shotsFired, int powerupsCollected)
    {
        // Assigns the arguments to the struct
        this.enemiesKilled = enemiesKilled;
        this.wavesSurvived = wavesSurvived;
        this.newGamesStarted = newGamesStarted;
        this.shotsFired = shotsFired;
        this.powerupsCollected = powerupsCollected;
    }
}
