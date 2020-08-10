using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OilBarrelPowerup : MonoBehaviour
{
    [Header("Animation")]
    [Tooltip("The speed at which the powerup rotates")]
    public float m_RotationSpeed = 15f;
    [Tooltip("The height change applied to induce a floating effect")]
    public AnimationCurve m_FloatHeight;
    [Tooltip("The multiplier for the float effect translation")]
    public float m_FloatIntensity = 1f;
    [Tooltip("The multiplier used to multiply acceleration and maximum speed. It is also used to divide reloadTime")]

    [Header("Powerup Properties")]
    public float m_PowerupMultiplier = 2f;
    [Tooltip("The duration in seconds for the powerup")]
    public float m_PowerupDuration = 10f;
    [Tooltip("Powerup score reward")]
    public int m_PowerupScoreReward = 200;
    [Tooltip("How long in seconds the powerup exists for")]
    public float m_PowerupLifetime = 60;
    [Tooltip("How fast the powerup will diminish in size until it dissapears")]
    public float m_PowerupDiminishRate = 0.1f;
    [Tooltip("How much the powerup will diminish before disappearing")]
    [Range(0f, 1f)]
    public float m_PowerupDiminishThreshold = 0.1f;
    [Tooltip("How much the powerup's rotation accelerates as it despawns")]
    public float m_PowerupDespawnAccelerate = 1.05f;

    [Header("Explosion Properties")]
    [Tooltip("The radius of the Oil Barrel's explosion")]
    public float m_ExplosionRadius = 4f;
    [Tooltip("The damage caused by the Oil Barrel's explosion")]
    public int m_ExplosionDamage = 2;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Rotates the Oil Barrel based on the Rotation Speed value, on its own Y axis
        transform.Rotate(new Vector3(0, m_RotationSpeed * Time.deltaTime, 0), Space.World);
        // Translates the Oil Barrel based on the height change induced by the accel curve
        transform.Translate(new Vector3(0, m_FloatHeight.Evaluate(Time.time) * Time.deltaTime * m_FloatIntensity, 0), Space.World);

        // Ticks down the powerup's lifetime
        m_PowerupLifetime -= Time.deltaTime;
        // checks to see if the powerup's lifetime has expired
        if (m_PowerupLifetime <= 0)
        {
            // Calls the despawn method
            Despawn(m_PowerupDiminishRate, m_PowerupDiminishThreshold, m_PowerupDespawnAccelerate);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Checks whether the collider is a tank shell
        if (other.gameObject.CompareTag("Projectile"))
        {
            // TODO: Cause explosion
            Destroy(other.gameObject);
            Explosion(m_ExplosionDamage, m_ExplosionRadius);

            // Destroys the powerup
            Destroy(gameObject);
        }

        // Checks whether the collider is the player's tank
        else if (other.gameObject.CompareTag("Player"))
        {
            // Apply powerup
            ApplyPowerup(m_PowerupDuration, m_PowerupMultiplier);

            // Destroys the powerup
            Destroy(gameObject);
        }
    }

    // Damage player method
    void Explosion(int damageAmount, float explosionRadius)
    {
        // Get Player Tank object reference
        // Find a way to get Player Tank reference through transform.root, as it is considerably more performant
        GameObject player = GameObject.Find("Player Tank");

        // Gets PlayerController class reference
        PlayerController playerController = player.gameObject.GetComponent<PlayerController>();

        // If the player is not invulnerable
        if (!playerController.m_Invulnerable)
        {
            // Damage is inversely proportional to the player's distance from the explosion's center
            float distance = explosionRadius / Vector3.Distance(player.transform.position, transform.position);
            // Reduce health variable by damage amount
            playerController.m_Health -= Mathf.RoundToInt(distance * damageAmount);

            Debug.Log(distance * damageAmount);
        }
    }

    void ApplyPowerup(float duration, float multiplier)
    {
        // Get Player Tank object reference
        // Find a way to get Player Tank reference through transform.root, as it is considerably more performant
        GameObject player = GameObject.Find("Player Tank");

        // Gets PlayerController class reference
        PlayerController playerController = player.gameObject.GetComponent<PlayerController>();

        // Sets powerup multiplier
        playerController.m_OilBarrelMultiplier = multiplier;
        // Sets powerup duration
        playerController.m_OilBarrelTimer = duration;
        // Enables powerup on player object
        playerController.m_powerupState[0] = true;
        // Gives player score reward
        playerController.m_Score += m_PowerupScoreReward;
    }

    void Despawn(float scaleDiminish, float destroyThreshold, float rotationAcceleration)
    {
        m_RotationSpeed += m_RotationSpeed * rotationAcceleration * Time.deltaTime;
        // Calculates the individual frame scale diminish
        float frameScaleDiminish = scaleDiminish * Time.deltaTime;
        // Diminishes scale on all axes by the value provided by scaleDiminish
        transform.localScale -= new Vector3(frameScaleDiminish, frameScaleDiminish, frameScaleDiminish);
        // Checks to see if scale is below scale threshold
        if (transform.localScale.x <= destroyThreshold)
        {
            // Destroys the object
            Destroy(gameObject);
        }
    }
}
