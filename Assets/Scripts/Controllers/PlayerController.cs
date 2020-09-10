using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : TankBase
{
    #region Field Declarations

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

    // Powerups
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
    private int nukeShellAmmo = 0;

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
        // Subscribes ActivatePowerup method to ActivatePowerup event
        EventBroker.ActivatePowerup += ActivatePowerup;

        // Creates and disables the shield powerup for future use
        GameObject forceField = Instantiate(forceFieldDome, transform.position, Quaternion.identity);
        forceField.SetActive(false);
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
            // Loops through the existing cannons up to the cannon amount, and fires once for every cannon
            for (int currentCannon = 1; currentCannon <= cannonAmount; currentCannon++)
            {
                // Creates shell
                // If the player has nuke shells, fire them instead
                if (nukeShellAmmo > 0)
                {
                    CreateProjectile(currentCannon, currentCannon, nukeShell);
                    nukeShellAmmo--;
                }
                else
                {
                    CreateProjectile(currentCannon, currentCannon, tankShell);
                }

                // Apply recoil to tank body
                string fireTransformKey = "Fire Transform " + currentCannon.ToString();
                bodyRigidbody.AddExplosionForce(shotRecoil, tankParts[fireTransformKey].transform.position, shotRecoilRadius, shotUpwardRecoil, ForceMode.Force);
            }

            // Activate cooldown and remove ammo
            fireCooldown = 0;

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

            // Speed powerup
            case (PowerupTypes.OilBarrel):
                OilPowerupRoutine = StartCoroutine(OilPowerup(duration, speedMultiplier));
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

    #region Coroutines

    // Oil powerup coroutine
    private IEnumerator OilPowerup(float duration, float speedMultiplier)
    {
        // TODO: add a way for the UI to tell how long there is left on a powerup

        // Multiplies max speed and acceleration
        maxSpeed = maxSpeed * speedMultiplier;
        acceleration = acceleration * speedMultiplier;

        // Waits until powerup time is over
        yield return new WaitForSeconds(duration);

        // Undoes multipliers on speed and acceleration
        maxSpeed = maxSpeed / speedMultiplier;
        acceleration = acceleration / speedMultiplier;

        // Marks coroutine as null so as to indicate powerup is inactive
        OilPowerupRoutine = null;

        // Ends coroutine
        yield break;
    }

    // Shield powerup coroutine
    private IEnumerator ShieldPowerup(float duration)
    {
        // Enables the powerup
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

    #endregion

    #endregion

}
