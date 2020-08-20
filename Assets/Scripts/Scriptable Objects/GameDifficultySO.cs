using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameDifficulty", menuName = "Game Configurations/Difficulty", order = 0)]
public class GameDifficultySO : ScriptableObject
{
    #region Field Declarations

    #region Core Values

    [Header("Spawn chances in percentile")]
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
    private int enemyAmount;
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

    public int RollerTankChance
    {
        get { return rollerTankChance; }
        set
        {
            rollerTankChance = value;
            SpawnChanceValidityCheck();
        }
    }

    public int SpeederTankChance
    {
        get { return speederTankChance; }
        set
        {
            speederTankChance = value;
            SpawnChanceValidityCheck();
        }
    }

    public int SmasherTankChance
    {
        get { return smasherTankChance; }
        set
        {
            smasherTankChance = value;
            SpawnChanceValidityCheck();
        }
    }

    public int AnnihilatorTankChance
    {
        get { return annihilatorTankChance; }
        set
        {
            annihilatorTankChance = value;
            SpawnChanceValidityCheck();
        }
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

    public void SpawnChanceValidityCheck()
    {
        float totalSpawnChance = (rollerTankChance + speederTankChance + smasherTankChance + annihilatorTankChance);
        if (totalSpawnChance > 100)
        {
            Debug.LogErrorFormat("Total spawn chance is currently {0}%! If all spawn chances add up to more than 100%, some spawns may be more likely than intended, and some may be impossible!", totalSpawnChance);
        }
    }

    #endregion

}