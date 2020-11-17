using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyController : TankBase
{
    #region Field Declarations

    #region Core Values

    [Header("Other values")]
    [SerializeField]
    [Tooltip("The speed at which the tank's head and cannon, repsectively, aim at the player.")]
    private Vector2 aimSpeeds = new Vector2(1.1f, 0.65f);

    // Basic variables for the functioning of the class
    private GameObject assignedReference;
    private PlayerController playerReference;

    // Variables used for state machine functionality
    private EnemyBaseState currentState;
    private RepositionState repositionState = new RepositionState();
    private FireState fireState = new FireState();

    [SerializeField]
    [Tooltip("The reward in score for defeating the enemy")]
    private float scoreReward;

    // Visual effect variables
    private List<Material> tankMaterials = new List<Material>();

    // AI variables
    private bool isStationary = false;
    private NavMeshAgent navigationAgent;
    private NavMeshObstacle navigationObstacle;
    private Coroutine currentStateRoutine;

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

    public Coroutine CurrentStateRoutine { get => currentStateRoutine; set => currentStateRoutine = value; }


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

        // Gets all mesh renderer materials
        tankMaterials = new List<Material>();
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            tankMaterials.Add(renderer.material);
            // Also sets the disentegration amount to full and the border color
            // to blue, to allow the tank to have a materialization effect when
            // spawning
            renderer.material.SetFloat("_DisAmount", 2f);
            renderer.material.SetColor("_EdgeColor", new Color(0f, 0.5f, 1f, 1f));
        }

        // Materializes the enemy tank using the disintegration coroutine
        disintegrateRoutine = StartCoroutine(Disintegration(0.75f, -2f, tankMaterials));

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
        // Notifies the enemy was destroyed
        EventBroker.CallEnemyDestroyed(gameObject);
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

    protected override void DestroyTank()
    {
        // Stops current AI coroutine
        StopCoroutine(currentStateRoutine);

        // Plays death explosion effect
        Instantiate(deathExplosion, transform.position, Quaternion.identity);

        // Sets color of all materials to explosion orange
        foreach (Material material in tankMaterials)
        {
            material.SetColor("_EdgeColor", new Color(1f, 0.3f, 0f, 1f));
        }

        // Starts burn coroutine
        disintegrateRoutine = StartCoroutine(Disintegration(0.75f, 2f, tankMaterials));
        Destroy(gameObject, 0.75f);
    }

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