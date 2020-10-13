using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class GameOverController : MenuBase
{
    #region Field Declarations

    [Header("User Interface")]
    [SerializeField]
    [Tooltip("The amount of time in seconds that it takes for the game over screen to fade in.")]
    private float fadeInTime = 1f;

    [Tooltip("The texts present in the game over screen")]
    [SerializeField]
    private Text[] gameOverTexts;
    [Tooltip("The background image present in the game over screen")]
    [SerializeField]
    private Image gameOverBackground;
    [Tooltip("The text containing how many waves the player survived")]
    [SerializeField]
    private Text wavesSurvived;
    [Tooltip("The text containing how many points the player accrued")]
    [SerializeField]
    private Text pointsAccrued;
    [Tooltip("The text entry field used to input the player name")]
    [SerializeField]
    private Text playerName;

    // The coroutine that fades text and images
    private Coroutine fadeRoutine;

    // Caches reference to GameController
    private GameController gameController;

    #endregion

    #region Unity Methods

    // OnEnable is called when the object is enabled
    private void OnEnable()
    {
        // Gets reference to GameObject
        gameController = GameObject.Find("Game Controller").GetComponent<GameController>();

        // Gets the player metrics for the game and displays them on the screen
        GetPlayerMetrics();

        // Fades in the game over screen and its content over time
        fadeRoutine = StartCoroutine(Fade(1f, fadeInTime, gameOverTexts, gameOverBackground));
    }

    private void Update()
    {
        // If the player presses enter
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // Saves the player metric into a save file
            SavePlayerMetrics();
            // Go to menu
            SwitchToScene("MenuScene", true);
        }
    }

    #endregion

    #region Custom Methods

    // Gets the player metrics for the match, namely score and waves survived
    private void GetPlayerMetrics()
    {
        wavesSurvived.text = (gameController.Wave - 1).ToString();
        pointsAccrued.text = ((int)gameController.Score).ToString();
    }

    // Saves the player metrics in a save file using a JSON format
    private void SavePlayerMetrics()
    {
        // Caches the filepath the JSON file will be saved to
        string filepath = Application.persistentDataPath + "/Leaderboard.json";

        // Stores the game data into a LeaderboardEntry struct
        LeaderboardEntry playerMetrics = new LeaderboardEntry(gameController.Wave - 1, (int)gameController.Score, playerName.text);

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
            Leaderboard saveFile = JsonUtility.FromJson<Leaderboard>(readData);

            // Adds a the LeaderboardEntry to the list
            saveFile.gameLeaderboard.Add(playerMetrics);

            // Writes the data to disk
            // Using keyword allows us to dispose of the StreamWriter once we're done using it.
            using (StreamWriter streamWriter = new StreamWriter(filepath))
            {
                // Uses stream writer to write the JSON-converted save data.
                streamWriter.WriteLine(DataCrypto.EncryptText(JsonUtility.ToJson(saveFile)));
            }
        }
        else // If the file does not exist, create it
        {
            // Creates a new list of Leaderboard Entries
            List<LeaderboardEntry> newLeaderboard = new List<LeaderboardEntry>();
            // Adds the player's current save data to the leaderboard
            newLeaderboard.Add(playerMetrics);

            // Creates leaderboard and passes it the list containing the current save data
            Leaderboard saveFile = new Leaderboard(newLeaderboard);

            // Writes the data to disk
            // Using keyword allows us to dispose of the StreamWriter once we're done using it.
            using (StreamWriter streamWriter = new StreamWriter(filepath))
            {
                // Uses stream writer to write the JSON-converted save data.
                streamWriter.WriteLine(DataCrypto.EncryptText(JsonUtility.ToJson(saveFile)));
            }
        }
    }

    // Fades text to a given transparency
    private IEnumerator Fade(float transparency, float fadeDuration, Text[] textsToFade, Image imageToFade)
    {
        // Updates text color until fade duration is over
        for (float time = 0; time <= fadeDuration; time += Time.deltaTime)
        {
            // Sets the color to the lerp operation's result color
            foreach (Text gameOverText in textsToFade)
            {
                gameOverText.color = Color.Lerp(gameOverText.color, new Color(gameOverText.color.r, gameOverText.color.g, gameOverText.color.b, transparency), Mathf.Min(1, (time / fadeDuration)));
            }
            // Does the same for the background image
            imageToFade.color = Color.Lerp(imageToFade.color, new Color(imageToFade.color.r, imageToFade.color.g, imageToFade.color.b, transparency), Mathf.Min(1, (time / fadeDuration)));

            // Waits for the next frame
            yield return null;
        }

        // Ends coroutine
        yield break;
    }

    #endregion
}
