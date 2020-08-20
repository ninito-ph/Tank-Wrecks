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
    private Queue<GameObject> powerupsToSpawn;
    private List<GameObject> activeEnemies;
    private List<GameObject> spawnPoints;
    private List<GameObject> spawnPool;
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

        // Creates instances of used lists
        spawnPool = new List<GameObject>();
        spawnPoints = new List<GameObject>();
        activeEnemies = new List<GameObject>();

        // Creates instances of used Queues
        enemiesToSpawn = new Queue<GameObject>();
        powerupsToSpawn = new Queue<GameObject>();
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
        // Increases wave
        wave ++;

        // Spawns enemies
        spawnEnemiesRoutine = StartCoroutine(SpawnEnemy(spawnEnemiesRoutine));



    }

    private void EndWave()
    {

    }

    private void NextLevel()
    {

    }

    // Picks an enemy from the list of spawnables
    private GameObject PickEnemy()
    {
        // Picks a random number to pick from the index
        int randomIndexPick = Mathf.RoundToInt(Random.Range(1f, 100f));

        // Randomly draw enemy type from spawnable enemy pool
        GameObject enemyToSpawn = spawnPool[randomIndexPick];
        return enemyToSpawn;
    }

    // Populates the list containing possibilities for spawning an enemy
    private void PopulateSpawnablesList(bool repopulate = false)
    {
        // If the list doesn't exist, create it.
        if (spawnPool == null)
        {
            spawnPool = new List<GameObject>();
        }

        // Checks to see if the list is meant to be REpopulated, meaning it needs to be cleared and populated again
        if (repopulate == true)
        {
            spawnPool.Clear();
        }

        //FIXME: This code is MONSTROUSLY better than our first implementation, but it still remains relatively inextensible
        // Populates list with roller tank entries
        for (int spawnEntry = 0; spawnEntry < difficultyConfig.RollerTankChance; spawnEntry ++)
        {
            spawnPool.Add(difficultyConfig.RollerTank);
        }

        // Populates list with speeder tank entries
        for (int spawnEntry = 0; spawnEntry < difficultyConfig.SpeederTankChance; spawnEntry++)
        {
            spawnPool.Add(difficultyConfig.SpeederTank);
        }

        // Populates list with smasher tank entries
        for (int spawnEntry = 0; spawnEntry < difficultyConfig.SmasherTankChance; spawnEntry++)
        {
            spawnPool.Add(difficultyConfig.SmasherTank);
        }

        // Populates list with annihilator tank entries
        for (int spawnEntry = 0; spawnEntry < difficultyConfig.AnnihilatorTankChance; spawnEntry++)
        {
            spawnPool.Add(difficultyConfig.AnnihilatorTank);
        }

        // Checks if list has more or less than 100 entries.
        if (spawnPool.Count > 100)
        {
            int listOverpopulationCount = spawnPool.Count - 100;
            Debug.LogWarningFormat("Overpopulated list by {0} entries. Full entry count at {1}", listOverpopulationCount, spawnPool.Count);
        }
        else if (spawnPool.Count < 100)
        {
            int listUnderpopulationCount = 100 - spawnPool.Count;
            Debug.LogWarningFormat("Underpopulated list by {0} entries. Full entry count at {1}", listUnderpopulationCount, spawnPool.Count);
        }
    }

    #region Coroutines

    // Coroutine for periodically spawning enemies
    private IEnumerator SpawnEnemy(Coroutine ownReference)
    {
        // Enqueues enemies to spawn
        for (int queueEntries = 0; queueEntries < difficultyConfig.EnemyAmount; queueEntries++)
        {
            enemiesToSpawn.Enqueue(PickEnemy());
        }

        // Spawns enemies until queue is empty
        while (enemiesToSpawn.Count > 0)
        {
            // Picks a random number in the spawnPoints list to choose the spawn location
            int randomSpawnPointPick = Mathf.RoundToInt(Random.Range(1f, spawnPoints.Count));
            // Defines a sphere to check if the spawn point is being occupied
            var spawnColliderCheck = Physics.OverlapSphere(spawnPoints[randomSpawnPointPick].transform.position, 2, 8);


            // Checks if the spawnpoint is being occupied by something else. If it is, change the spawn point position, update sphere collider and wait 3 seconds before trying again.
            while (spawnColliderCheck.Length > 0)
            {
                randomSpawnPointPick = Mathf.RoundToInt(Random.Range(1f, spawnPoints.Count));
                spawnColliderCheck = Physics.OverlapSphere(spawnPoints[randomSpawnPointPick].transform.position, 2, 8);
                yield return new WaitForSeconds(3);
            }

            // Instantiates the enemy at the front of the queue
            GameObject spawnedEnemy = Instantiate(enemiesToSpawn.Dequeue(), spawnPoints[randomSpawnPointPick].transform.position, Quaternion.identity);
            // Passes the spawnedEnemy its reference.
            EnemyBase spawnedEnemyBase = spawnedEnemy.GetComponent<EnemyBase>();
            spawnedEnemyBase.AssignedReference = spawnedEnemy;
            // Adds the spawnedEnemy to the activeEnemy list
            activeEnemies.Add(spawnedEnemy);
        }

        // The coroutine ends itself on completion of spawns
        StopCoroutine(ownReference);
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
