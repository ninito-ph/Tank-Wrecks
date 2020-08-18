using System;

public static class EventBroker
{
    #region Actions
    
    // Actions that notify other classes of notable events
    // Methods and actions are static so that classes don't need a reference to EventBroker

    // Notifies that an enemy has been destroyed
    public static event Action<EnemyBase> EnemyDestroyed;

    // Notifies that a shot has been fired
    public static event Action ShotFired;

    #endregion

    #region Call Methods

    // Methods that are called by other classes to trigger actions
    // Methods and actions are static so that classes don't need a reference to EventBroker
    public static void CallEnemyDestroyed(EnemyBase assignedReference)
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

    #endregion
}
