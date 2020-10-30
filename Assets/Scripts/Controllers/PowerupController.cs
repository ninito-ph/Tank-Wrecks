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
    [SerializeField]
    [Tooltip("The sound effect played when the powerup is picked up")]
    private AudioClip powerupClip;

    // Private reference to the gamecontroller
    private GameController gameControllerRef;

    // Reference to the renderer component
    private Renderer powerupRenderer;

    // Deltas for the movement and rotation of the powerup
    // These are being declared in this manner to reduce heap memory usage (garbage generation)
    private Vector3 rotationDelta = new Vector3(0f, 0f, 0f);
    private Vector3 movementDelta = new Vector3(0f, 0f, 0f);

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
        powerupRenderer = gameObject.GetComponent<Renderer>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        // Floats the powerup
        if (powerupRenderer.isVisible == true)
        {
            PowerupFloat();
        }

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

    private void OnDestroy()
    {
        StopAllCoroutines();

        // Plays clip at pickup point
        AudioSource.PlayClipAtPoint(powerupClip, transform.position);

        // TODO: Add pickup VFX
    }

    #endregion

    #region Custom Methods

    // Animates the powerup with a float-like effect
    private void PowerupFloat()
    {
        // Calls an update to the delta
        UpdateDeltas();

        // Rotates the Oil Barrel based on the rotation speed around its Y axis
        transform.Rotate(rotationDelta, Space.World);
        // Translates the Oil Barrel based on the height change induced by the accel curve
        transform.Translate(movementDelta, Space.World);
    }

    // Updates the values in the rotation and movement deltas
    private void UpdateDeltas()
    {
        // Updates the movement delta
        movementDelta.Set(0f, Mathf.Sin(Time.time) * floatMagnitude * Time.deltaTime, 0f);

        // Updates the rotation delta
        rotationDelta.Set(0f, rotationSpeed * Time.deltaTime, 0f);
    }

    #endregion

    #region Coroutines

    private IEnumerator PowerupLifetimeRoutine()
    {
        // Waits the lifetime of the powerup
        yield return new WaitForSeconds(powerupLifetime);
        // Start powerup destroy section
        // TODO: Switch to dissolve shader
        // Wait 3 seconds
        yield return new WaitForSeconds(3);
        // Destroy powerup
        Destroy(gameObject);
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
