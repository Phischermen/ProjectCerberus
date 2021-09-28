/*
 * PuzzleUIEndCardFailure controls what is displayed on the failure end card, and has a button to reset the level.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleUIEndCardFailure : MonoBehaviour
{
    private GameManager _gameManager;
    // TODO Samuel (When it's ready): List UI that displays various
    // game over cards.

    private void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();
    }

    // Button actions
    public void Retry()
    {
        _gameManager.ReplayLevel();
        // TODO Play animation to hide UI instead of imediately destroying it.
        Destroy(gameObject);
    }
}
