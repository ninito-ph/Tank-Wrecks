// A class that carries data through scenes
public static class GlobalData
{
    #region Field Declarations
    
    // The next scene to load
    private static string sceneToLoad = null;
    // The game's difficulty
    private static GameDifficultyObject gameDifficulty;
    // Stores the current ongoing game metrics
    // Uses a nullable struct because the game checks whether there is an ongoing game by checking if this variable is null or not
    private static LeaderboardEntry? currentGame;
    private static AchievementMetrics? currentAchievements;
    // Saves whether the player used a cheat
    private static bool cheated = false;

    #region Properties

    public static string SceneToLoad
    {
        get { return sceneToLoad; }
        set { sceneToLoad = value; }
    }

    public static GameDifficultyObject GameDifficulty
    {
        get { return gameDifficulty; }
        set { gameDifficulty = value; }
    }

    public static LeaderboardEntry? CurrentGame { get => currentGame; set => currentGame = value; }
    public static AchievementMetrics? CurrentAchievements { get => currentAchievements; set => currentAchievements = value; }
    public static bool Cheated { get => cheated; set => cheated = value; }

    #endregion

    #endregion
}