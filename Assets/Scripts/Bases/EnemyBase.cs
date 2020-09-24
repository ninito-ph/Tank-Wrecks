using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyBase : TankBase, IEnemy
{
    #region Field Declarations

    #region Core Values

    // Basic variables for the functioning of the class
    protected GameObject assignedReference;
    protected EnemyTypes enemyType;
    protected GameObject playerReference;

    // Variables used for state machine functionality
    protected EnemyBaseState currentState;
    protected RepositionState repositionState = new RepositionState();
    protected FireState fireState = new FireState();
    protected RushState rushState = new RushState();

    [SerializeField]
    [Tooltip("The reward in score for defeating the enemy")]
    private float scoreReward;

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

    /* protected override void Start()
    {
        // Calls the base class start
        base.Start();

        // Transitions to the initial repositioning state
        TransitionToState(repositionState);
    }

    protected override void Update()
    {
        // Calls the base class update
        base.Update();

        // Runs the update from the current state of the machine
        currentState.Update(this);
    } */

    // Destroy runs before the GameObject is destroyed
    private void OnDestroy()
    {
        // Awards the player with points
        EventBroker.CallAddScore(scoreReward);

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