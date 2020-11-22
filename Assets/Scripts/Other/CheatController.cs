using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheatController : MonoBehaviour
{

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        // Loads a scene
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            ChangeLevel("GreatForest");
            GlobalData.Cheated = true;
        }

        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            ChangeLevel("OldwoodTown");
            GlobalData.Cheated = true;
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            ChangeLevel("AncientCity");
            GlobalData.Cheated = true;
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            ChangeLevel("MenuScene");
            GlobalData.Cheated = true;
        }

        // Heals
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            PlayerController player = FindObjectOfType<PlayerController>();

            if (player != null)
            {
                player.Health = 3;
                GlobalData.Cheated = true;
            }
        }

        // Gives ammo
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            PlayerController player = FindObjectOfType<PlayerController>();

            if (player != null)
            {
                player.Ammo = 25;
                EventBroker.CallShotFired();
                GlobalData.Cheated = true;
            }
        }
    }

    // Loads the next level in the game
    private void ChangeLevel(string sceneToLoad)
    {
        // Loads the next level
        GlobalData.SceneToLoad = sceneToLoad;
        SceneManager.LoadScene("LoadingScene");
    }
}
