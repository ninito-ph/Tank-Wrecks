﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

// I don't usually enjoy using replacements to monobehaviour, but Odin Inspector is a proven tool used in very large-scale games.
public class TankBase : SerializedMonoBehaviour
{
    #region Field Declarations

    #region Core Values
    
    // Health of the tank
    protected int health;

    [Header("Core Values")]
    [SerializeField]
    [Tooltip("Maximum health of the tank.")]
    protected int maxHealth = 3;

    // The amount of cannons the tank has
    [SerializeField]
    [Tooltip("The amount of cannons the tank has.")]
    protected int cannonAmount = 1;

    // How long the tank has to wait to fire again
    // fireCooldown is used internally as a counter variable.
    // NOTE: fireCooldown, as a counter, unusually ticks UP instead of down, due to the way HUDController functions and uses it.
    protected float fireCooldown = 0f;
    [SerializeField]
    [Tooltip("The amount of time the tank must wait between shots.")]
    protected float maxFireCooldown = 2.5f;

    // The tip used in the loading screen
    [Space]
    [SerializeField]
    [Tooltip("The tooltip displayed during the loading screen for this tank")]
    [Multiline(3)]
    protected string loadingTip;

    // Properties
    // properties are being used to preserve encapsulation
    [HideInInspector]
    public int Health
    {
        get { return health; }
        set
        {
            health = value;
            if (health <= 0)
            {
                DestroyTank();
            }
        }
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

    public string LoadingTip
    {
        get { return loadingTip; }
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

    // Internal reference to the body rigidbody
    protected Rigidbody bodyRigidbody;

    [Header("Recoil Values")]
    [SerializeField]
    // The recoil amount applied to different parts of the tank
    [Tooltip("The recoil applied to the tank's body when it fires.")]
    protected float shotRecoil = 3f;
    [SerializeField]
    [Tooltip("The radius of the small explosion when a shot is fired")]
    protected float shotRecoilRadius = 10f;
    [SerializeField]
    [Tooltip("The upwards force applied to the bodies within the firing explosion radius")]
    protected float shotUpwardRecoil = 5f;

    #endregion

    #region Audio Values

    [Header("Audio Values")]
    [SerializeField]
    [Tooltip("The audio clips for cannon fire")]
    protected AudioClip[] cannonFireSFX;

    // The audio source for the tank engine
    protected AudioSource engineSoundSource;
    // Uses an array of sound sources for cannons, so that it is compatible with multiple cannons
    protected AudioSource[] cannonSoundSources;

    #endregion

    #region Additional Values

    [Header("Additional Values")]
    // Other values needed for script functionality.
    [SerializeField]
    [Tooltip("The tankShell which the tank fires.")]
    protected GameObject tankShell;
    [SerializeField]
    [Tooltip("The strenght (force) at which the projectile is fired")]
    protected float fireForce = 20f;
    [SerializeField]
    [Tooltip("The dictionary containing the Body, Head, Cannon 1, Cannon Anchor, Fire Transform 1, Fire Transform 2, Fire Transform 3, Cannon 2 and Cannon 3 gameObjects. They ")]
    [DictionaryDrawerSettings(KeyLabel = "Tank Part Name", ValueLabel = "Tank Part GameObject")]
    protected Dictionary<string, GameObject> tankParts = new Dictionary<string, GameObject>();

    // Keeps a reference to the GameController object
    protected GameController gameController;

    // Properties
    // properties are being used to preserve encapsulation
    public GameObject FireProjectile
    {
        get { return tankShell; }
    }

    public GameController GameController
    {
        get { return gameController; }
        set { gameController = value; }
    }

    #endregion

    #endregion

    #region Unity Methods

    protected virtual void Awake()
    {
        // Initializes cannon sound source array
        cannonSoundSources = new AudioSource[cannonAmount];

        // Caches reference for the body rigidbody
        bodyRigidbody = tankParts["Body"].GetComponent<Rigidbody>();

        // Caches references to the audio sources on the body and the cannon
        engineSoundSource = tankParts["Body"].GetComponent<AudioSource>();
        Debug.Log(engineSoundSource);

        // Caches references to cannon sound sources
        cannonSoundSources[0] = tankParts["Cannon 1"].GetComponent<AudioSource>();

        // Checks if a second cannon exists
        if (tankParts.ContainsKey("Cannon 2"))
        {
            // Adds it to sound sources
            cannonSoundSources[1] = tankParts["Cannon 2"].GetComponent<AudioSource>();
        }

        // Checks if a third cannon exists
        if (tankParts.ContainsKey("Cannon 3"))
        {
            // Adds it to sound sources
            cannonSoundSources[2] = tankParts["Cannon 3"].GetComponent<AudioSource>();
        }

    }

    protected virtual void Start()
    {
        // Sets health to maxHealth at start
        health = maxHealth;

        // Starts with the player being able to fire
        fireCooldown = maxFireCooldown;
    }

    protected virtual void Update()
    {
        // Ticks fire cooldown up
        fireCooldown += 1 * Time.deltaTime;

        // Updates engine sounds
        UpdateEngineSound();
    }

    #endregion

    #region Custom Methods

    // Updates tank engine sound effect
    protected virtual void UpdateEngineSound()
    {
            // Update engine sound volume
            float engineVolume = Mathf.Clamp01((bodyRigidbody.velocity.magnitude / maxSpeed) + 0.35f);
            if (engineVolume != engineSoundSource.volume)
            {
                engineSoundSource.volume = engineVolume;
            }
    }

    // Destroys the tank in a big explosion
    protected virtual void DestroyTank()
    {
        // TODO: Add explosion VFX
        Destroy(gameObject);

        // Ends all of the tank's associated coroutines
        StopAllCoroutines();
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
            bodyRigidbody.AddExplosionForce(shotRecoil, tankParts[fireTransformKey].transform.position, shotRecoilRadius, shotUpwardRecoil, ForceMode.Impulse);

            // TODO: Add VFX to firing cannon

            // Sound effects
            // Picks a random fire cannon sound effect from the array and assigns it to the sound source
            cannonSoundSources[currentCannon - 1].clip = cannonFireSFX[Random.Range(0, cannonFireSFX.Length)];
            // Randomizes pitch to increase sound variance
            cannonSoundSources[currentCannon - 1].pitch = Random.Range(1.2f, 1.5f);
            // Makes the sound source play its sound
            cannonSoundSources[currentCannon - 1].Play();
        }
        // Activate cooldown and remove ammo
        fireCooldown = 0;
    }

    // Creates a tankShell shell
    protected GameObject CreateProjectile(int fireTransformNumber, int cannonNumber, GameObject tankShellToFire)
    {
        // These Key strings are one-based
        string fireTransformKey = "Fire Transform " + fireTransformNumber.ToString();
        string cannonKey = "Cannon " + cannonNumber.ToString();

        // Gets references for the cannon anchor and fire transform
        GameObject fireTransform = tankParts[fireTransformKey];

        // Creates new Vector3 values for when the fired Projectile is created
        Vector3 ProjectileOrigin = new Vector3(fireTransform.transform.position.x, fireTransform.transform.position.y, fireTransform.transform.position.z);
        Vector3 ProjectileRotation = new Vector3(fireTransform.transform.rotation.eulerAngles.x, fireTransform.transform.rotation.eulerAngles.y, fireTransform.transform.rotation.eulerAngles.z + 90);

        // Create a tankShell and add the cannon who fired it to the collision ignore list (to prevent shells from exploding in the cannon that fired them)
        GameObject firedTankShell = Instantiate(tankShellToFire, ProjectileOrigin, Quaternion.Euler(ProjectileRotation));
        ProjectileController firedTankShellController = firedTankShell.GetComponent<ProjectileController>();
        firedTankShellController.FiredFrom = tankParts[cannonKey];

        // Adds impulse to fired projectile
        firedTankShellController.ProjectileImpulse = fireForce;

        // Returns a reference to the fired tankShell if needed.
        return firedTankShell;
    }

    #endregion
}
