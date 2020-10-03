using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MenuBase
{
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

    // Base class methods are re-implemented because Unity's button component
    // does not see inherited methods for the OnClick() event
    public override void SwitchToMenu(string desiredMenu, bool mouseEnabled = false)
    {
        base.SwitchToMenu(desiredMenu, mouseEnabled);
    }

    public override void SwitchToScene(string sceneName, bool useLoadingScreen)
    {
        base.SwitchToScene(sceneName, true);
    }

    #endregion

}
