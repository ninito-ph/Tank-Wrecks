using System;
using UnityEngine;

public static class EventBroker
{
    #region Actions

    // Actions that notify other classes of notable events
    // Methods and actions are static so that classes don't need a reference to EventBroker

    // Notifies that an enemy has been destroyed
    // This action passes the enemy's reference as an argument
    public static event Action<GameObject> EnemyDestroyed;
    public static event Action<Achievement> KillAchieve;

    // Notifies that a shot has been fired
    public static event Action ShotFired;
    public static event Action<Achievement> ShotAchieve;

    // Notifies that a wave has ended
    public static event Action WaveStarted;
    public static event Action<Achievement> WaveAchieve;

    // Sends a notification to add a number to score
    // This action passes a score amount as an argument
    public static event Action<float> AddScore;

    // Sends a notification that a powerup has been picked up
    // This leviathan of an action passes the powerup type, its duration, its count, and its multiplier.
    public static event Action<PowerupTypes, float, int, float> ActivatePowerup;
    public static event Action<Achievement> PowerupAchieve;

    // Notifies that the player has been destroyed
    public static event Action PlayerDestroyed;

    // Notifies that the game has ended
    public static event Action<LeaderboardEntry> GameEnded;

    // Notifies the game has been paused
    public static event Action PauseToggled;

    // Induces stress (shakes) on the camera transform
    public static event Action<float, Vector3> ShakeCamera;

    // Notifies an achievement has been achieved
    public static event Action<string, string> NotifyAchievement;

    // Notifies a new game plus has begun
    public static event Action<Achievement> NewGamePlusStarted;

    #endregion

    #region Call Methods

    // Methods that are called by other classes to trigger actions
    // Methods and actions are static so that classes don't need a reference to EventBroker
    // Actions are checked to see if they are null. If they are, then they have no subscribers, and would case an error on being raised.

    // Consider using EventName?.Invoke() as a succint way check for subscribers

    public static void CallKillAchieve(Achievement achievement = Achievement.KillAchievement)
    {
        KillAchieve?.Invoke(achievement);
    }

    public static void CallShotAchieve(Achievement achievement = Achievement.ShotAchievement)
    {
        ShotAchieve?.Invoke(achievement);
    }

    public static void CallWaveAchieve(Achievement achievement = Achievement.WaveAchievement)
    {
        WaveAchieve?.Invoke(achievement);
    }

    public static void CallPowerupAchieve(Achievement achievement = Achievement.PowerupAchievement)
    {
        PowerupAchieve?.Invoke(achievement);
    }

    public static void CallNewGamePlusStarted(Achievement achievement = Achievement.NewGameAchievement)
    {
        NewGamePlusStarted?.Invoke(achievement);
    }

    public static void CallGameEnded(LeaderboardEntry gameMetrics)
    {
        GameEnded?.Invoke(gameMetrics);
    }

    public static void CallShakeCamera(float stress, Vector3 stressOrigin)
    {
        if (ShakeCamera != null)
        {
            ShakeCamera(stress, stressOrigin);
        }
    }

    public static void CallEnemyDestroyed(GameObject assignedReference)
    {
        if (EnemyDestroyed != null)
        {
            EnemyDestroyed(assignedReference);
        }
    }

    public static void CallShotFired()
    {
        if (ShotFired != null)
        {
            ShotFired();
        }
    }

    public static void CallWaveStarted()
    {
        if (WaveStarted != null)
        {
            WaveStarted();
        }
    }

    public static void CallAddScore(float scoreAmount)
    {
        if (AddScore != null)
        {
            AddScore(scoreAmount);
        }
    }

    public static void CallActivatePowerup(PowerupTypes powerupType, float powerupDuration = 0, int powerupAmount = 1, float speedMultiplier = 1)
    {
        if (ActivatePowerup != null)
        {
            ActivatePowerup(powerupType, powerupDuration, powerupAmount, speedMultiplier);
        }
    }

    public static void CallPlayerDestroyed()
    {
        if (PlayerDestroyed != null)
        {
            PlayerDestroyed();
        }
    }

    public static void CallPauseToggled()
    {
        if (PauseToggled != null)
        {
            PauseToggled();
        }
    }

    public static void CallNotifyAchievement(string achievementQuantity, string achievementMessage)
    {
        if (NotifyAchievement != null)
        {
            NotifyAchievement(achievementQuantity, achievementMessage);
        }
    }

    #endregion
}
