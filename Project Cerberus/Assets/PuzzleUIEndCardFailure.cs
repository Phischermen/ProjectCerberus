/*
 * PuzzleUIEndCardFailure controls what is displayed on the failure end card, and has a button to reset the level.
 */

using System;
using UnityEngine;

public class PuzzleUIEndCardFailure : MonoBehaviour
{
    private GameManager _gameManager;
    // TODO(Sam) Get UI Components.
    private void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();
    }

    [Serializable]
    public struct Endcard
    {
        public string title;
        public Sprite image;
    }

    [SerializeField] private Endcard[] endcards;

    public enum FailureEndcard
    {
        Spiked,
        FallIntoPit
    }

    public void DisplayEncard(FailureEndcard endcard)
    {
        var endcard1 = endcards[(int) endcard];
        // TODO(SAM) set UI component fields.
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

    private void OnDestroy()
    {
        _gameManager.gameOverEndCardDisplayed = false;
    }
}