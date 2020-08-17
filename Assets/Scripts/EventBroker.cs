using System;

public class EventBroker
{
    #region Actions
    
    // Actions that notify other classes of notable events
    // Methods and actions are static so that classes don't need a reference to EventBroker
    public static event Action<EnemyBase> EnemyDestroyed;
    
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
    
    #endregion
}
