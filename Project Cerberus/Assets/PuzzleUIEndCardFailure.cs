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

    public void UndoLastMove()
    {
        _gameManager.UndoLastMove();
        Destroy(gameObject);
    }
}
