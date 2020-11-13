using System.Collections;
using UnityEngine;

public class PlayerController : TankBase
{
    #region Field Declarations

    // Ammunition of the tank
    private int ammo;

    [SerializeField]
    [Tooltip("Maximum ammo of the tank.")]
    private int maxAmmo = 25;

    [Header("Movement values")]
    // Tank part turn/incline speeds.
    [SerializeField]
    [Tooltip("The speed at which the tank turns")]
    private float bodyTurnRate = 30f;
    [SerializeField]
    [Tooltip("The speed at which the tank's 'head' turns.")]
    private float headTurnRate = 30f;
    [SerializeField]
    [Tooltip("The speed at which the tank's cannon inclines/declines")]
    private float cannonInclineRate = 30f;

    // Maximum turn and incline angles for head and cannon
    [SerializeField]
    [Tooltip("The maximum angle the tank's head can turn either direction.")]
    private float maxHeadRotation = 120f;
    [SerializeField]
    [Tooltip("The maximum angle the tank's cannon can decline or incline (respectively)")]
    private Vector2 maxCannonInclineAngle = new Vector2(20f, -50f);

    [SerializeField]
    [Tooltip("The acceleration of the tank's movement.")]
    private float acceleration = 3f;

    // Internal variables to keep track of the head's/cannon's current angle
    protected float headAngle = 0f;
    protected float cannonAngle = 0f;

    #region Controls

    // Movement
    // The movement vectors are declared in the start and only modified later,
    // to minimize heap memory usage.
    private Vector3 bodyFrontalMovement = new Vector3(0f, 0f, 0f);
    private Vector3 bodyLateralMovement = new Vector3(0f, 0f, 0f);
    private Vector3 headMovement = new Vector3(0f, 0f, 0f);
    private Vector3 cannonMovement = new Vector3(0f, 0f, 0f);

    #endregion
    #region Powerups

    // Powerup coroutines
    private Coroutine OilPowerupRoutine;
    private Coroutine ShieldPowerupRoutine;
    private Coroutine NukePowerupRoutine;

    [Header("Powerups")]
    [SerializeField]
    [Tooltip("The prefab for the shield powerup dome")]
    private GameObject forceFieldDome;
    [SerializeField]
    [Tooltip("The prefab for the nuke shell powerup")]
    private GameObject nukeShell;

    // The forcefield powerup instance
    private GameObject forceField;
    // The ammo for nuke shells
    private int nukeShellAmmo = 0;

    [Header("Other Values")]
    [SerializeField]
    [Tooltip("The reload sound of the cannon")]
    private AudioClip reloadClip;

    // The trajectory controller reference and coroutine
    private TrajectoryController trajectoryController;
    private Coroutine giveLaunchDataRoutine;

    #endregion
    #region AI interface

    [Header("AI Interface")]
    [SerializeField]
    [Tooltip("The radius of the fire area. Enemy AI will stop aiming and reposition if they leave this area. Shown in yellow. The safe fire area is shown in green.")]
    private float fireAreaRadius = 10f;
    [SerializeField]
    [Tooltip("Whether to show fire areas in the scene view")]
    private bool showFireAreas = false;

    // Hidden areas (areas that are based on the radius of the fire area)
    private float safeFireAreaRadius;
    private float rushFireAreaRadius;

    #endregion

    #region Properties

    public int Ammo
    {
        get { return ammo; }
        set
        {
            if (ammo + value > maxAmmo)
            {
                ammo = maxAmmo;
            }
            else
            {
                ammo += value;
            }
        }
    }

    public float FireAreaRadius
    {
        get { return fireAreaRadius; }
    }

    public float SafeFireAreaRadius
    {
        get { return safeFireAreaRadius; }
    }

    public float RushAreaFireRadius
    {
        get { return rushFireAreaRadius; }
    }

    #endregion

    #endregion

    #region Unity Methods

    protected override void Awake()
    {
        // Calls the base's awake method
        base.Awake();
    }

    // Update runs every frame
    protected override void Update()
    {
        // Calls the base class' update
        base.Update();

        // Checks if player is pressing fire key, if the cooldown is off and the game is unpaused
        // NOTE: fireCooldown, as a counter, unusually ticks UP instead of down, due to the way HUDController functions and uses it.
        if (Input.GetKeyDown(KeyCode.Space) && fireCooldown >= maxFireCooldown && Time.timeScale > 0f)
        {
            TankFire();
        }
    }

    protected override void Start()
    {
        // Calls the base's start method
        base.Start();

        // Caches the trajectory controller and begins giving it projectile launch data
        trajectoryController = GetComponentInChildren<TrajectoryController>();

        // Sets the radius of the other fire areas
        safeFireAreaRadius = fireAreaRadius * 0.75f;
        rushFireAreaRadius = fireAreaRadius * 0.33f;

        // Sets ammo to max ammo
        ammo = maxAmmo;

        // Subscribes ActivatePowerup method to ActivatePowerup event
        EventBroker.ActivatePowerup += ActivatePowerup;

        // Creates and disables the shield powerup for future use
        forceField = Instantiate(forceFieldDome, transform.position, Quaternion.identity);
        forceField.SetActive(false);
    }

