/*
 * PuzzleUIEndCardSuccess controls what is displayed on the success end card, and has a button to reset the level and a
 * button to proceed to the next level. The game calculates how many stars to reward the player for their performance,
 * using data from game manager. One star is earned for clearing the level under a "par time." Another star is earned
 * for clearing the level making fewer moves than the set "move limit." The final star is rewarded for collecting the
 * bonus star, which is hidden in the level somewhere.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class PuzzleUIEndCardSuccess : MonoBehaviour
{
    private GameManager _gameManager;

    public Text timeText;
    public Text moveText;

    public Image timeTrophyImage;
    public Image moveTrophyImage;
    public Image bonusStarTrophyImage;

    // [Serializable]
    // public struct TropheyData
    // {
    //     public Sprite gold;
    //     public Sprite silver;
    //     public Sprite bronze;
    //     public Sprite nope;
    //
    //     public float silverBracket;
    //     public float bronzeBracket;
    // }

    public TrophyData timeTrophyData;
    public TrophyData moveTrophyData;
    public TrophyData bonusStarTrophyData;

    // public enum TrophyCodes
    // {
    //     Gold = 'a',
    //     Silver = 'b',
    //     Bronze = 'c',
    //     Nope = 'd'
    // }

    private void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();

        // Get currently owned trophies for level
        var currentLevelBuildIndex =
            GameManager.levelSequence.GetSceneBuildIndexForLevel(GameManager.currentLevel).ToString();
        var currentTrophies = PlayerPrefs.GetString(currentLevelBuildIndex, TrophyData.initialTrophyCode);
        // Calculate which time trophy was earned
        var codeOfTimeTrophyToDisplay =
            GetCodeOfTropheyToDisplay(currentTrophies[0], Mathf.Floor(_gameManager.timer), _gameManager.parTime,
                _gameManager.infiniteParTime);
        // Calculate which move trophy was earned
        var codeOfMoveTrophyToDisplay =
            GetCodeOfTropheyToDisplay(currentTrophies[1], _gameManager.move, _gameManager.maxMovesBeforeStarLoss,
                _gameManager.infinteMovesTilStarLoss);
        // Calculate bonus star trophy
        var currentlyOwnsBonusStarTrophy = currentTrophies[2] == TrophyData.goldCode;
        char codeOfBonusStarTrophyToDisplay =
            _gameManager.collectedStar || currentlyOwnsBonusStarTrophy ? TrophyData.goldCode : TrophyData.nopeCode;

        // Store the displayed trophies in a string.
        var data =
            $"{codeOfTimeTrophyToDisplay}{codeOfMoveTrophyToDisplay}{(codeOfBonusStarTrophyToDisplay)}";
        PlayerPrefs.SetString(GameManager.levelSequence.name + currentLevelBuildIndex, data);
        // Unlock the next level for level select screen.
        if (GameManager.currentLevel >= MainMenuController.availableLevels)
        {
            PlayerPrefs.SetInt(GameManager.levelSequence.name + "AvailableLevels", GameManager.currentLevel + 1);
        }

        // Save data to PlayerPrefs
        PlayerPrefs.Save();
        // Display trophies
        timeTrophyImage.sprite = timeTrophyData.GetSpriteToDisplay(codeOfTimeTrophyToDisplay);
        moveTrophyImage.sprite = moveTrophyData.GetSpriteToDisplay(codeOfMoveTrophyToDisplay);
        bonusStarTrophyImage.sprite = bonusStarTrophyData.GetSpriteToDisplay(codeOfBonusStarTrophyToDisplay);

        // Display time
        timeText.text = $"Time\n{Mathf.Floor(_gameManager.timer),3:0}s";
        timeText.color = Mathf.Floor(_gameManager.timer) <= _gameManager.parTime ? Color.yellow : Color.white;
        // Display moves
        moveText.text = $"Moves\n{_gameManager.move + 1}";
        moveText.color = _gameManager.move < _gameManager.maxMovesBeforeStarLoss ? Color.yellow : Color.white;
    }

    // Trophy helper function
    private char GetCodeOfTropheyToDisplay(char codeOfCurrentlyOwnedTrophy, float score, float parScore,
        bool automaticSuccess)
    {
        if (automaticSuccess)
        {
            return TrophyData.goldCode;
        }

        char codeOfTimeTrophyEarnedThisLevel =
            TrophyData.GetTropheyCode(score, parScore, parScore * 1.1f, parScore * 2.0f);
        if (codeOfTimeTrophyEarnedThisLevel < codeOfCurrentlyOwnedTrophy)
        {
            return codeOfCurrentlyOwnedTrophy;
        }

        return codeOfTimeTrophyEarnedThisLevel;
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