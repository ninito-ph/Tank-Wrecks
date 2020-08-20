using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyBase : TankBase
{
    #region Field Declarations

    #region Core Values

    // Basic variables for the functioning of the class
    private GameObject assignedReference;
    private EnemyTypes enemyType;
    private GameObject playerReference;

    // Variables used for state machine functionality
    private EnemyBaseState currentState;
    protected RepositionState repositionState = new RepositionState();
    protected FireState fireState = new FireState();
    protected RushState rushState = new RushState();

    #endregion

    #region Properties

    // Properties to preserve encapsulation while still allowing other classes to alter and retrieve values
    public GameObject PlayerReference
    {
        get { return playerReference; }
        set { playerReference = value; }
    }

    public GameObject AssignedReference
    {
        get { return assignedReference; }
        set { assignedReference = value; }
    }

    public EnemyTypes EnemyType
    {
        get { return enemyType; }
        set { enemyType = value; }
    }

    #endregion

    #region Actions

    // Actions to notify other classes of noteworthy events
    private Action<EnemyBase> EnemyDestroyed;

    #endregion

    #endregion

    #region Unity Methods

    protected void Start()
    {
        TransitionToState(repositionState);
    }

    protected override void Update()
    {
        base.Update();
        // Runs the update from the current state of the machine
        currentState.Update(this);
    }

    // Destroy runs before the GameObject is destroyed
    private void OnDestroy()
    {
        // Notifies event broker the enemy has been destroyed.
        EventBroker.CallEnemyDestroyed(assignedReference);
    }

    #endregion

    #region Custom Methods

    // Transitions to the state indicated in the state machine
    protected void TransitionToState(EnemyBaseState state)
    {
        currentState = state;
        // Runs "start" method of the individual state
        currentState.EnterState(this);
    }

    #endregion
}

#region Enum

// Enum containing the types of enemies
public enum EnemyTypes
{
    Roller,
    Speeder,
    Smasher,
    Annihilator
}

#endregion