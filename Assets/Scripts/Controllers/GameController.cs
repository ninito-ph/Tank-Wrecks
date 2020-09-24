using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    #region Field Declarations

    #region  Core Values

    private int wave = 0;
    private float score = 0;
    [Header("Difficulty")]
    [SerializeField]
    [Tooltip("The difficulty config profile to be used")]
    private GameDifficultySO difficultyConfig;
    private Queue<GameObject> enemiesToSpawn = new Queue<GameObject>();
    private List<GameObject> activeEnemies = new List<GameObject>();
    [Header("Spawnpoints and level lists")]
    [SerializeField]
    [Tooltip("A list containing the individual spawn points for enemies")]
    private List<GameObject> enemySpawnPoints = new List<GameObject>();
    [SerializeField]
    [Tooltip("A list containing the individual spawn points for powerups")]
    private List<GameObject> powerupSpawnPoints = new List<GameObject>();

    // 
    [SerializeField]
    [Tooltip("A list containing the scenes for each level in the game")]
    private List<Scene> gameLevels = new List<Scene>();
    private List<GameObject> enemySpawnPool = new List<GameObject>();
    private List<GameObject> powerupSpawnPool = new List<GameObject>();

    // Coroutines for spawning and checking wave status
    private Coroutine spawnEnemiesRoutine;
    private Coroutine spawnPowerupsRoutine;
    private Coroutine isWaveOverRoutine;

    // Caches a player reference
    private GameObject playerReference;

#if UNITY_EDITOR

    [Header("Debug")]
    [SerializeField]
    [Tooltip("Whether to display a wire sphere indicating the spawn areas for powerups and enemies")]
    private bool displaySpawnSpheres = false;

#endif

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

    public GameObject PlayerReference
    {
        get { return playerReference; }
    }

    #endregion

    #endregion

    #region Unity Methods

    private void Awake()
    {
        // Gets a reference of playerController so other classes can access it
        playerReference = GameObject.Find("Tank (Player)");
    }

    private void Start()
    {
        // Subscribes RemoveEnemyFromList to EnemyDestroyed event
        // NOTE: We're not sure whether a method actually receives an action's parameters, so this may cause a bug. make sure to debug this.
        EventBroker.EnemyDestroyed += RemoveEnemyFromList;
        EventBroker.AddScore += AddScore;

        // Starts the game
        StartGame();
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        // If the debug option has been turned on
        if (displaySpawnSpheres == true)
        {
            // Draws wire sphere on spawn point areas
            Gizmos.color = Color.cyan;
            foreach (GameObject powerupSpawnPoint in powerupSpawnPoints)
            {
                Gizmos.DrawWireSphere(powerupSpawnPoint.transform.position, 2f);
            }

            Gizmos.color = Color.magenta;
            foreach (GameObject enemySpawnPoint in enemySpawnPoints)
            {
                Gizmos.DrawWireSphere(enemySpawnPoint.transform.position, 5f);
            }
        }
    }

