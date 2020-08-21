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
    private Queue<GameObject> enemiesToSpawn;
    private List<GameObject> activeEnemies;
    private List<GameObject> enemySpawnPoints;
    private List<GameObject> powerupSpawnPoints;
    private List<GameObject> enemySpawnPool;
    private List<GameObject> powerupSpawnPool;
    private Coroutine spawnEnemiesRoutine;
    private Coroutine spawnPowerupsRoutine;
    private Coroutine isWaveOverRoutine;
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

        // Creates instances of used lists
        powerupSpawnPool = new List<GameObject>();
        enemySpawnPool = new List<GameObject>();

        enemySpawnPoints = new List<GameObject>();
        powerupSpawnPoints = new List<GameObject>();

        activeEnemies = new List<GameObject>();

        // Creates instances of used Queues
        enemiesToSpawn = new Queue<GameObject>();

        // Starts the game
        StartGame();
    }

    // Update is called once per frame
    void Update()
    {

    }

    #endregion

    #region Custom Methods

    private void StartGame()
    {
        // Initial populate spawnpools
        PopulateEnemySpawnpool();
        PopulatePowerupSpawnpool();
        // Begins the first wave
        BeginWave();
    }

    private void EndGame()
    {
        // TODO: Add game end effects, save game metrics and return to main menu
    }

    private void BeginWave()
    {
        // Spawns enemies & powerups
        spawnEnemiesRoutine = StartCoroutine(SpawnEnemy());
        spawnPowerupsRoutine = StartCoroutine(SpawnPowerup());
        // Adds to score based on previous wave
        score += difficultyConfig.WaveCompleteBonus * difficultyConfig.DifficultyScoreModifier * wave;
        // Increases wave
        wave++;
        // Calls WaveOver event
        EventBroker.CallWaveStarted();
        // Starts checking if wave is over
        isWaveOverRoutine = StartCoroutine(IsWaveOver());
    }

    private void EndWave()
    {
        // Stop checking if wave is over
        StopCoroutine(IsWaveOver());
        // Stops powerups from spawning
        StopCoroutine(spawnPowerupsRoutine);
        // TODO: Add a grace period inbetween waves
        // Starts next wave
        BeginWave();
    }

    private void NextLevel()
    {

    }

    // Picks an object from a list of spawnables
    private GameObject PickObject(List<GameObject> spawnPool)
    {
        // Picks a random number to pick from the index
        int randomIndexPick = Mathf.RoundToInt(Random.Range(1f, 100f));

        // Randomly draw enemy type from spawnable enemy pool
        GameObject objectToSpawn = spawnPool[randomIndexPick];
        return objectToSpawn;
    }

    // Populates the list containing possibilities for spawning a powerup
    private void PopulatePowerupSpawnpool()
    {
        // If the list doesn't exist, create it.
        if (powerupSpawnPool == null)
        {
            powerupSpawnPool = new List<GameObject>();
        }

        // Clears the list to repopulate it, in case it has been populated before
        powerupSpawnPool.Clear();


        //FIXME: This code is MONSTROUSLY better than our first implementation, but it still remains relatively inextensible
        // Populates list with oil barrel entries
        for (int spawnEntry = 0; spawnEntry < difficultyConfig.OilBarrelChance; spawnEntry++)
        {
            powerupSpawnPool.Add(difficultyConfig.OilBarrelPowerup);
        }

        // Populates list with shield powerup entries
        for (int spawnEntry = 0; spawnEntry < difficultyConfig.ShieldChance; spawnEntry++)
        {
            powerupSpawnPool.Add(difficultyConfig.ShieldPowerup);
        }

        // Populates list with wrench powerup entries
        for (int spawnEntry = 0; spawnEntry < difficultyConfig.WrenchChance; spawnEntry++)
        {
            powerupSpawnPool.Add(difficultyConfig.WrenchPowerup);
        }

        // Populates list with ammo powerup entries
        for (int spawnEntry = 0; spawnEntry < difficultyConfig.AmmoChance; spawnEntry++)
        {
            powerupSpawnPool.Add(difficultyConfig.AmmoPowerup);
        }

        // Populates list with ammo powerup entries
        for (int spawnEntry = 0; spawnEntry < difficultyConfig.NukeShellChance; spawnEntry++)
        {
            powerupSpawnPool.Add(difficultyConfig.NukeShellPowerup);
        }

        // Checks if list has more or less than 100 entries.
        if (powerupSpawnPool.Count > 100)
        {
            int listOverpopulationCount = powerupSpawnPool.Count - 100;
            Debug.LogWarningFormat("Overpopulated powerup list by {0} entries. Full entry count at {1}", listOverpopulationCount, powerupSpawnPool.Count);
        }
        else if (powerupSpawnPool.Count < 100)
        {
            int listUnderpopulationCount = 100 - powerupSpawnPool.Count;
            Debug.LogWarningFormat("Underpopulated powerup list by {0} entries. Full entry count at {1}", listUnderpopulationCount, powerupSpawnPool.Count);
        }
    }

    // Populates the list containing possibilities for spawning an enemy
    private void PopulateEnemySpawnpool(bool repopulate = false)
    {
        // If the list doesn't exist, create it.
        if (enemySpawnPool == null)
        {
            enemySpawnPool = new List<GameObject>();
        }

        // Checks to see if the list is meant to be REpopulated, meaning it needs to be cleared and populated again to update chances
        if (repopulate == true)
        {
            enemySpawnPool.Clear();
        }

        //FIXME: This code is MONSTROUSLY better than our first implementation, but it still remains relatively inextensible
        // Populates list with roller tank entries
        for (int spawnEntry = 0; spawnEntry < difficultyConfig.RollerTankChance; spawnEntry++)
        {
            enemySpawnPool.Add(difficultyConfig.RollerTank);
        }

        // Populates list with speeder tank entries
        for (int spawnEntry = 0; spawnEntry < difficultyConfig.SpeederTankChance; spawnEntry++)
        {
            enemySpawnPool.Add(difficultyConfig.SpeederTank);
        }

        // Populates list with smasher tank entries
        for (int spawnEntry = 0; spawnEntry < difficultyConfig.SmasherTankChance; spawnEntry++)
        {
            enemySpawnPool.Add(difficultyConfig.SmasherTank);
        }

        // Populates list with annihilator tank entries
        for (int spawnEntry = 0; spawnEntry < difficultyConfig.AnnihilatorTankChance; spawnEntry++)
        {
            enemySpawnPool.Add(difficultyConfig.AnnihilatorTank);
        }

        // Checks if list has more or less than 100 entries.
        if (enemySpawnPool.Count > 100)
        {
            int listOverpopulationCount = enemySpawnPool.Count - 100;
            Debug.LogWarningFormat("Overpopulated enemy list by {0} entries. Full entry count at {1}", listOverpopulationCount, enemySpawnPool.Count);
        }
        else if (enemySpawnPool.Count < 100)
        {
            int listUnderpopulationCount = 100 - enemySpawnPool.Count;
            Debug.LogWarningFormat("Underpopulated enemy list by {0} entries. Full entry count at {1}", listUnderpopulationCount, enemySpawnPool.Count);
        }
    }

    #region Coroutines

    // Coroutine for periodically spawning enemies. Ends itself once it finishes spawning things.
    private IEnumerator SpawnEnemy()
    {
        // Enqueues enemies to spawn
        for (int queueEntries = 0; queueEntries < difficultyConfig.EnemyAmount; queueEntries++)
        {
            enemiesToSpawn.Enqueue(PickObject(enemySpawnPool));
        }

        // Spawns enemies until queue is empty
        while (enemiesToSpawn.Count > 0)
        {
            // Picks a random number in the enemySpawnPoints list to choose the spawn location
            int randomSpawnPointPick = Mathf.RoundToInt(Random.Range(1f, enemySpawnPoints.Count));
            // Defines a sphere to check if the spawn point is being occupied
            var spawnColliderCheck = Physics.OverlapSphere(enemySpawnPoints[randomSpawnPointPick].transform.position, 2, 8);


            // Checks if the spawnpoint is being occupied by something else. If it is, change the spawn point position, update sphere collider and wait 3 seconds before trying again.
            while (spawnColliderCheck.Length > 0)
            {
                randomSpawnPointPick = Mathf.RoundToInt(Random.Range(1f, enemySpawnPoints.Count));
                spawnColliderCheck = Physics.OverlapSphere(enemySpawnPoints[randomSpawnPointPick].transform.position, 2, 8);
                yield return new WaitForSeconds(3);
            }

            // Instantiates the enemy at the front of the queue
            GameObject spawnedEnemy = Instantiate(enemiesToSpawn.Dequeue(), enemySpawnPoints[randomSpawnPointPick].transform.position, Quaternion.identity);
            // Passes the spawnedEnemy its reference.
            EnemyBase spawnedEnemyBase = spawnedEnemy.GetComponent<EnemyBase>();
            spawnedEnemyBase.AssignedReference = spawnedEnemy;
            // Adds the spawnedEnemy to the activeEnemy list
            activeEnemies.Add(spawnedEnemy);
        }

        // The spawnEnemy coroutine marks itself as null to signal it has finished spawning enemies 
        spawnEnemiesRoutine = null;

        yield break;
    }

    // Coroutine for periodically spawning powerups. Loops.
    private IEnumerator SpawnPowerup()
    {
        // Picks a random number in the powerupSpawnPoints list to choose the spawn location
        int randomSpawnPointPick = Mathf.RoundToInt(Random.Range(1f, powerupSpawnPoints.Count));
        // Defines a sphere to check if the spawn point is being occupied
        var spawnColliderCheck = Physics.OverlapSphere(enemySpawnPoints[randomSpawnPointPick].transform.position, 2, 8);

        // Checks if the spawnpoint is being occupied by something else. If it is, change the spawn point position, update sphere collider and wait 2 seconds before trying again.
        while (spawnColliderCheck.Length > 0)
        {
            randomSpawnPointPick = Mathf.RoundToInt(Random.Range(1f, powerupSpawnPoints.Count));
            spawnColliderCheck = Physics.OverlapSphere(powerupSpawnPoints[randomSpawnPointPick].transform.position, 2, 8);
            yield return new WaitForSeconds(2);
        }

        // Chooses a random number to compare to the spawn chance. If it is greater or equal, the powerup will spawn.
        float powerupSpawnRoll = Random.Range(0f, 1f);

        if (powerupSpawnRoll >= difficultyConfig.PowerupChance)
        {
            // Creates the powerup at the powerup spawn location if it is free
            GameObject spawnedPowerup = Instantiate(PickObject(powerupSpawnPool), powerupSpawnPoints[randomSpawnPointPick].transform.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(7);
    }

    // Coroutine for periodically checking if all enemies have been destroyed
    private IEnumerator IsWaveOver()
    {
        if (activeEnemies.Count == 0 && spawnEnemiesRoutine == null)
        {
            EndWave();
        }

        yield return new WaitForSeconds(4);
    }

    #endregion

    #endregion
}
