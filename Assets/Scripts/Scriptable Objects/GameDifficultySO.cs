using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameDifficulty", menuName = "Game Configurations/Difficulty", order = 0)]
public class GameDifficultySO : ScriptableObject
{
    #region Field Declarations

    #region Core Values

    [Header("Powerup spawn chances in percentile")]
    [SerializeField]
    [Tooltip("The spawn chance of this type of powerup in percentage. The percentages must add up to 100.")]
    private int oilBarrelChance = 20;
    [SerializeField]
    [Tooltip("The spawn chance of this type of powerup in percentage. The percentages must add up to 100.")]
    private int shieldChance = 20;
    [SerializeField]
    [Tooltip("The spawn chance of this type of powerup in percentage. The percentages must add up to 100.")]
    private int wrenchChance = 20;
    [SerializeField]
    [Tooltip("The spawn chance of this type of powerup in percentage. The percentages must add up to 100.")]
    private int ammoChance = 30;
    [SerializeField]
    [Tooltip("The spawn chance of this type of powerup in percentage. The percentages must add up to 100.")]
    private int nukeShellChance = 10;

    [Header("Powerup frequency")]
    [Tooltip("The percentage chance that a powerup will in fact spawn. Only after a powerup in fact spawns, will its type be decided.")]
    [Range(0f, 1f)]
    private float powerupChance = 0.33f;

    [Header("Enemy spawn chances in percentile")]
    [SerializeField]
    [Tooltip("The spawn chance of this type of enemy in percentage. The percentages must add up to 100.")]
    private int rollerTankChance = 40;
    [SerializeField]
    [Tooltip("The spawn chance of this type of enemy in percentage. The percentages must add up to 100.")]
    private int speederTankChance = 25;
    [SerializeField]
    [Tooltip("The spawn chance of this type of enemy in percentage. The percentages must add up to 100.")]
    private int smasherTankChance = 25;
    [SerializeField]
    [Tooltip("The spawn chance of this type of enemy in percentage. The percentages must add up to 100.")]
    private int annihilatorTankChance = 10;

    [Header("Enemy amount")]
    [SerializeField]
    [Tooltip("The amount of enemies that will spawn on a given wave. This number increases as the waves increase.")]
    private int enemyAmount = 4;
    [SerializeField]
    [Tooltip("The Roller Tank prefab")]
    private GameObject rollerTank;
    [SerializeField]
    [Tooltip("The Speeder Tank prefab")]
    private GameObject speederTank;
    [SerializeField]
    [Tooltip("The Smasher Tank prefab")]
    private GameObject smasherTank;
    [SerializeField]
    [Tooltip("The Annihilator Tank prefb")]
    private GameObject annihilatorTank;

    #endregion

    #region Properties

    // Powerup spawn chance properties
    public int OilBarrelChance
    {
        get { return oilBarrelChance; }
        set
        {
            oilBarrelChance = value;
            PowerupSpawnChanceValidity();
        }
            
    }

    public int ShieldChance
    {
        get { return shieldChance; }
        set
        {
            shieldChance = value;
            PowerupSpawnChanceValidity();
        }
    }

    public int WrenchChance
    {
        get { return wrenchChance; }
        set
        {
            wrenchChance = value;
            PowerupSpawnChanceValidity();
        }
    }

    public int AmmoChance
    {
        get { return ammoChance; }
        set
        {
            ammoChance = value;
            PowerupSpawnChanceValidity();
        }
    }

    public int NukeShellChance
    {
        get { return nukeShellChance; }
        set
        {
            nukeShellChance = value;
            PowerupSpawnChanceValidity();
        }
    }

    // Powerup spawn chance
    public float PowerupChance
    {
        get { return powerupChance; }
    }

    // Enemy spawn chance properties
    public int RollerTankChance
    {
        get { return rollerTankChance; }
        set
        {
            rollerTankChance = value;
            EnemySpawnChanceValidity();
        }
    }

    public int SpeederTankChance
    {
        get { return speederTankChance; }
        set
        {
            speederTankChance = value;
            EnemySpawnChanceValidity();
        }
    }

    public int SmasherTankChance
    {
        get { return smasherTankChance; }
        set
        {
            smasherTankChance = value;
            EnemySpawnChanceValidity();
        }
    }

    public int AnnihilatorTankChance
    {
        get { return annihilatorTankChance; }
        set
        {
            annihilatorTankChance = value;
            EnemySpawnChanceValidity();
        }
    }

    // Enemy properties
    public int EnemyAmount
    {
        get { return enemyAmount; }
    }

    public GameObject RollerTank
    {
        get { return rollerTank; }
    }

    public GameObject SpeederTank
    {
        get { return speederTank; }
    }

    public GameObject SmasherTank
    {
        get { return smasherTank; }
    }

    public GameObject AnnihilatorTank
    {
        get { return annihilatorTank; }
    }

    #endregion

    #endregion

    #region Methods

    //TODO: add a method that increases the enemy spawn amount as the wave increases.
    public void UpdateEnemyAmount()
    {

    }

    // Checks whether them sum of all spawn chances are below or over 100
    public void EnemySpawnChanceValidity()
    {
        float totalSpawnChance = (rollerTankChance + speederTankChance + smasherTankChance + annihilatorTankChance);
        if (totalSpawnChance > 100)
        {
            Debug.LogErrorFormat("Total enemy spawn chance is currently {0}%! If all spawn chances add up to more than 100%, some spawns may be more likely than intended, and some may be impossible!", totalSpawnChance);
        }
        else if (totalSpawnChance < 100)
        {
            Debug.LogErrorFormat("Total enemy spawn chance is currently {0}%! If all spawn chances add up to less than 100%, some spawns may fail!", totalSpawnChance);
        }
    }

        // Checks whether them sum of all spawn chances are below or over 100
    public void PowerupSpawnChanceValidity()
    {
        float totalSpawnChance = (oilBarrelChance + shieldChance + wrenchChance + ammoChance + nukeShellChance);
        if (totalSpawnChance > 100)
        {
            Debug.LogErrorFormat("Total powerup spawn chance is currently {0}%! If all spawn chances add up to more than 100%, some spawns may be more likely than intended, and some may be impossible!", totalSpawnChance);
        }
        else if (totalSpawnChance < 100)
        {
            Debug.LogErrorFormat("Total powerup spawn chance is currently {0}%! If all spawn chances add up to less than 100%, some spawns may fail!", totalSpawnChance);
        }
    }

    #endregion

}