#endif

    #endregion

    #region Custom Methods

    // Adds score
    private void AddScore(float scoreAmount)
    {
        score += scoreAmount * difficultyConfig.DifficultyScoreModifier + wave * 20;
    }

    // Runs before the first wave in the game
    private void StartGame()
    {
        // Initial populate spawnpools
        PopulateEnemySpawnpool();
        PopulatePowerupSpawnpool();
        // Begins the first wave
        BeginWave();
    }

    // Runs after the player dies
    private void EndGame()
    {
        // TODO: Add game end effects, save game metrics and return to main menu
    }

    // Begins next wave
    private void BeginWave()
    {
        // Spawns enemies & powerups
        spawnEnemiesRoutine = StartCoroutine(SpawnEnemy());
        spawnPowerupsRoutine = StartCoroutine(SpawnPowerup());

        // Adds to score based on previous wave. Does not happen on the first wave
        if (wave != 1)
        {
            AddScore(difficultyConfig.WaveCompleteBonus);
        }

        // Increases wave
        wave++;
        // Calls WaveOver event
        EventBroker.CallWaveStarted();
    }

    // Ends current wave
    private void EndWave()
    {
        // Stops powerups and enemies from spawning
        StopCoroutine(spawnPowerupsRoutine);
        StopCoroutine(spawnEnemiesRoutine);
        // TODO: Add a grace period inbetween waves
        // Starts next wave
        BeginWave();
        // Checks if the wave is a multiple of ten. If so, change the level
        if (wave % 10 == 0)
        {
            NextLevel();
        }
    }

    // Loads the next level in the game
    private void NextLevel()
    {
        //TODO: Add a transition between scenes
        //TODO: Evaluate whether the scenes take long enough to load to the point where they would need a loading screen

        // Gets the current scene and its index on the list
        Scene currentScene = SceneManager.GetActiveScene();
        int levelIndex = gameLevels.FindIndex(Scene => Scene == currentScene);

        // Stores the index of the next scene to be loaded
        int levelToLoadIndex;

        // Checks to see if there actually is a next level, or whether the game should reset back to the first level
        if (levelIndex + 1 > gameLevels.Count)
        {
            // Sends the player back to the first level
            levelToLoadIndex = 0;
        }
        else
        {
            // Sends the player to the next level
            levelToLoadIndex = levelIndex + 1;
        }

        // Loads the next level
        SceneManager.LoadScene(gameLevels[levelToLoadIndex].name);
    }

    // Picks an object from a list of spawnables
    private GameObject PickObject(List<GameObject> spawnPool)
    {
        // Picks a random number to pick from the index
        int randomIndexPick = Random.Range(0, 99);

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

    // Removes and enemy from the list and ends the wave if the list is empty
    private void RemoveEnemyFromList(GameObject enemyReference)
    {
        // Removes the enemy from the list
        activeEnemies.Remove(enemyReference);

        // Checks if all enemies are dead
        if (activeEnemies.Count == 0)
        {
            // Ends wave
            EndWave();
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
            // Picks a random number in the enemySpawnPoints list to choose the spawn locations
            int randomSpawnPointPick = Random.Range(0, enemySpawnPoints.Count - 1);

            // Makes a layer mask out TankBodies layer to perform physics raycasts
            LayerMask tankMask = LayerMask.GetMask("TankBodies", "Tanks");

            // Defines a sphere to check if the spawn point is being occupied. The radius 5 is the nearest integer radius that can fit an entire tank inside it.
            Collider[] spawnColliderCheck = Physics.OverlapSphere(enemySpawnPoints[randomSpawnPointPick].transform.position, 5, tankMask.value);

            // Checks if the spawnpoint is being occupied by something else. If it is, change the spawn point position, update sphere collider and wait 3 seconds before trying again.
            while (spawnColliderCheck.Length > 0)
            {
                // Tries for a new spawn point
                randomSpawnPointPick = Random.Range(0, enemySpawnPoints.Count - 1);

                // Checks if anything is in the collision sphere
                spawnColliderCheck = Physics.OverlapSphere(enemySpawnPoints[randomSpawnPointPick].transform.position, 2, tankMask.value);

                // Waits before trying again
                yield return new WaitForSeconds(3);
            }

            // Instantiates the enemy at the front of the queue
            GameObject spawnedEnemy = Instantiate(enemiesToSpawn.Dequeue(), enemySpawnPoints[randomSpawnPointPick].transform.position, Quaternion.identity);
            // Passes the spawnedEnemy its reference.
            IEnemy spawnedEnemyBase = spawnedEnemy.GetComponent<EnemyBase>();
            spawnedEnemyBase.AssignedReference = spawnedEnemy;
            // Adds the spawnedEnemy to the activeEnemy list
            activeEnemies.Add(spawnedEnemy);
        }

        yield break;
    }

    // Coroutine for periodically spawning powerups. Loops.
    private IEnumerator SpawnPowerup()
    {
        // FIXME: The spawnpowerup routine does not use layermasks properly.

        // Picks a random number in the powerupSpawnPoints list to choose the spawn location
        int randomSpawnPointPick = Random.Range(0, powerupSpawnPoints.Count - 1);
        // Defines a sphere to check if the spawn point is being occupied
        var spawnColliderCheck = Physics.OverlapSphere(enemySpawnPoints[randomSpawnPointPick].transform.position, 2, 8);

        // Checks if the spawnpoint is being occupied by something else. If it is, change the spawn point position, update sphere collider and wait 2 seconds before trying again.
        while (spawnColliderCheck.Length > 0)
        {
            randomSpawnPointPick = Mathf.RoundToInt(Random.Range(0f, powerupSpawnPoints.Count));
            spawnColliderCheck = Physics.OverlapSphere(powerupSpawnPoints[randomSpawnPointPick].transform.position, 2, 8);
            yield return new WaitForSeconds(2);
        }

        // Chooses a random number to compare to the spawn chance. If it is greater or equal, the powerup will spawn.
        float powerupSpawnRoll = Random.Range(0f, 1f);

        if (powerupSpawnRoll >= difficultyConfig.PowerupChance)
        {
            // Creates the powerup at the powerup spawn location if it is free
            GameObject spawnedPowerup = Instantiate(PickObject(powerupSpawnPool), powerupSpawnPoints[randomSpawnPointPick].transform.position, Quaternion.identity);
            // Gives the powerup a reference to the gamecontroller
            PowerupController powerupController = spawnedPowerup.GetComponent<PowerupController>();
            powerupController.GameControllerRef = this;
        }

        yield return new WaitForSeconds(7);
    }

    #endregion

    #endregion
}
