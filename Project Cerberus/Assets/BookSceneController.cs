/*
 * Some levels only consist of flipping through a book to get some context. This script hooks up the book so that it
 * progresses to the next level when the last page is reached.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BookSceneController : MonoBehaviour
{
    [HideInInspector] public BookCanvas book;

    [HideInInspector] public LevelSequence levelSequence;
    [HideInInspector] public int currentLevel;
    // Start is called before the first frame update
    void Start()
    {
        book = FindObjectOfType<BookCanvas>();
        book.OnNextPressedOnLastPage += OnNextPressedOnLastPage;
        
        levelSequence = MainMenuController.chosenLevelSequence;
        currentLevel = levelSequence.FindCurrentLevelSequence(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnNextPressedOnLastPage()
    {
        var nextScene = levelSequence.GetSceneBuildIndexForLevel(currentLevel + 1, andPlayMusic: true);
        if (nextScene == -1)
        {
            Debug.Log($"Could not find next level {currentLevel + 1}");
        }
        else
        {
            currentLevel += 1;
            SceneManager.LoadScene(nextScene);
        }

        GameManager.currentLevel = currentLevel;
    }
}
