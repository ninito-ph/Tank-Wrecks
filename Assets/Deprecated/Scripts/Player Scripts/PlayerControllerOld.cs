using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControllerOld : MonoBehaviour
{
    [Header("Core Properties")]
    [Tooltip("The maximum amount of HP the player can have")]
    public int m_MaxHealth = 3;
    [Tooltip("The current amount of HP the player has")]
    public int m_Health = 3;
    [Tooltip("The player's score")]
    public float m_Score = 0;
    [Tooltip("The player's max ammo")]
    public int m_MaxAmmo = 25;
    [Tooltip("The player's ammo")]
    public int m_Ammo = 25;

    [Header("Child class references")]
    [Tooltip("The class reference for the tank's cannon")]
    public TankCannonController cannonController;
    [Tooltip("The class refence for the tank's head")]
    public TankHeadController headController;
    [Tooltip("The class reference for the tank's body")]
    public TankBodyController bodyController;

    [Header("User Interface")]
    [Tooltip("The ammunition viewer text reference")]
    public Text m_ammoViewerText;
    [Tooltip("The Fire Readiness Icon image reference")]
    public Image m_FireReadinessIcon;
    [Tooltip("The reference to the score number text")]
    public Text m_UIScore;
    [Tooltip("The speed multiplier at which the displayed score will be lerped at")]
    public float m_ScoreLerpSpeed = 1;
    [Tooltip("An array containing references to heart images, in order to control fill")]
    public Image[] m_heartFill = new Image[3];




    // The score displayed to the player
    private float m_ScoreLerp = 0;
    [HideInInspector]
    // An array to store powerup activation state
    public bool[] m_powerupState = new bool[] {false, false, false};



    // Private variables referring to the Shield powerup
    // Whether the player is currently invulnerable
    [HideInInspector]
    public bool m_Invulnerable = false;

    // Private variables referring to the OilBarrel powerup
    // Whether the powerup has undergone the startup process
    private bool m_OilBarrelStartup = false;
    // Saves the default acceleration so it can be resotred later
    [HideInInspector]
    public float m_OilBarrelMultiplier;
    // A timer containing the remaining duration of the oilbarrel powerup
    [HideInInspector]
    public float m_OilBarrelTimer;

    // Start is called before the first frame update
    void Start()
    {
        // Sets display to current ammo
        m_ammoViewerText.text = m_Ammo.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        // SCORE MANAGEMENT
        // The score number is linearly interpolated for a crisp score increase effect
        m_ScoreLerp = Mathf.Lerp(m_ScoreLerp, m_Score, Time.deltaTime * m_ScoreLerpSpeed);
        // The displayed number is then rounded to an integer, so that no decimal scores may appear
        int displayedScore = Mathf.RoundToInt(m_ScoreLerp);
        // The displayed score number is then converted to a string and passed to the UI score number
        m_UIScore.text = displayedScore.ToString();

        // AMMO MANAGEMENT
        // Ammo text updater
        // TODO: Find a way to cleanly code this in a more performant manner; running this every frame is not necessary
        m_ammoViewerText.text = m_Ammo.ToString();
        // Ammo limit checker
        if (m_Ammo > m_MaxAmmo)
        {
            m_Ammo = m_MaxAmmo;
        }
        // Fire readiness icon fill control. The fill is linerarly interpolated for a crisp, smooth effect.
        m_FireReadinessIcon.fillAmount = Mathf.Lerp(m_FireReadinessIcon.fillAmount, cannonController.reloadCooldown / cannonController.reloadTime, Time.deltaTime * 13);

        // HEALTH MANAGEMENT
        // Health limit checker
        if (m_Health > m_MaxHealth)
        {
            m_Health = m_MaxHealth;
        }

        // POWERUP MANAGEMENT
        // Oil Barrel - Powerup 1
        if (m_powerupState[0] == true)
        {
            if (!m_OilBarrelStartup)
            {
                // Multiplies max speed and acceleration by the OilBarrelMultiplier, divides reload time by OilBarrelMultiplier
                bodyController.maxSpeed = bodyController.maxSpeed * m_OilBarrelMultiplier;
                bodyController.acceleration = bodyController.acceleration * m_OilBarrelMultiplier;
                cannonController.reloadTime = cannonController.reloadTime / m_OilBarrelMultiplier;
                // Sets startup to true, as in startup complete
                m_OilBarrelStartup = true;
            }

            // Ticks down timer
            m_OilBarrelTimer -= Time.deltaTime;

            // If time has expired
            if (m_OilBarrelTimer <= 0)
            {
                // Reverts the maxSpeed, acceleration and reloadTime variables by performing the inverse operations
                bodyController.maxSpeed = bodyController.maxSpeed / m_OilBarrelMultiplier;
                bodyController.acceleration = bodyController.acceleration / m_OilBarrelMultiplier;
                cannonController.reloadTime = cannonController.reloadTime * m_OilBarrelMultiplier;

                // Ends the powerup
                m_OilBarrelStartup = false;
                m_powerupState[0] = false;
            }
        }

        // Energy Shield

        // Repair Powerup
        if (m_powerupState[2] == true)
        {
            m_Health = m_MaxHealth;
            m_powerupState[2] = false;
        }
    }
}
