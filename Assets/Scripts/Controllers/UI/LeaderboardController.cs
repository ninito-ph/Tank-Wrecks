using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardController : MonoBehaviour
{

    #region Field Declarations

    [Header("Leaderboard Elements")]
    [SerializeField]
    [Tooltip("The number of entries to be loaded into the leaderboard")]
    private float entryAmount = 15;
    [Tooltip("The height of a leaderboard entry")]
    [SerializeField]
    private float templateHeight = 13.4f;
    [Tooltip("The container for a leaderboard entry")]
    [SerializeField]
    private Transform entryContainer;
    [SerializeField]
    [Tooltip("The entry template for a leaderboard entry")]
    private Transform entryTemplate;

    // Stores the leaderboard object
    private Leaderboard leaderboard;
    // Stores the transforms of each entry in the leaderboard
    private List<Transform> entryTransforms = new List<Transform>();

    #endregion

    #region Unity Methods

    // Start is called before the first frame update
    void Start()
    {
        // Disables the template
        entryTemplate.gameObject.SetActive(false);

        // Loads the leaderboard file
        LoadLeaderboard();

        // Sorts the list in place using lambda magic
        leaderboard.gameLeaderboard.Sort((entryA, entryB) => entryB.playerScore.CompareTo(entryA.playerScore));

        // Adds leaderboard entries for each entry in the gameLeaderboard, up to the amount defined in entryAmount
        for (int entry = 1; entry <= Mathf.Min(entryAmount, leaderboard.gameLeaderboard.Count); entry++)
        {
            AddEntry(leaderboard.gameLeaderboard[entry - 1], entryTransforms);
        }

    }

    #endregion

    #region Custom Methods

    private void LoadLeaderboard()
    {
        // Caches the filepath the JSON file will be saved to
        string filepath = Application.persistentDataPath + "/Leaderboard.json";

        // Checks if a save file exists
        if (File.Exists(filepath))
        {
            // String to store the data the streamReader recovers.
            string readData = "";

            // Get the leaderboard JSON save file
            using (StreamReader streamReader = new StreamReader(filepath))
            {
                // Declares a temporary line to store things in
                string line;

                // While the line is different to null (ReadLine is actually reading something)
                while ((line = streamReader.ReadLine()) != null)
                {
                    // Add the line to readData
                    readData += DataCrypto.DecryptText(line);
                }
            }

            // Reconstructs the leaderboard
            leaderboard = JsonUtility.FromJson<Leaderboard>(readData);
        }
        else // If no save file exists
        {
            // Creates a sample leaderboard entry list
            List<LeaderboardEntry> sampleEntryList = new List<LeaderboardEntry>()
            {
                new LeaderboardEntry(59, 61450, 1,"Delta VI"),
                new LeaderboardEntry(39, 40990, 1, "Libellula"),
                new LeaderboardEntry(34, 35400, 1,"Lazertank"),
                new LeaderboardEntry(91, 95300, 3,"Spark")
            };

            // Creates a leaderboard using the sample entry list
            leaderboard = new Leaderboard(sampleEntryList);
        }
    }

    // Adds an entry to the leaderboard
    private void AddEntry(LeaderboardEntry entryData, List<Transform> transformList)
    {
        // Instantiates an entry template at the entry container's location
        Transform entryTransform = Instantiate(entryTemplate, entryContainer);
        // Saves the entry's rect transform
        RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();

        // Descends the anchored position based on the entry's index
        entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * transformList.Count);

        // Fills entry text using entry data
        entryTransform.Find("Entry Player Name").GetComponent<Text>().text = entryData.playerName;
        entryTransform.Find("Entry Wave").GetComponent<Text>().text = entryData.wavesSurvived.ToString();
        entryTransform.Find("Entry Score").GetComponent<Text>().text = entryData.playerScore.ToString();

        // Alternates background opacity
        if (transformList.Count % 2 != 0)
        {
            entryTransform.Find("Entry Background").GetComponent<Image>().color = Color.clear;
        }

        // Adds the entry to the transform list
        transformList.Add(entryTransform);

        // Enables the gameObject
        entryTransform.gameObject.SetActive(true);
    }

    #endregion
}

#region Support Classes and Structs

// Struct that holds all the player data
// Uses System.Serializable to allow the struct to be converted to binary data
[System.Serializable]
public class Leaderboard
{
    [SerializeField]
    // List containing all the leaderboard entries
    public List<LeaderboardEntry> gameLeaderboard;

    // Constructor for the struct that initializes the leaderboard
    public Leaderboard(List<LeaderboardEntry> leaderboard)
    {
        // Equals the struct's (this) values to the ones passed as arguments
        this.gameLeaderboard = leaderboard;
    }
}

// Struct that holds player data generated after each playthrough
// Uses System.Serializable to allow the struct to be converted to binary data
[System.Serializable]
public struct LeaderboardEntry
{
    // Data saved when the game ends
    [SerializeField]
    public int wavesSurvived;
    [SerializeField]
    public int playerScore;
    [SerializeField]
    public int newGamePlus;
    [SerializeField]
    public string playerName;

    // Constructor for the struct that initializes relevant data 
    public LeaderboardEntry(int wavesSurvived, int playerScore, int newGamePlus, string playerName)
    {
        // Equals the struct's (this) values to the ones passed as arguments
        this.wavesSurvived = wavesSurvived;
        this.playerScore = playerScore;
        this.newGamePlus = newGamePlus;
        this.playerName = playerName;
    }
}

#endregion