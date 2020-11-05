using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpScreenController : MenuBase
{
    // GameManager reference cache
    private GameManager gameManager;

    private void OnEnable()
    {
        // Caches reference to the GameManager
        gameManager = GameObject.Find("Game Controller").GetComponent<GameManager>();

        // Marks the game as no its first startup
        PlayerPrefs.SetString("First Startup", "false");

        // Pauses the game
        gameManager.IsPaused = true;
    }

    // Update is called once per frame
    void Update()
    {
        // If any key is pressed
        if (Input.anyKeyDown)
        {
            // Unpauses the game
            gameManager.IsPaused = false;

            // Goes back to the hud
            SwitchToMenu("HUD", false);
        }
    }
}
