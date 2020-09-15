using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupController : MonoBehaviour
{
    #region Field Declarations

    #region Core Values

    [Header("Generic properties")]
    [SerializeField]
    [Tooltip("The speed at which the powerup rotates")]
    private float rotationSpeed = 15f;
    // FIXME: The animation curve doesn't seem to actually fluctuate from 1 to 0 to 1, even when set up properly.
    [SerializeField]
    [Tooltip("How high or low the powerup will float")]
    private float floatMagnitude;
    [SerializeField]
    [Tooltip("The score bonus for collecting a powerup")]
    private float powerupScore = 200f;
    [SerializeField]
    [Tooltip("The lifetime in seconds the powerup will last for")]
    private float powerupLifetime = 60f;

    private Coroutine powerupLifetimeRoutine;
    private Coroutine powerupDestroyRoutine;

    // Individual powerup properties
    [Header("Individual properties")]
    [SerializeField]
    [Tooltip("The type of the powerup")]
    private PowerupTypes powerupType;
    [SerializeField]
    [Tooltip("The multiplier of the powerup")]
    private float speedMultipler;
    [SerializeField]
    [Tooltip("The duration of the powerup")]
    private float powerupDuration;
    [SerializeField]
    [Tooltip("The amount of items provided by the powerup")]
    private int powerupAmount;

    // Private reference to the gamecontroller
    private GameController gameControllerRef;

    #endregion

    #region Properties
    
    public GameController GameControllerRef
    {
        get { return gameControllerRef; }
        set { gameControllerRef = value; }
    }

    #endregion

    #endregion

    #region Unity Methods

    // Start is called before the first frame update
    private void Start()
    {
        // Starts the lifetime routine
        powerupLifetimeRoutine = StartCoroutine(PowerupLifetimeRoutine());
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        // Floats the powerup
        PowerupFloat();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Sends the action to enable powerups
            EventBroker.CallActivatePowerup(powerupType, powerupDuration, powerupAmount, speedMultipler);
            // Adds score
            EventBroker.CallAddScore(powerupScore);

            // TODO: Add powerup collect VFX

            Destroy(gameObject);
        }
    }

    #endregion

    #region Custom Methods

    // Animates the powerup with a float-like effect
    private void PowerupFloat()
    {
        // Rotates the Oil Barrel based on the rotation speed around its Y axis
        transform.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime, 0), Space.World);
        // Translates the Oil Barrel based on the height change induced by the accel curve
        transform.Translate(new Vector3(0, Mathf.Sin(Time.time) * floatMagnitude * Time.deltaTime, 0), Space.World);
    }

    #endregion

    #region Coroutines

    private IEnumerator PowerupLifetimeRoutine()
    {
        // Waits the lifetime of the powerup
        yield return new WaitForSeconds(powerupLifetime);
        // Start powerup destroy section
        powerupDestroyRoutine = StartCoroutine(PowerupDestroyRoutine());
        // Ends coroutine
        yield break;
    }

    private IEnumerator PowerupDestroyRoutine()
    {
        // TODO: Switch to dissolve shader
        // Wait 3 seconds
        yield return new WaitForSeconds(3);
        // Destroy powerup
        Destroy(gameObject);
        // Ends coroutine
        yield break;
    }

    #endregion
}

public enum PowerupTypes
{
    OilBarrel,
    Shield,
    NukeShell,
    Wrench,
    Ammo
}
