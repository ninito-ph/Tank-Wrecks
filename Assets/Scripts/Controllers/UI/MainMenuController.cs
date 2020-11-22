using UnityEngine;

public class MainMenuController : MenuBase
{
    protected override void Start()
    {
        // Runs the base's start method
        base.Start();

        // Resume the audio listener
        AudioListener.pause = false;
    }

    #region Custom Methods

    // Exits the game
    public void ExitGame()
    {
        // Exits the application or stops playmode
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Methods visible for Unity button API
    // Play button functionality
    public void PlayButton()
    {
        SwitchToScene("GreatForest", true);
    }

    // Credits button functionality
    public void CreditsButton()
    {
        SwitchToMenu("Credits Menu");
    }

    // Options button functionalty
    public void OptionsButton()
    {
        SwitchToMenu("Options Menu");
    }

    // Back to main menu button
    public void BackToMainMenu()
    {
        SwitchToMenu("Main Menu");
    }

    #endregion

}
