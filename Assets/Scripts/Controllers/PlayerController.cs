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
    private float headHorizontalMovement;
    private float cannonVerticalMovement;

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
        cannonVerticalMovement = System.Convert.ToSingle((Input.GetKey(KeyCode.UpArrow))) + (System.Convert.ToSingle((Input.GetKey(KeyCode.DownArrow))) * -1f);
        headHorizontalMovement = System.Convert.ToSingle((Input.GetKey(KeyCode.RightArrow))) + (System.Convert.ToSingle((Input.GetKey(KeyCode.LeftArrow))) * -1f);
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
        // If the head is within its turn range
        if (Mathf.Abs(tankParts["Head"].transform.eulerAngles.y) <= maxHeadTurnAngle)
        {
            headRigidbody.MoveRotation(Quaternion.Euler(0f, headHorizontalMovement * headTurnRate, 0f));
        }
    }

    // Moves the tank's cannon accourding to input
    private void TankCannonMovement()
    {
        // Inclines the cannon
        // If the cannon is within its incline range
        if (tankParts["Cannon 1"].transform.eulerAngles.z <= maxCannonInclineAngle.x && tankParts["Cannon 1"].transform.eulerAngles.z >= maxCannonInclineAngle.y)
        {
            // Turns rigidbody
            // FIXME: The tank's model X axis is inverted. For this reason, the X component is being multiplied by -1.
            cannonRigidbody.AddRelativeTorque(new Vector3(0f, 0f, cannonVerticalMovement * cannonInclineRate * -1f));
            Debug.LogFormat("canVertMov {0}, canInclRat {1}", cannonVerticalMovement, cannonInclineRate);
        }
    }

    #endregion

}
