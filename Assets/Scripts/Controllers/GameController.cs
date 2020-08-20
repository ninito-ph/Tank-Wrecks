using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    #region Field Declarations

    #region  Core Values

    private int wave = 1;
    private float score = 0;
    private GameDifficultySO difficultyConfig;
    private Queue<EnemyBase> enemiesToSpawn;
    private Queue<PowerupBase> powerupsToSpawn;
    private List<EnemyBase> activeEnemies;
    private List<GameObject> spawnPoints;
    private List<GameObject> spawnablesList;
    private Coroutine spawnEnemiesRoutine;
    private Coroutine spawnPowerupsRoutine;
    private PlayerController playerReference;

    #endregion

    #region Properties

    public int Wave
    {
        get { return wave; }
    }

    public float Score
    {
        get { return score; }
    }

    public PlayerController PlayerReference
    {
        get { return playerReference; }
    }

    #endregion

    #endregion

    #region Unity Methods
    // Start is called before the first frame update
    void Start()
    {
        // Gets a reference of playerController so other classes can access it
        playerReference = FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #endregion

    #region Custom Methods

    private void StartGame()
    {
        
    }

    private void EndGame()
    {

    }

    private void BeginWave()
    {

    }

    private void EndWave()
    {

    }

    private GameObject PickEnemy()
    {
        // Uses the system randomizer, which is considerably better than Unity's
        System.Random trueRandomizer = new System.Random();
        int randomIndexPick = trueRandomizer.Next(1, 100);

        // Randomly draw enemy type from spawnable enemy pool
        GameObject enemyToSpawn = spawnablesList[randomIndexPick];
        return enemyToSpawn;
    }

    // Populates the list containing possibilities for spawning an enemy
    private void PopulateSpawnablesList(bool repopulate = false)
    {
        // If the list doesn't exist, create it.
        if (spawnablesList == null)
        {
            spawnablesList = new List<GameObject>();
        }

        // Checks to see if the list is meant to be REpopulated, meaning it needs to be cleared and populated again
        if (repopulate == true)
        {
            spawnablesList.Clear();
        }

        //FIXME: This code is MONSTROUSLY better than our first implementation, but it still remains relatively unextensible
        // Populates list with roller tank entries
        for (int spawnEntry = 0; spawnEntry < difficultyConfig.RollerTankChance; spawnEntry ++)
        {
            spawnablesList.Add(difficultyConfig.RollerTank);
        }

        // Populates list with speeder tank entries
        for (int spawnEntry = 0; spawnEntry < difficultyConfig.SpeederTankChance; spawnEntry++)
        {
            spawnablesList.Add(difficultyConfig.SpeederTank);
        }

        // Populates list with smasher tank entries
        for (int spawnEntry = 0; spawnEntry < difficultyConfig.SmasherTankChance; spawnEntry++)
        {
            spawnablesList.Add(difficultyConfig.SmasherTank);
        }

        // Populates list with annihilator tank entries
        for (int spawnEntry = 0; spawnEntry < difficultyConfig.AnnihilatorTankChance; spawnEntry++)
        {
            spawnablesList.Add(difficultyConfig.AnnihilatorTank);
        }

        // Checks if list has more or less than 100 entries.
        if (spawnablesList.Count > 100)
        {
            int listOverpopulationCount = spawnablesList.Count - 100;
            Debug.LogWarningFormat("Overpopulated list by {0} entries. Full entry count at {1}", listOverpopulationCount, spawnablesList.Count);
        }
        else if (spawnablesList.Count < 100)
        {
            int listUnderpopulationCount = 100 - spawnablesList.Count;
            Debug.LogWarningFormat("Underpopulated list by {0} entries. Full entry count at {1}", listUnderpopulationCount, spawnablesList.Count);
        }
    }

    #region Coroutines

    // Coroutine for periodically spawning enemies
    private IEnumerator SpawnEnemy()
    {

        // Spawn enemy on spawn points
        yield return null;
    }

    // Coroutine for periodically spawning powerups
    private IEnumerator SpawnPowerup()
    {
        yield return null;
    }

    #endregion

    #endregion
}
