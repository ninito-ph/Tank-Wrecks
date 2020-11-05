using UnityEngine;
using System;

public static class EventBroker
{
    #region Actions
    
    // Actions that notify other classes of notable events
    // Methods and actions are static so that classes don't need a reference to EventBroker

    // Notifies that an enemy has been destroyed
    // This action passes the enemy's reference as an argument
    public static event Action<GameObject> EnemyDestroyed;

    // Notifies that a shot has been fired
    public static event Action ShotFired;

    // Notifies that a wave has ended
    public static event Action WaveStarted;

    // Sends a notification to add a number to score
    // This action passes a score amount as an argument
    public static event Action<float> AddScore;

    // Sends a notification that a powerup has been picked up
    // This leviathan of an action passes the powerup type, its duration, its count, and its multiplier.
    public static event Action<PowerupTypes, float, int, float> ActivatePowerup;

    // Notifies that the player has been destroyed
    public static event Action PlayerDestroyed;

    // Notifies the game has been paused
    public static event Action PauseToggled;

    // Induces stress (shakes) on the camera transform
    public static event Action<float, Vector3> ShakeCamera;

    #endregion

    #region Call Methods

    // Methods that are called by other classes to trigger actions
    // Methods and actions are static so that classes don't need a reference to EventBroker
    // Actions are checked to see if they are null. If they are, then they have no subscribers, and would case an error on being raised.
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

    #endregion
}
