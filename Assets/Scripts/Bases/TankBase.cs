using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [SerializeField]
    [Tooltip("The maximum speed at which a tank can move.")]
    protected float maxSpeed = 5f;

    // The amount of cannons the tank has
    [SerializeField]
    [Tooltip("The amount of cannons the tank has.")]
    protected int cannonAmount = 1;

    // How long the tank has to wait to fire again fireCooldown is used
    // internally as a counter variable. NOTE: fireCooldown, as a counter,
    // unusually ticks UP instead of down, due to the way HUDController
    // functions and uses it.
    protected float fireCooldown = 0f;
    [SerializeField]
    [Tooltip("The amount of time the tank must wait between shots.")]
    protected float maxFireCooldown = 2.5f;

    #endregion

    #region Audio Values

    [Header("Audio Values")]
    [SerializeField]
    [Tooltip("The audio clips for cannon fire")]
    protected AudioClip[] cannonFireSFX;
    // The explosion clip
    [SerializeField]
    [Tooltip("The death explosion sound of the tank")]
    protected AudioClip deathExplosionSound;

    // The audio source for the tank engine
    protected AudioSource engineSoundSource;
    // Uses an array of sound sources for cannons, so that it is compatible with multiple cannons
    protected AudioSource[] cannonSoundSources;
    // Rigidbody used to determine velocity
    protected Rigidbody bodyRigidbody;

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

    [Header("Visual Effect Values")]
    [Tooltip("The explosion effect when firing from cannon")]
    [SerializeField]
    protected GameObject cannonExplosion;
    [Tooltip("The explosion effect when the tank is destroyed")]
    [SerializeField]
    protected GameObject deathExplosion;

    // Disintegration coroutine
    protected Coroutine disintegrateRoutine;

    // Properties
    // properties are being used to preserve encapsulation
    public GameObject FireProjectile
    {
        get { return tankShell; }
    }

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

    public int MaxHealth
    {
        get { return maxHealth; }
    }

    public float FireCooldown
    {
        get { return fireCooldown; }
        set { fireCooldown = value; }
    }

    public float MaxFireCooldown
    {
        get { return maxFireCooldown; }
    }

    public ProjectileLaunchData LaunchData
    {
        get
        {
            // Retrieves the launch angle and launch height
            float launchAngle = TankParts["Cannon 1"].transform.eulerAngles.x;
            float launchHeight = TankParts["Fire Transform 1"].transform.position.y;

            // Calculates the forces of the launch
            Vector3 launchVelocity = tankParts["Fire Transform 1"].transform.forward * fireForce;

            // Stores information into the launch data struct
            ProjectileLaunchData launchInfo = new ProjectileLaunchData(launchVelocity, Physics.gravity.magnitude, launchAngle, launchHeight);

            // Returns the info
            return launchInfo;
        }
    }

    public Dictionary<string, GameObject> TankParts { get => tankParts; }

    public int CannonAmount { get => cannonAmount; }

    #endregion

    #endregion

    #region Unity Methods

    protected virtual void Awake()
    {
        // Initializes cannon sound source array
        cannonSoundSources = new AudioSource[CannonAmount];

        // Caches reference for the body rigidbody
        bodyRigidbody = gameObject.GetComponent<Rigidbody>();

        // Caches references to the audio sources on the body and the cannon
        engineSoundSource = gameObject.GetComponent<AudioSource>();

        // Caches references to cannon sound sources
        cannonSoundSources[0] = TankParts["Cannon 1"].GetComponent<AudioSource>();

        // Checks if a second cannon exists
        if (TankParts.ContainsKey("Cannon 2"))
        {
            // Adds it to sound sources
            cannonSoundSources[1] = TankParts["Cannon 2"].GetComponent<AudioSource>();
        }

        // Checks if a third cannon exists
        if (TankParts.ContainsKey("Cannon 3"))
        {
            // Adds it to sound sources
            cannonSoundSources[2] = TankParts["Cannon 3"].GetComponent<AudioSource>();
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
        fireCooldown += Time.deltaTime;

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
        // Plays death explosion effect
        Instantiate(deathExplosion, transform.position, Quaternion.identity);
        // Gets all mesh renderer materials
        List<Material> tankMaterials = new List<Material>();
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            tankMaterials.Add(renderer.material);
        }
        // Starts burn coroutine
        disintegrateRoutine = StartCoroutine(Disintegration(0.75f, 2f, tankMaterials));
        Destroy(gameObject, 0.75f);
    }

    // Fires the tank's cannon
    public virtual void TankFire()
    {
        // Loops through the existing cannons up to the cannon amount, and fires once for every cannon
        for (int currentCannon = 1; currentCannon <= CannonAmount; currentCannon++)
        {
            // Creates shell
            CreateProjectile(currentCannon, currentCannon, tankShell);

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
        GameObject fireTransform = TankParts[fireTransformKey];

        // Creates new Vector3 values for when the fired Projectile is created
        Vector3 ProjectileOrigin = new Vector3(fireTransform.transform.position.x, fireTransform.transform.position.y, fireTransform.transform.position.z);
        Vector3 ProjectileRotation = new Vector3(fireTransform.transform.rotation.eulerAngles.x + 90f, fireTransform.transform.rotation.eulerAngles.y, fireTransform.transform.rotation.eulerAngles.z);

        // Create a tankShell and add the cannon who fired it to the collision ignore list (to prevent shells from exploding in the cannon that fired them)
        GameObject firedTankShell = Instantiate(tankShellToFire, ProjectileOrigin, Quaternion.Euler(ProjectileRotation));
        ProjectileController firedTankShellController = firedTankShell.GetComponent<ProjectileController>();
        firedTankShellController.FiredFrom = TankParts[cannonKey];

        // Adds impulse to fired projectile
        firedTankShellController.ProjectileImpulse = fireForce;

        // Returns a reference to the fired tankShell if needed.
        return firedTankShell;
    }

    #region Coroutines

    // Controls how much the object is disintegrating through the material shader
    protected IEnumerator Disintegration(float disintegrationTime, float disintegrationTarget, List<Material> disintegrationMaterials)
    {
        // Gets the current disintegration amount
        float startingDisintegration = disintegrationMaterials[0].GetFloat("_DisAmount");

        // Updates text color until fade duration is over
        for (float time = 0; time <= disintegrationTime; time += Time.deltaTime)
        {
            // Calculates the disintegration amount
            float disintegrationAmount = Mathf.Lerp(startingDisintegration, Mathf.Clamp(disintegrationTarget, -2f, 2f), time / disintegrationTime);

            // Applies the disintegration amount in all tank materials
            foreach (Material material in disintegrationMaterials)
            {
                material.SetFloat("_DisAmount", disintegrationAmount);
            }

            // Waits for the next frame
            yield return null;
        }

        // Clears coroutine
        disintegrateRoutine = null;
    }

    #endregion

    #endregion
}
