/*
 * LevelChoice is a UI button for selecting which level to play. It is instantiated & initialized by MainMenuController.
 * LevelChoice displays which trophies the player has earned in each level. (NOT YET IMPLEMENTED)
 */

using System;
using Multiplayer;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelChoice : MonoBehaviour
{
    public int levelIdx;
    public int sceneIdx;
    public string settings;

    public Image timeTrophyImage;
    public Image moveTrophyImage;
    public Image bonusStarTrophyImage;

    public TrophyData timeTrophyData;
    public TrophyData moveTrophyData;
    public TrophyData bonusStarTrophyData;

    private void Start()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(ButtonPressed);
        // Make sure that the button is only interactable if the Player has reached this level.
        if (levelIdx <= MainMenuController.availableLevels)
        {
            button.interactable = true;
        }
        else
        {
            button.interactable = false;
        }
    }

    public void ApplySettings()
    {
        // Display stars
        timeTrophyImage.sprite = timeTrophyData.GetSpriteToDisplay(settings[0]);
        moveTrophyImage.sprite = moveTrophyData.GetSpriteToDisplay(settings[1]);
        bonusStarTrophyImage.sprite = bonusStarTrophyData.GetSpriteToDisplay(settings[2]);
    }

    private void ButtonPressed()
    {
        //Play music.
        MainMenuController.chosenLevelSequence.GetSceneBuildIndexForLevel(levelIdx, andPlayMusic: true);
        if (PhotonNetwork.InRoom)
        {
            FindObjectOfType<Launcher>().LevelSelectedForMultiplayer(sceneIdx);
        }
        else
        {
            SceneManager.LoadScene(sceneIdx);
        }
    }
}