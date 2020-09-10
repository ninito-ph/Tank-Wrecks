using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : TankBase
{
    #region Field Declarations

    private Coroutine OilPowerupRoutine;
    private Coroutine ShieldPowerupRoutine;
    private Coroutine NukePowerupRoutine;

    // Ammunition of the tank
    [SerializeField]
    [Tooltip("Current ammo of the tank.")]
    private int ammo = 25;
    [SerializeField]
    [Tooltip("Maximum ammo of the tank.")]
    private int maxAmmo = 25;

    #region Controls

    // Movement
    private float bodyVerticalMovement;
    private float bodyHorizontalMovement;
    private float headMovement;
    private float cannonMovement;

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

    #endregion

    #endregion

    #region Unity Methods

    // Update runs every frame
    protected override void Update()
    {
        // Calls the base class' update
        base.Update();
        // Updates player input
        UpdatePlayerInput();
        // Checks if player is pressing fire key
        // NOTE: fireCooldown, as a counter, unusually ticks UP instead of down, due to the way UIController functions and uses it.
        if (Input.GetKey(KeyCode.Space) && fireCooldown >= maxFireCooldown)
        {
            TankFire();
        }
    }

    private void Start()
    {
        EventBroker.ActivatePowerup += ActivatePowerup;
    }

    // Fixed update runs on every fixed update. Good for physics. 
    private void FixedUpdate()
    {
        // Moves the tank
        TankBodyMovement();
        TankHeadMovement();
        TankCannonMovement();
    }

    #endregion

    #region Custom Methods

    protected override void TankFire()
    {
        // Checks if ammo is greater than 0
        if (ammo > 0)
        {
            // Calls base fire method
            base.TankFire();
            // Reduces ammo
            ammo--;
            // Notifies that the player has shot
            EventBroker.CallShotFired();
        }
    }

    // Updates player input
    private void UpdatePlayerInput()
    {
        // Get player input for WASD
        bodyVerticalMovement = System.Convert.ToSingle((Input.GetKey(KeyCode.W))) + (System.Convert.ToSingle((Input.GetKey(KeyCode.S))) * -1f);
        bodyHorizontalMovement = System.Convert.ToSingle((Input.GetKey(KeyCode.D))) + (System.Convert.ToSingle((Input.GetKey(KeyCode.A))) * -1f);
        // Get player input for arrow keys
        cannonMovement = System.Convert.ToSingle((Input.GetKey(KeyCode.UpArrow))) + (System.Convert.ToSingle((Input.GetKey(KeyCode.DownArrow))) * -1f);
        headMovement = System.Convert.ToSingle((Input.GetKey(KeyCode.RightArrow))) + (System.Convert.ToSingle((Input.GetKey(KeyCode.LeftArrow))) * -1f);
    }

    // Moves the tank's body accourding to input
    private void TankBodyMovement()
    {
        // Moves the body
        // If the rigidbody's velocity is below max speed
        if (Mathf.Abs(bodyRigidbody.velocity.magnitude) <= maxSpeed)
        {
            // Accelerates rigidbody
            // FIXME: The tank's model X axis is inverted. For this reason, the X component is being multiplied by -1.
            bodyRigidbody.AddRelativeForce(new Vector3(bodyVerticalMovement * acceleration * -1f, 0f, 0f));
        }

        // Turns the body
        if (Mathf.Abs(bodyRigidbody.velocity.magnitude) > 0f)
        {
            // FIXME: The tank turns left faster than it turns right, for some reason
            bodyRigidbody.AddRelativeTorque(new Vector3(0f, bodyHorizontalMovement * bodyTurnRate, 0f));
        }
    }

    // Moves the tank's head accourding to input
    private void TankHeadMovement()
    {
        // Turns the head
        // Stores the absolute value of the desired head rotation
        float desiredHeadRotation = Mathf.Abs(tankParts["Head"].transform.localEulerAngles.y + (headMovement * headTurnRate * Time.deltaTime));

        // Checks if the value of the desired head movement is within the max head turn angle range
        if (desiredHeadRotation <= maxHeadRotation || desiredHeadRotation >= 360 - maxHeadRotation)
        {
            tankParts["Head"].transform.Rotate(new Vector3(0f, headMovement * headTurnRate * Time.deltaTime, 0f), Space.Self);
        }
    }

    // Moves the tank's cannon accourding to input
    private void TankCannonMovement()
    {
        // Inclines the cannon
        // Stores the absolute value of the cannon's inclination
        float desiredCannonInclination = Mathf.Abs(tankParts["Cannon 1"].transform.localEulerAngles.z + (cannonMovement * cannonInclineRate * Time.deltaTime * -1f));

        // If the desired inclination is within the max cannon incline
        if (desiredCannonInclination <= maxCannonInclineAngle.x || desiredCannonInclination >= 360 - maxCannonInclineAngle.y)
        {
            // Inclines the cannon
            // The rotation is multiplied by -1 because we want to rotate the cannon counter-clockwise based on our input
            tankParts["Cannon 1"].transform.Rotate(new Vector3(0f, 0f, cannonMovement * cannonInclineRate * Time.deltaTime * -1f), Space.Self);
        }
        Debug.LogFormat("desiredCannonInclination {0}, currentCannonInclination {1}, cannonInclineDelta {2}", desiredCannonInclination, tankParts["Cannon 1"].transform.localEulerAngles.z, (cannonMovement * cannonInclineRate * Time.deltaTime));
    }

    // Enables a powerup
    private void ActivatePowerup(PowerupTypes powerupTypes, float duration, int powerupAmount, float speedMultiplier)
    {
        switch (powerupTypes)
        {
            // Ammo powerup
            case (PowerupTypes.Ammo):
                Ammo += powerupAmount;
                break;

            // Health powerup
            case (PowerupTypes.Wrench):
                Health += powerupAmount;
                break;

            case (PowerupTypes.OilBarrel):
                OilPowerupRoutine = StartCoroutine("OilPowerup");
                break;

            case (PowerupTypes.Shield):
                ShieldPowerupRoutine = StartCoroutine("ShieldPowerup");
                break;

            case (PowerupTypes.NukeShell):
                NukePowerupRoutine = StartCoroutine("NukePowerup");
                break;
        }
    }

    #region Coroutines

    // Oil powerup coroutine
    private IEnumerator OilPowerup(float duration, float speedMultiplier)
    {
        yield break;
    }

    // Shield powerup coroutine
    private IEnumerator ShieldPowerup(float duration)
    {
        yield break;
    }

    // Shield powerup coroutine
    private IEnumerator NukePowerup(int powerupAmount)
    {
        yield break;
    }

    #endregion

    #endregion

}