    // Fixed update runs on every fixed update. Good for physics. 
    private void FixedUpdate()
    {
        TankBodyMovement();
        TankHeadMovement();
        TankCannonMovement();

        // Gives trajectory controller launch data to calculate trajectory
        trajectoryController.LaunchData = LaunchData;
    }

    // OnDestroy runs once gameobject is destroyed
    private void OnDestroy()
    {
        // Unsubscribes from event to prevent memory leaks and odd behaviour
        EventBroker.ActivatePowerup -= ActivatePowerup;
    }

    // Runs when the editor draws scene gizmos
    private void OnDrawGizmos()
    {
        // If showing fire areas has been enabled
        if (showFireAreas == true)
        {
            // Draws a yellow sphere to show the fire area
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, fireAreaRadius);

            // Draws a green sphere to show the safe fire area
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, fireAreaRadius * 0.75f);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, fireAreaRadius * 0.33f);
        }
    }

    #endregion

    #region Custom Methods

    // Fires a tank cannon
    public override void TankFire()
    {
        // Checks if ammo is greater than 0
        if (ammo > 0 || nukeShellAmmo > 0)
        {
            // Loops through the existing cannons up to the cannon amount, and fires once for every cannon
            for (int currentCannon = 1; currentCannon <= CannonAmount; currentCannon++)
            {
                // Creates shell
                // If the player has nuke shells, fire them instead
                if (nukeShellAmmo > 0)
                {
                    CreateProjectile(currentCannon, currentCannon, nukeShell);
                    // Reduces nuke shell ammo
                    nukeShellAmmo--;
                }
                else
                {
                    CreateProjectile(currentCannon, currentCannon, tankShell);
                    // Reduces ammo
                    ammo--;
                }

                // Apply recoil to tank body
                string fireTransformKey = "Fire Transform " + currentCannon.ToString();

                // Plays cannon fire explosion
                Instantiate(cannonExplosion, tankParts[fireTransformKey].transform.position, Quaternion.identity);

                // Sound effects
                // Picks a random fire cannon sound effect from the array and assigns it to the sound source
                cannonSoundSources[currentCannon - 1].clip = cannonFireSFX[Random.Range(0, cannonFireSFX.Length)];
                // Randomizes pitch to increase sound variance
                cannonSoundSources[currentCannon - 1].pitch = Random.Range(1.2f, 1.5f);
                // Makes the sound source play its sound
                cannonSoundSources[currentCannon - 1].Play();
            }

            StartCoroutine(PlayReloadSound(maxFireCooldown / 2.2f));

            // Activate cooldown and remove ammo
            fireCooldown = 0;

            // Notifies that the player has shot
            EventBroker.CallShotFired();
        }
    }

    // Moves the tank's body accourding to input
    private void TankBodyMovement()
    {
        // Moves the body
        // If the magnitude of the velocity is lesser than the max speed, and the speed is not 0
        // Uses approximately to check if there is any input due to floating point innacuraccy
        if (Mathf.Abs(bodyRigidbody.velocity.magnitude) <= maxSpeed && !Mathf.Approximately(Input.GetAxis("Vertical"), 0f))
        {
            // Sets the movement vector instead of creating a new one, to reduce heap memory usage (doesn't actually make a difference, Vector3 is a struct anyways)
            bodyFrontalMovement.Set(0f, 0f, Input.GetAxis("Vertical") * acceleration * -1f);

            // Accelerates rigidbody
            bodyRigidbody.AddRelativeForce(bodyFrontalMovement);
        }

        // Turns the body
        // Uses approximately to check if there is any input due to floating point innacuraccy
        if (Mathf.Abs(bodyRigidbody.velocity.z) > 0.001f && !Mathf.Approximately(Input.GetAxis("Horizontal"), 0f))
        {
            // Sets the movement vector instead of creating a new one, to reduce heap memory usage (doesn't actually make a difference, Vector3 is a struct anyways)
            bodyLateralMovement.Set(0f, Input.GetAxis("Horizontal") * bodyTurnRate * (Mathf.Clamp01(bodyRigidbody.velocity.magnitude / maxSpeed)) * Time.deltaTime, 0f);

            // Rotates the tank based on the direction the player is pressing. When in reverse, the directions are inverted.
            TankParts["Body"].transform.Rotate(bodyLateralMovement);
        }
    }

    // Moves the tank's head accourding to input
    private void TankHeadMovement()
    {
        // Turns the head
        // Stores the absolute value of the desired head rotation
        float desiredHeadRotation = Mathf.Abs(TankParts["Head"].transform.localEulerAngles.y + (Input.GetAxis("HorizontalSecondary") * headTurnRate * Time.deltaTime));

        // Checks if the value of the desired head movement is within the max head turn angle range
        if (desiredHeadRotation <= maxHeadRotation || desiredHeadRotation >= 360 - maxHeadRotation)
        {
            // Sets the movement vector instead of creating a new one, to reduce heap memory usage
            headMovement.Set(0f, Input.GetAxis("HorizontalSecondary") * headTurnRate * Time.deltaTime, 0f);

            TankParts["Head"].transform.Rotate(headMovement, Space.Self);
        }
    }

    // Moves the tank's cannon accourding to input
    private void TankCannonMovement()
    {
        // Inclines the cannon
        // Stores the absolute value of the cannon's inclination
        float desiredCannonInclination = Mathf.Abs(TankParts["Cannon 1"].transform.localEulerAngles.x + (Input.GetAxis("VerticalSecondary") * cannonInclineRate * Time.deltaTime * -1f));

        // If the desired inclination is within the max cannon incline
        if (desiredCannonInclination <= maxCannonInclineAngle.x || desiredCannonInclination >= 360 - maxCannonInclineAngle.y)
        {
            // Sets the movement vector instead of creating a new one, to reduce heap memory usage
            cannonMovement.Set(Input.GetAxis("VerticalSecondary") * cannonInclineRate * Time.deltaTime * -1f, 0f, 0f);

            // Inclines the cannon
            // The rotation is multiplied by -1 because we want to rotate the cannon counter-clockwise based on our input
            TankParts["Cannon 1"].transform.Rotate(cannonMovement, Space.Self);
        }
    }

    // Enables a powerup
    private void ActivatePowerup(PowerupTypes powerupTypes, float duration, int powerupAmount, float speedMultiplier)
    {
        switch (powerupTypes)
        {
            // Ammo powerup
            case (PowerupTypes.Ammo):
                if (Ammo + powerupAmount > maxAmmo)
                {
                    Ammo = maxAmmo;
                    // Calls the shotFired event to make the UI update its ammo count
                    EventBroker.CallShotFired();
                }
                else
                {
                    Ammo += powerupAmount;
                    // Calls the shotFired event to make the UI update its ammo count
                    EventBroker.CallShotFired();
                }

                break;

            // Health powerup
            case (PowerupTypes.Wrench):
                if (Health + powerupAmount > maxHealth)
                {
                    Health = maxHealth;
                }
                else
                {
                    Health += powerupAmount;
                }
                break;

            // Speed powerup
            case (PowerupTypes.OilBarrel):
                if (OilPowerupRoutine == null)
                {
                    OilPowerupRoutine = StartCoroutine(OilPowerup(duration, speedMultiplier));
                }
                break;

            // Shield powerup
            case (PowerupTypes.Shield):
                ShieldPowerupRoutine = StartCoroutine(ShieldPowerup(duration));
                break;

            // Nuke powerup
            case (PowerupTypes.NukeShell):
                nukeShellAmmo++;
                break;
        }
    }

    protected override void DestroyTank()
    {
        // Calls the player destroyed event
        EventBroker.CallPlayerDestroyed();

        // Calls the base's method
        base.DestroyTank();
    }

    #region Coroutines

    // Oil powerup coroutine
    private IEnumerator OilPowerup(float duration, float speedMultiplier)
    {
        // TODO: add a way for the UI to tell how long there is left on a powerup

        // Multiplies max speed and acceleration
        maxSpeed = maxSpeed * speedMultiplier;
        acceleration = acceleration * speedMultiplier;
        bodyTurnRate = bodyTurnRate * speedMultiplier;

        // Waits until powerup time is over
        yield return new WaitForSeconds(duration);

        // Undoes multipliers on speed and acceleration
        maxSpeed = maxSpeed / speedMultiplier;
        acceleration = acceleration / speedMultiplier;
        bodyTurnRate = bodyTurnRate / speedMultiplier;

        // Marks coroutine as null so as to indicate powerup is inactive
        OilPowerupRoutine = null;

        // Ends coroutine
        yield break;
    }

    // Shield powerup coroutine
    private IEnumerator ShieldPowerup(float duration)
    {
        // Enables the powerup
        forceField.transform.SetPositionAndRotation(transform.position, Quaternion.identity);
        forceField.SetActive(true);

        // Waits until powerup time is over
        yield return new WaitForSeconds(duration);

        // Disables the powerup
        forceField.SetActive(false);

        //  Marks coroutine as null so as to indicate powerup is inactive
        ShieldPowerupRoutine = null;

        // Ends coroutine
        yield break;
    }

    // Reload sound coroutine
    private IEnumerator PlayReloadSound(float reloadDelay)
    {
        // Waits the reload delay
        yield return new WaitForSeconds(reloadDelay);

        // Randomizes pitch
        cannonSoundSources[0].pitch = Random.Range(1.2f, 1.3f);
        // Sets clip to reload clip
        cannonSoundSources[0].clip = reloadClip;
        // Plays reload sound
        cannonSoundSources[0].Play();

        // Ends coroutine
        yield break;
    }

    #endregion

    #endregion
}
