using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Core Properties")]
    [Tooltip("The maximum amount of HP the player can have")]
    public int m_MaxHealth = 3;
    [Tooltip("The current amount of HP the player has")]
    public int m_CurrentHealth = 3;
    [Tooltip("The player's score")]
    public float m_Score = 0;
    [Tooltip("The player's max ammo")]
    public int m_MaxAmmo = 25;
    [Tooltip("The player's ammo")]
    public int m_Ammo = 25;

    [Header("Child class references")]
    public TankCannonController cannonController;

    [Header("User Interface")]
    [Tooltip("The ammunition viewer text reference")]
    public Text m_ammoViewerText;
    [Tooltip("The Fire Readiness Icon image reference")]
    public Image m_FireReadinessIcon;
    [Tooltip("The reference for the first heart image")]
    public Image m_Heart1;
    [Tooltip("The reference for the second heart image")]
    public Image m_Heart2;
    [Tooltip("The reference for the third heart image")]
    public Image m_Heart3;
    [Tooltip("The reference to the score number text")]
    public Text m_UIScore;
    [Tooltip("The speed multiplier at which the displayed score will be lerped at")]
    public float m_ScoreLerpSpeed = 1;



    // The score displayed to the player
    private float m_ScoreLerp = 0;

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
        if (m_CurrentHealth > m_MaxHealth)
        {
            m_CurrentHealth = m_MaxHealth;
        }
        // Health heart fill updater
        // TODO: Move over to array-based Heart fill amount control, which will make easy lerp fading of the hearts possible
        switch (m_CurrentHealth)
        {
            case 0:
                m_Heart1.fillAmount = 0;
                m_Heart2.fillAmount = 0;
                m_Heart3.fillAmount = 0;

                break;

            case 1:
                m_Heart1.fillAmount = 0;
                m_Heart2.fillAmount = 0;
                m_Heart3.fillAmount = 1;
                break;

            case 2:
                m_Heart1.fillAmount = 0;
                m_Heart2.fillAmount = 1;
                m_Heart3.fillAmount = 1;
                break;

            case 3:
                m_Heart1.fillAmount = 1;
                m_Heart2.fillAmount = 1;
                m_Heart3.fillAmount = 1;
                break;

            default:
                break;
        }
    }
}
