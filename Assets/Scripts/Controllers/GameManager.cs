using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
//using DebuggingEssentials;

public class GameManager : MonoBehaviour
{
    #region Field Declarations

    #region  Core Values

    private bool isPaused = false;
    private int wave = 0;
    private float score = 0;
    [Header("Difficulty")]
    [SerializeField]
    [Tooltip("The difficulty config profile to be used")]
    private GameDifficultyObject difficultyConfig;
    private Queue<GameObject> enemiesToSpawn = new Queue<GameObject>();
    private List<GameObject> activeEnemies = new List<GameObject>();
    [Header("Spawnpoints and level lists")]
    [SerializeField]
    [Tooltip("A list containing the individual spawn points for enemies")]
    private List<GameObject> enemySpawnPoints = new List<GameObject>();
    [SerializeField]
    [Tooltip("A list containing the individual spawn points for powerups")]
    private List<GameObject> powerupSpawnPoints = new List<GameObject>();

    [SerializeField]
    [Tooltip("The asset name for the next level in the level chain")]
    private string nextLevel;
    private List<GameObject> enemySpawnPool = new List<GameObject>();
    private List<GameObject> powerupSpawnPool = new List<GameObject>();

    // Coroutines for spawning and checking wave status
    private Coroutine spawnEnemiesRoutine;
    private Coroutine spawnPowerupsRoutine;
    private Coroutine garbageCollectorRoutine;

    // Caches a player and main camera reference
    private PlayerController playerReference;
    private GameObject mainCameraReference;

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

    public PlayerController PlayerReference
    {
        get { return playerReference; }
    }

    public GameObject MainCameraReference { get => mainCameraReference; }

    public bool IsPaused
    {
        get { return isPaused; }
        set
        {
            // Only perform actions if the pause value provided is different to the one provided
            if (isPaused != value)
            {
                // Equals the pause value to the one provided
                isPaused = value;
                // Notifies the game has been paused
                EventBroker.CallPauseToggled();
                // Pauses the game
                PauseGame();
            }
        }
    }


    #endregion

    #endregion

    #region Unity Methods

    private void Awake()
    {
        // Gets a reference of playerController and main camera so other classes can access it
        playerReference = GameObject.Find("Tank Player").GetComponent<PlayerController>();
        mainCameraReference = Camera.main.gameObject;

        //HACK: For some reason, when returning to the game scene from the menu, the TimeScale starts at 0
        // Setting it to 1 works, but ideally we should find what is setting it to 0 in the first place
        Time.timeScale = 1;
    }

    private void Start()
    {
        // Subscribes RemoveEnemyFromList to EnemyDestroyed event
        EventBroker.EnemyDestroyed += RemoveEnemyFromList;
        EventBroker.AddScore += AddScore;

        // Starts the game
        StartGame();

    }

