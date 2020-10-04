using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A class that carries data through scenes
public static class LoadData
{
    #region Field Declarations
    // The next scene to load
    private static string sceneToLoad = null;

    #region Properties

    public static string SceneToLoad
    {
        get { return sceneToLoad; }
        set { sceneToLoad = value; }
    }

    #endregion

    #endregion
}