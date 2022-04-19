using UnityEngine;
using UnityEngine.SceneManagement;

public class PuzzleUIPause : MonoBehaviour
{
    private GameManager _gameManager;

    private void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();
    }

    // Button actions
    public void Unpause()
    {
        _gameManager.gameplayEnabled = true;
        Destroy(gameObject);
    }

    public void Retry()
    {
        _gameManager.ReplayLevel();
        // TODO Play animation to hide UI instead of imediately destroying it.
        Destroy(gameObject);
    }

    public void GoToMenu()
    {
        //NOTE: LeaveRoom() will run logic to return to main menu.
        if (!_gameManager.LeaveRoom())
        {
            SceneManager.LoadScene((int) Scenum.Scene.MainMenu);
        }
    }

    private void OnDestroy()
    {
        _gameManager.gameOverEndCardDisplayed = false;
    }
}