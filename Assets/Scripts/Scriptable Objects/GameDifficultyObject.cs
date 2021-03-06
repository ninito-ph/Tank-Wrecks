using UnityEngine;

[CreateAssetMenu(fileName = "GameDifficulty", menuName = "Game Configurations/Difficulty Profile", order = 0)]
public class GameDifficultyObject : ScriptableObject
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

    [Header("Powerups")]
    [SerializeField]
    [Tooltip("The Oil Barrel powerup prefab")]
    private GameObject oilBarrelPowerup;
    [SerializeField]
    [Tooltip("The Shield powerup prefab")]
    private GameObject shieldPowerup;
    [SerializeField]
    [Tooltip("The Wrench powerup prefab")]
    private GameObject wrenchPowerup;
    [SerializeField]
    [Tooltip("The Ammo powerup prefab")]
    private GameObject ammoPowerup;
    [SerializeField]
    [Tooltip("The Nuke Shell powerup prefab")]
    private GameObject nukeShellPowerup;
    [SerializeField]
    [Tooltip("The interval between powerup spawns")]
    private float powerupSpawnCooldown;

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
    [Tooltip("The amount of enemies that will spawn at the final wave (30).")]
    private int maxEnemyAmount = 12;
    //The amount of enemies that will spawn on a given wave. This number increases up to the max enemy amount as the waves increase
    private int enemyAmount = 1;
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

    [Header("Score Rewards")]
    [SerializeField]
    [Tooltip("The bonus in points for completing a wave")]
    private float waveCompleteBonus = 50f;
    [SerializeField]
    [Tooltip("The difficulty for which score is multiplied")]
    private float difficultyScoreModifier = 1f;

    #endregion

    #region Properties

    #region Score Properties
    public float WaveCompleteBonus
    {
        get { return waveCompleteBonus; }
    }

    public float DifficultyScoreModifier
    {
        get { return difficultyScoreModifier; }
    }
    #endregion

    #region Powerup Properties
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
    public GameObject OilBarrelPowerup
    {
        get { return oilBarrelPowerup; }
    }

    public GameObject ShieldPowerup
    {
        get { return shieldPowerup; }
    }

    public GameObject WrenchPowerup
    {
        get { return wrenchPowerup; }
    }

    public GameObject AmmoPowerup
    {
        get { return ammoPowerup; }
    }

    public GameObject NukeShellPowerup
    {
        get { return nukeShellPowerup; }
    }

    public float PowerupSpawnCooldown
    {
        get { return powerupSpawnCooldown; }
    }

    #endregion

    #region Enemy Properties
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

    #endregion

    #region Methods

    // Increases the enemy spawn amount accourding to a easeInOutSine curve
    public void UpdateEnemyAmount(int wave, int peakDifficultyWave)
    {
        // Calculates the time to sample the curve
        float sampleTime = Mathf.Min(1f, (float)wave / (float)peakDifficultyWave);

        // Calculates difficulty curve progress
        float sampleTimeSquared = sampleTime * sampleTime;

        // Updates enemy amount
        enemyAmount = Mathf.CeilToInt(sampleTimeSquared / (2 * (sampleTimeSquared - sampleTime) + 1) * maxEnemyAmount);
    }

    // Checks whether them sum of all spawn chances are below or over 100
    private void EnemySpawnChanceValidity()
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
    private void PowerupSpawnChanceValidity()
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