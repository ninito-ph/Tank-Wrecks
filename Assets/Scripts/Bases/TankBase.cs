﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankBase : MonoBehaviour
{
    #region Field Declarations

    #region Core Values

    [Header("Core Values")]
    // Health of the tank
    [SerializeField]
    [Tooltip("Current health of the tank.")]
    protected int health = 3;
    [SerializeField]
    [Tooltip("Maximum health of the tank.")]
    protected int maxHealth = 3;

    // The amount of cannons the tank has
    [SerializeField]
    [Tooltip("The amount of cannons the tank has.")]
    protected int cannonAmount = 1;

    // How long the tank has to wait to fire again
    // fireCooldown is used internally as a counter variable.
    // NOTE: fireCooldown, as a counter, unusually ticks UP instead of down, due to the way UIController functions and uses it.
    protected float fireCooldown = 0f;
    [SerializeField]
    [Tooltip("The amount of time the tank must wait between shots.")]
    protected float maxFireCooldown = 2.5f;

    // Properties
    // properties are being used to preserve encapsulation
    [HideInInspector]
    public int Health
    {
        get { return health; }
        set { health = value; }
    }

    [HideInInspector]
    public int MaxHealth
    {
        get { return maxHealth; }
    }

    [HideInInspector]
    public float FireCooldown
    {
        get { return fireCooldown; }
    }

    [HideInInspector]
    public float MaxFireCooldown
    {
        get { return maxFireCooldown; }
    }

    #endregion

    #region Movement Values

    [Header("Movement values")]
    // Tank part turn/incline speeds.
    [SerializeField]
    [Tooltip("The speed at which the tank turns")]
    protected float bodyTurnRate = 30f;
    [SerializeField]
    [Tooltip("The speed at which the tank's 'head' turns.")]
    protected float headTurnRate = 30f;
    [SerializeField]
    [Tooltip("The speed at which the tank's cannon inclines/declines")]
    protected float cannonInclineRate = 30f;

    // Maximum turn and incline angles for head and cannon
    [SerializeField]
    [Tooltip("The maximum angle the tank's head can turn either direction.")]
    protected float maxHeadRotation = 120f;
    [SerializeField]
    [Tooltip("The maximum angle the tank's cannon can decline or incline (respectively)")]
    protected Vector2 maxCannonInclineAngle = new Vector2(20f, -50f);

    // The maximum speed and acceleration a tank may have/has
    [SerializeField]
    [Tooltip("The maximum speed at which the tank can move.")]
    protected float maxSpeed = 5f;
    [SerializeField]
    [Tooltip("The acceleration of the tank's movement.")]
    protected float acceleration = 3f;

    // Internal variables to keep track of the head's/cannon's current angle
    protected float headAngle = 0f;
    protected float cannonAngle = 0f;

    // Internal references to the head and body's rigidbody
    protected Rigidbody headRigidbody;
    protected Rigidbody bodyRigidbody;
    protected Rigidbody cannonRigidbody;

    [Header("Recoil Values")]
    [SerializeField]
    // The recoil amount applied to different parts of the tank
    [Tooltip("The recoil applied to the tank's body when it fires.")]
    protected float shotRecoil = 3f;
    [SerializeField]
    [Tooltip("The recoil of the tank's cannon when it fires.")]
    protected AnimationCurve shotRecoilCannon;
    [SerializeField]
    [Tooltip("The radius of the small explosion when a shot is fired")]
    protected float shotRecoilRadius = 10f;
    [SerializeField]
    [Tooltip("The upwards force applied to the bodies within the firing explosion radius")]
    protected float shotUpwardRecoil = 5f;

    #endregion

    #region Additional Values

    [Header("Additional Values")]
    // Other values needed for script functionality.
    
    
    [SerializeField]
    [Tooltip("The tankShell which the tank fires.")]
    protected GameObject tankShell;
    [SerializeField]
    [Tooltip("A list containing, respectively, body, head, cannon, cannon anchor, fire transform, fire transform 2, fire transform 3, cannon 2 and cannon 3 references. ATTENTION! Must be in the aforementioned order!")]
    protected List<GameObject> tankPartList = new List<GameObject>();
    // [SerializeField] Unfortunately, generic dictionaries are not serializable within Unity. We work around this by adding entries to the dictionary using a list.
    // [Tooltip("A dictionary containing all of the tank's parts.")]
    protected Dictionary<string, GameObject> tankParts = new Dictionary<string, GameObject>();

    // Properties
    // properties are being used to preserve encapsulation
    [HideInInspector]
    public GameObject FireProjectile
    {
        get { return tankShell; }
    }

    #endregion

    #endregion

    #region Unity Methods

    protected virtual void Awake()
    {
        // Populates dictionary with tankPartList entries
        tankParts.Add("Body", tankPartList[0]);
        tankParts.Add("Head", tankPartList[1]);
        tankParts.Add("Cannon 1", tankPartList[2]);
        tankParts.Add("Cannon Anchor", tankPartList[3]);
        tankParts.Add("Fire Transform 1", tankPartList[4]);

        // Populate the list with optional elements, if they exist
        if (tankPartList.Count >= 6)
        {
            tankParts.Add("Fire Transform 2", tankPartList[5]);
        }
        
        if (tankPartList.Count >= 7)
        {
            tankParts.Add("Fire Transform 3", tankPartList[6]);
        }
        
        if (tankPartList.Count >= 8)
        {
            tankParts.Add("Cannon 2", tankPartList[7]);
        }
        
        if (tankPartList.Count >= 9)
        {
            tankParts.Add("Cannon 3", tankPartList[8]);
        }

        // Gets references for the body rigidbody
        bodyRigidbody = tankParts["Body"].GetComponent<Rigidbody>();
    }

    protected virtual void Update()
    {
        // Ticks fire cooldown up
        fireCooldown++;
    }

    #endregion

    #region Custom Methods

    // Destroys the tank in a big explosion
    protected void DestroyTank()
    {
        // TODO: Add explosion VFX
        Destroy(this.gameObject);
    }

    // Fires the tank's cannon
    protected virtual void TankFire()
    {
        // Loops through the existing cannons up to the cannon amount, and fires once for every cannon
        for (int currentCannon = 1; currentCannon <= cannonAmount; currentCannon++)
        {
            // Creates shell
            CreateProjectile(currentCannon, currentCannon, tankShell);

            // Apply recoil to tank body
            string fireTransformKey = "Fire Transform " + currentCannon.ToString();
            bodyRigidbody.AddExplosionForce(shotRecoil, tankParts[fireTransformKey].transform.position, shotRecoilRadius, shotUpwardRecoil, ForceMode.Force);
        }
        // Activate cooldown and remove ammo
        fireCooldown = 0;
    }

    // Deduces an amount from the player's health
    protected void TakeDamage(int damageAmount)
    {
        // Subtract the health. If it is lesser than 0, destroy the tank.
        health = health - damageAmount;
        if (health <= 0)
        {
            DestroyTank();
        }
    }

    // Creates a tankShell shell
    protected GameObject CreateProjectile(int fireTransformNumber, int cannonNumber, GameObject tankShellToFire)
    {
        // These Key strings are one-based
        string fireTransformKey = "Fire Transform " + fireTransformNumber.ToString();
        string cannonKey = "Cannon " + cannonNumber.ToString();

        // Gets references for the cannon anchor and fire transform
        GameObject cannonAnchor = tankParts["Cannon Anchor"];
        GameObject fireTransform = tankParts[fireTransformKey];

        // Creates new Vector3 values for when the fired Projectile is created
        Vector3 ProjectileOrigin = new Vector3(fireTransform.transform.position.x, fireTransform.transform.position.y, fireTransform.transform.position.z);
        Vector3 ProjectileRotation = new Vector3(cannonAnchor.transform.rotation.eulerAngles.x, fireTransform.transform.rotation.eulerAngles.y, cannonAnchor.transform.rotation.eulerAngles.z + 90);

        // Create a tankShell and add the cannon who fired it to the collision ignore list (to prevent shells from exploding in the cannon that fired them)
        GameObject firedTankShell = Instantiate(tankShell, ProjectileOrigin, Quaternion.Euler(ProjectileRotation));
        ProjectileController firedTankShellController = firedTankShell.GetComponent<ProjectileController>();
        firedTankShellController.FiredFrom = tankParts[cannonKey];

        // Returns a reference to the fired tankShell if needed.
        return firedTankShell;
    }

    #endregion
}
