/*
 * PuzzleUIEndCardSuccess controls what is displayed on the success end card, and has a button to reset the level and a
 * button to proceed to the next level. The game calculates how many stars to reward the player for their performance,
 * using data from game manager.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleUIEndCardSuccess : MonoBehaviour
{
    private GameManager _gameManager;

    private void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();
        // Display stars
        
        // Display time
        
        // Display moves
    }

    // Button actions
    public void Retry()
    {
        _gameManager.ReplayLevel();
        // TODO Play animation to hide UI instead of immediately destroying it.
        Destroy(gameObject);
    }

    public void ProceedToNextLevel()
    {
        _gameManager.ProceedToNextLevel();
    }
}
