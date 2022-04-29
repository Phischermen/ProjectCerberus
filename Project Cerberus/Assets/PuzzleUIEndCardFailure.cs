/*
 * PuzzleUIEndCardFailure controls what is displayed on the failure end card, and has a button to reset the level.
 */

using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleUIEndCardFailure : MonoBehaviour
{
    private GameManager _gameManager;
    
    public Button proceedButton;
    public Button replayButton;
    public Button undoLastMoveButton;

    private void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();
    }
    
    private void Update()
    {
        // Configure buttons
        if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
        {
            proceedButton.interactable = false;
            replayButton.interactable = false;
            undoLastMoveButton.interactable = false;
        }
        else
        {
            proceedButton.interactable = true;
            replayButton.interactable = true;
            undoLastMoveButton.interactable = true;
        }
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
