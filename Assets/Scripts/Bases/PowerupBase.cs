using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupBase : MonoBehaviour
{
    #region Field Declarations

    [Header("Powerup properties")]
    [SerializeField]
    [Tooltip("The speed at which the powerup rotates")]
    private float rotationSpeed = 15f;
    // FIXME: The animation curve doesn't seem to actually fluctuate from 1 to 0 to 1, even when set up properly.
    [SerializeField]
    [Tooltip("The modifier for the powerup's height. It is changed every frame accourding to a sample of the curve at a given time. Causes a float effect.")]
    private AnimationCurve floatHeight;
    [SerializeField]
    [Tooltip("The score bonus for collecting a powerup")]
    private float powerupScore = 200f;
    [SerializeField]
    [Tooltip("The lifetime in seconds the powerup will last for")]
    private float powerupLifetime = 60f;

    private Coroutine powerupLifetimeRoutine;
    private Coroutine powerupDestroyRoutine;
    private PowerupTypes powerupType;

    #endregion

    #region Unity Methods

    // Start is called before the first frame update
    protected virtual void Start()
    {
        // Starts the lifetime routine
        powerupLifetimeRoutine = StartCoroutine(PowerupLifetimeRoutine());
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        // Floats the powerup
        PowerupFloat();
    }

    protected virtual void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            EventBroker.CallActivatePowerup(powerupType);
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
        transform.Translate(new Vector3(0, floatHeight.Evaluate(Time.time) * Time.deltaTime, 0), Space.World);
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
        Destroy(this);
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
