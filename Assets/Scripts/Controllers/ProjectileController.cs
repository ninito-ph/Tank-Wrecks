using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    #region Field Declarations

    #region Core Values

    [Header("Core values")]
    [SerializeField]
    [Tooltip("The damage of the projectile's explosion")]
    private int projectileDamage = 1;
    [Header("Explosion Properties")]
    [SerializeField]
    [Tooltip("The radius of the projectile's explosion")]
    private float projectileExplosionRadius = 1f;
    // The initial impulse given to the projectile by the cannon who fired it
    private float projectileImpulse;
    [SerializeField]
    [Tooltip("The amount of bounces the shell has before exploding")]
    private int projectileBounces = 0;

    // TODO: This should be part of the explosion prefab.
    // Remove this from here once it exists
    [Header("Audio")]
    [SerializeField]
    [Tooltip("The array containing possible explosion sounds")]
    private AudioClip[] explosionSounds;
    private AudioSource explosionSoundSource;

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField]
    [Tooltip("Whether a red wire sphere is drawn to demonstrate the explosion radius")]
    private bool enableRadiusGizmo = false;
#endif


    // The rigidbody of the projectile for internal reference
    private Rigidbody projectileRigidbody;

    // The cannon that fired the shell
    private GameObject firedFrom;

    #endregion

    #region Properties

    public GameObject FiredFrom
    {
        get { return firedFrom; }
        set { firedFrom = value; }
    }
    public float ProjectileImpulse
    {
        get { return projectileImpulse; }
        set { projectileImpulse = value; }
    }

    #endregion

    #endregion

    #region Unity Methods
    // Start is called before the first frame update
    void Start()
    {
        // Caches the rigidbody component
        projectileRigidbody = GetComponent<Rigidbody>();
        // Caches the AudioSource component
        explosionSoundSource = GetComponent<AudioSource>();
        // Applies force on the projectile
        projectileRigidbody.AddRelativeForce(projectileImpulse * Vector3.up, ForceMode.Impulse);
        // Ignores collisions with fired from object
        Physics.IgnoreCollision(projectileRigidbody.GetComponent<Collider>(), firedFrom.GetComponent<Collider>(), true);
        // Autodestroys a shell after 4 seconds - the longest time of flight a shell could get is 3.68 seconds, so it shouldn't cause any problems.
        Destroy(gameObject, 4f);
    }

    // FixedUpdate is called once per fixed frame update
    private void FixedUpdate()
    {
        // Checks if the rotation vector is 0
        if (projectileRigidbody.velocity.magnitude != 0)
        {
            // Rotates the shell so that it points towards its velocity
            Vector3 orientation = Quaternion.LookRotation(projectileRigidbody.velocity).eulerAngles;
            projectileRigidbody.MoveRotation(Quaternion.Euler(orientation));
        }
    }

    // OnCollisionEnter is called whenever the GameObject collides with something
    private void OnCollisionEnter(Collision other)
    {
        // Reduces the amount of bounces left
        projectileBounces--;

        // If the object that fired the shell wasn't the one colliding with it
        if (other.gameObject != firedFrom)
        {
            ExplodeShell();
        }
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (enableRadiusGizmo == true)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, projectileExplosionRadius);
        }
    }

#endif
    #endregion

    #region Custom Methods

    private void ExplodeShell()
    {
        // TODO: Add explosion VFX

        // Play explosion sound
        // Picks a random fire cannon sound effect from the array and assigns it to the sound source
        explosionSoundSource.clip = explosionSounds[Random.Range(0, explosionSounds.Length)];
        // Randomizes pitch to increase sound variance
        explosionSoundSource.pitch = Random.Range(1.1f, 1.4f);
        // Makes the sound source play its sound
        explosionSoundSource.Play();

        // Makes a layer into a layermask so that it may be used in physics raycasts
        LayerMask tankBodyLayer = LayerMask.GetMask("TankBodies");

        // Gathers all entity colliders in the explosion's radius
        Collider[] tanksInRadius = Physics.OverlapSphere(transform.position, projectileExplosionRadius, tankBodyLayer.value);

        // Iterates through each of the tanks
        foreach (Collider tank in tanksInRadius)
        {

            // FIXME: Realistically, this won't cause performance drops, as a tank shell will rarely hit more than 2
            // gameobjects at a time, but using GetComponent in this manner is not optimal performance-wise.
            TankBase tankController = tank.transform.root.gameObject.GetComponent<TankBase>();

            // Adds explosion impact
            Rigidbody tankRigidbody = tank.gameObject.GetComponent<Rigidbody>();

            // Subtracts from the tank's health
            tankController.Health = tankController.Health - projectileDamage;
        }

        // Destroys the shell
        // FIXME: Realistically, this too won't cause performance drops, as there won't be too many shells being created
        // or destroyed too frequently. Still, the ideal way of doing this performance-wise is through object pooling
        if (projectileBounces <= 0)
        {
            Destroy(gameObject);
        }
        
    }

    #endregion
}
