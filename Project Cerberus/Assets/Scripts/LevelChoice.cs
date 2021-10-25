/*
 * LevelChoice is a UI button for selecting which level to play. It is instantiated & initialized by MainMenuController.
 * LevelChoice displays which trophies the player has earned in each level. (NOT YET IMPLEMENTED)
 */
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelChoice : MonoBehaviour
{
    public int levelIdx;
    public int sceneIdx;
    public string settings;

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
        // TODO Implement other settings
    }

    private void ButtonPressed()
    {
        SceneManager.LoadScene(sceneIdx);
    }
}
