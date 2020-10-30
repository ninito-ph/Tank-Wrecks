using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A class that carries data through scenes
public static class GlobalData
{
    #region Field Declarations
    // The next scene to load
    private static string sceneToLoad = null;
    // The game's difficulty
    private static GameDifficultySO gameDifficulty;

    #region Properties

    public static string SceneToLoad
    {
        get { return sceneToLoad; }
        set { sceneToLoad = value; }
    }

    public static GameDifficultySO GameDifficulty
    {
        get { return gameDifficulty; }
        set { gameDifficulty = value; }
    }

    #endregion

    #endregion
}