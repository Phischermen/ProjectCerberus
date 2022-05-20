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
using Level = LevelSequence.Level;

public class LevelChoice : MonoBehaviour
{
    public int levelIdx;
    public Level sceneIdx;
    public string settings;

    public Image timeTrophyImage;
    public Image moveTrophyImage;
    public Image bonusStarTrophyImage;
    public Image bookImage;

    public Text text;

    public TrophyData timeTrophyData;
    public TrophyData moveTrophyData;
    public TrophyData bonusStarTrophyData;

    private void Start()
    {
        // Setup button.
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
        // Display the correct set of ui elements.
        if (sceneIdx.isGameplay)
        {
            // Display stars
            timeTrophyImage.sprite = timeTrophyData.GetSpriteToDisplay(settings[0]);
            moveTrophyImage.sprite = moveTrophyData.GetSpriteToDisplay(settings[1]);
            bonusStarTrophyImage.sprite = bonusStarTrophyData.GetSpriteToDisplay(settings[2]);
            text.text = levelIdx.ToString();
        }
        else
        {
            timeTrophyImage.gameObject.SetActive(false);
            moveTrophyImage.gameObject.SetActive(false);
            bonusStarTrophyImage.gameObject.SetActive(false);
            text.gameObject.SetActive(false);
            // Note: The book is hidden by default.
            bookImage.gameObject.SetActive(true);
        }
    }

    private void ButtonPressed()
    {
        //Play music.
        MainMenuController.chosenLevelSequence.GetSceneBuildIndexForLevel(levelIdx, andPlayMusic: true);
        if (PhotonNetwork.InRoom)
        {
            FindObjectOfType<Launcher>().LevelSelectedForMultiplayer(sceneIdx.idxForInstancing);
        }
        else
        {
            SceneManager.LoadScene(sceneIdx.idxForInstancing);
        }
    }
}