    // OnDestroy runs once before the gameObject is destroyed
    private void OnDestroy()
    {
        // Unsubscribes from events to prevent memory leaks and other behaviours
        EventBroker.EnemyDestroyed -= RemoveEnemyFromList;
        EventBroker.AddScore -= AddScore;

        // Stops all coroutines
        StopAllCoroutines();
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        // If the debug option has been turned on
        if (displaySpawnSpheres == true)
        {
            // Draws wire sphere on spawn point areas
            Gizmos.color = Color.green;
            foreach (GameObject powerupSpawnPoint in powerupSpawnPoints)
            {
                Gizmos.DrawWireSphere(powerupSpawnPoint.transform.position, 2f);
            }

            Gizmos.color = Color.red;
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

    // Runs before the first wave in the level
    private void StartGame()
    {
        // Checks if an ongoing game already exists
        if (GlobalData.CurrentGame.HasValue == true)
        {
            // Loads the survived waves from the previous scene
            wave = GlobalData.CurrentGame.Value.wavesSurvived;
            // Loads the score from the previous scene
            score = GlobalData.CurrentGame.Value.playerScore;
        }
        else // If one doesn't, create a new one
        {
            GlobalData.CurrentGame = new LeaderboardEntry(wave, (int)score, "");
        }

        // Initial populate spawnpools
        PopulateEnemySpawnpool();
        PopulatePowerupSpawnpool();
        // Begins the wave
        BeginWave();
        // Starts the manual garbage collector
        garbageCollectorRoutine = StartCoroutine(CollectGarbage());
    }

    // Runs after the player dies, handles the end of the game
    public void EndGame(string sceneName = "MenuScene")
    {
        // Stops all coroutines
        StopAllCoroutines();

        // Unpauses the game
        Time.timeScale = 1;

        // Goes to requested screen
        // Set the scene to load data to the desired scene
        GlobalData.SceneToLoad = sceneName;
        // Load the loading screen scene
        SceneManager.LoadScene("LoadingScene");
    }

    // Begins next wave
    private void BeginWave()
    {
        // Increases wave
        wave++;

        // Updates the enemy amount
        difficultyConfig.UpdateEnemyAmount(wave, 30);

        // Spawns enemies & powerups
        spawnEnemiesRoutine = StartCoroutine(SpawnEnemy());
        spawnPowerupsRoutine = StartCoroutine(SpawnPowerup());

        // Adds to score based on previous wave. Does not happen on the first wave
        if (wave != 1)
        {
            AddScore(difficultyConfig.WaveCompleteBonus);
        }

        // Calls WaveOver event
        EventBroker.CallWaveStarted();
    }

    // Ends current wave
    private void EndWave()
    {
        // Checks if gameobject is null first to prevent null reference
        // exceptions
        if (this != null)
        {
            // Stops powerups and enemies from spawning
            StopCoroutine(spawnPowerupsRoutine);
            StopCoroutine(spawnEnemiesRoutine);
        }

        // Checks if the wave is a multiple of ten. If so, change the level
        if (wave % 10 == 0)
        {
            ChangeLevel();
        }
        else
        {
            // Starts next wave
            BeginWave();
        }

    }

    // Loads the next level in the game
    private void ChangeLevel(bool gotoMenu = false)
    {
        // If we must go to the menu
        if (gotoMenu == true)
        {
            GlobalData.SceneToLoad = "MenuScene";
            SceneManager.LoadScene("LoadingScene");
        }
        else // Otherwise, simply load the next level of the game
        {
            // Saves current player metrics
            GlobalData.CurrentGame = new LeaderboardEntry(wave, (int)score, null);

            //TODO: Add a transition between scenes

            // Loads the next level
            GlobalData.SceneToLoad = nextLevel;
            SceneManager.LoadScene("LoadingScene");
        }

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
        if (activeEnemies.Count == 0 && enemiesToSpawn.Count == 0)
        {
            // Ends wave
            EndWave();
        }
    }

    private void PauseGame()
    {
        // If the game is paused
        if (isPaused == true)
        {
            // Freeze time and suspend audio
            Time.timeScale = 0f;
            AudioListener.pause = true;
        }
        else // If the game is resumed
        {
            // Set timescale to 1 and unpause it
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }

    #region Coroutines

    // Coroutine for periodically spawning enemies. Ends itself once it finishes spawning things.
    private IEnumerator SpawnEnemy()
    {
        // Declares waitforseconds only once to reduce garbage colletion
        WaitForSeconds retrySpawnCooldown = new WaitForSeconds(3f);

        // Initial delay before spawning anything        
        yield return retrySpawnCooldown;

        // Enqueues enemies to spawn
        for (int queueEntries = 0; queueEntries < difficultyConfig.EnemyAmount; queueEntries++)
        {
            enemiesToSpawn.Enqueue(PickObject(enemySpawnPool));
        }

        // Spawns enemies until queue is empty
        while (enemiesToSpawn.Count > 0)
        {
            // Picks a random number in the enemySpawnPoints list to choose the spawn locations
            int randomSpawnPointPick = Random.Range(0, enemySpawnPoints.Count);

            // Makes a layer mask out TankBodies layer to perform physics raycasts
            LayerMask tankMask = LayerMask.GetMask("TankBodies", "Tanks");

            // FIXME: This generates a considerable amount of garbage and runs frequently enought to be a problem
            // Defines a sphere to check if the spawn point is being occupied.
            // The radius 5 is the nearest integer radius that can fit an entire
            // tank inside it.
            Collider[] spawnColliderCheck = Physics.OverlapSphere(enemySpawnPoints[randomSpawnPointPick].transform.position, 4, tankMask.value);

            // Checks if the spawnpoint is being occupied by something else. If
            // it is, change the spawn point position, update sphere collider
            // and wait 3 seconds before trying again.
            while (spawnColliderCheck.Length > 0)
            {
                // Tries for a new spawn point
                randomSpawnPointPick = Random.Range(0, enemySpawnPoints.Count);

                // FIXME: This generates a considerable amount of garbage and runs frequently enought to be a problem
                // Checks if anything is in the collision sphere
                spawnColliderCheck = Physics.OverlapSphere(enemySpawnPoints[randomSpawnPointPick].transform.position, 4, tankMask.value);

                // Waits before trying again
                yield return retrySpawnCooldown;
            }

            // Instantiates the enemy at the front of the queue
            GameObject spawnedEnemy = Instantiate(enemiesToSpawn.Dequeue(), enemySpawnPoints[randomSpawnPointPick].transform.position, Quaternion.identity);

            // Passes the spawnedEnemy its reference and the player's reference.
            EnemyController spawnedEnemyController = spawnedEnemy.GetComponent<EnemyController>();
            spawnedEnemyController.AssignedReference = spawnedEnemy;
            spawnedEnemyController.PlayerReference = playerReference;

            // Adds the spawnedEnemy to the activeEnemy list
            activeEnemies.Add(spawnedEnemy);
        }

        yield break;
    }

    // Coroutine for periodically spawning powerups. Loops.
    private IEnumerator SpawnPowerup()
    {
        // Declares waitforseconds only once to reduce garbage colletion
        WaitForSeconds spawnCooldown = new WaitForSeconds(difficultyConfig.PowerupSpawnCooldown);
        WaitForSeconds retrySpawnCooldown = new WaitForSeconds(2f);

        // Runs the powerup routine cyclically, until it is stopped
        while (true)
        {
            // Picks a random number in the powerupSpawnPoints list to choose the spawn location
            int randomSpawnPointPick = Random.Range(0, powerupSpawnPoints.Count);

            // Makes a layer mask out of the Powerups layer to perform physics raycasts
            LayerMask powerupMask = LayerMask.GetMask("Powerups");

            // Defines a sphere to check if the spawn point is being occupied
            // FIXME: This generates a considerable amount of garbage and runs frequently enought to be a problem
            Collider[] spawnColliderCheck = Physics.OverlapSphere(powerupSpawnPoints[randomSpawnPointPick].transform.position, 2, powerupMask.value, QueryTriggerInteraction.Collide);

            // Checks if the spawnpoint is being occupied by something else. If it is, change the spawn point position, update sphere collider and wait 2 seconds before trying again.
            while (spawnColliderCheck.Length > 0)
            {
                // Tries for a new spawn point
                randomSpawnPointPick = Random.Range(0, powerupSpawnPoints.Count);

                // FIXME: This generates a considerable amount of garbage and runs frequently enought to be a problem
                // Checks if anything is in the collision sphere
                spawnColliderCheck = Physics.OverlapSphere(powerupSpawnPoints[randomSpawnPointPick].transform.position, 2, powerupMask.value, QueryTriggerInteraction.Collide);

                // Waits before trying again
                yield return retrySpawnCooldown;
            }

            // Creates the powerup at the powerup spawn location if it is free
            GameObject spawnedPowerup = Instantiate(PickObject(powerupSpawnPool), powerupSpawnPoints[randomSpawnPointPick].transform.position, Quaternion.Euler(0f, 0f, -24f));

            // Gives the powerup a reference to the GameManager
            PowerupController powerupController = spawnedPowerup.GetComponent<PowerupController>();
            powerupController.MainCamera = mainCameraReference;

            yield return spawnCooldown;
        }
    }

    // Collects garbage periodically
    private IEnumerator CollectGarbage()
    {
        // Collect garbage every 1.5 seconds
        WaitForSecondsRealtime collectInterval = new WaitForSecondsRealtime(1.5f);
        bool needsMoreCollection = true;

        // Loops periodically
        while (true)
        {
            // Collects the garbage incrementally
            while (needsMoreCollection)
            {
                needsMoreCollection = GarbageCollector.CollectIncremental(2000000);
            }

            // Waits for next interval
            yield return collectInterval;
        }
    }
    #endregion
    #endregion
}