using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : TankBase
{
    #region Field Declarations

    #region Core Values

    [Header("Other values")]
    [SerializeField]
    [Tooltip("The speed at which the tank's head and cannon, repsectively, aim at the player.")]
    private Vector2 aimSpeeds = new Vector2(1.1f, 0.65f);

    // Basic variables for the functioning of the class
    protected GameObject assignedReference;
    protected PlayerController playerReference;

    // Variables used for state machine functionality
    protected EnemyBaseState currentState;
    protected RepositionState repositionState = new RepositionState();
    protected FireState fireState = new FireState();

    [SerializeField]
    [Tooltip("The reward in score for defeating the enemy")]
    private float scoreReward;

    // AI variables
    private bool isStationary = false;
    protected NavMeshAgent navigationAgent;
    protected NavMeshObstacle navigationObstacle;

    [Header("Debug")]
    [Tooltip("Whether or not to draw AI logic to scene view")]
    [SerializeField]
    private bool aiDebugMode;

    #endregion

    #region Properties

    // Properties to preserve encapsulation while still allowing other classes to alter and retrieve values
    public PlayerController PlayerReference
    {
        get { return playerReference; }
        set { playerReference = value; }
    }

    public GameObject AssignedReference
    {
        get { return assignedReference; }
        set { assignedReference = value; }
    }

    public NavMeshAgent NavigationAgent { get => navigationAgent; }

    public Vector2 AimSpeeds { get => aimSpeeds; }

    // Accessor for enabling and disabling agent/obstacle to improve obstacle avoidance
    public bool IsStationary
    {
        get { return isStationary; }
        set
        {
            if (value == false)
            {
                navigationObstacle.enabled = false;
                navigationAgent.enabled = true;
            }
            else
            {
                navigationAgent.enabled = false;
                navigationObstacle.enabled = true;
            }
        }
    }

    public bool AIDebugMode
    {
        get { return aiDebugMode; }
    }

    // Accessors for the machine states
    public EnemyBaseState CurrentState { get { return currentState; } }
    public EnemyBaseState RepositionState { get { return repositionState; } }
    public EnemyBaseState FireState { get { return fireState; } }


    #endregion

    #region Actions

    // Actions to notify other classes of noteworthy events
    private Action<EnemyController> EnemyDestroyed;

    #endregion

    #endregion

    #region Unity Methods

    protected override void Start()
    {
        // Caches the navmesh agent and obstacle reference
        navigationAgent = gameObject.GetComponent<NavMeshAgent>();
        navigationObstacle = gameObject.GetComponent<NavMeshObstacle>();

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
    }

    // Destroy runs before the GameObject is destroyed
    private void OnDestroy()
    {
        // Awards the player with points
        EventBroker.CallAddScore(scoreReward);
    }

    // On draws gizmos runs whenever the editor renders gizmos on the scene view
    private void OnDrawGizmos()
    {
        // If show pathing destinations is true and the current state is not null
        if (aiDebugMode == true && currentState != null)
        {
            currentState.OnDrawGizmos(this);
        }

    }

    #endregion

    #region Custom Methods

    // Transitions to the state indicated in the state machine
    public void TransitionToState(EnemyBaseState state)
    {
        // Runs exit method of the current state isn't null
        if (currentState != null)
        {
            currentState.LeaveState(this);
        }

        // Changes the current state
        currentState = state;

        // Runs "start" method of the new current state
        currentState.EnterState(this);
    }

    #endregion
}