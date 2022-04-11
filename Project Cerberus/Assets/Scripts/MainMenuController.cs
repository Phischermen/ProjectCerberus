/*
 * MainMenuController contains actions to be executed when buttons from the main menu are pressed. It is also
 * responsible for instantiating the level select screen and populating it with instances of LevelChoice.
 */

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public static int availableLevels = 0;
    public static bool silenceTutorials;
    public static bool silenceStory;

    public AudioClip mainMenuMusic;
    public GameObject levelChoiceButton;
    public GameObject worldContainer;
    public GameObject levelChoicePanel;
    public GameObject mainPanel;

    public int displayedWorld = 0;
    public int maxWorld = 0;

    private List<GameObject> _worldContainers;
    public Button nextWorldButton;
    public Button prevWorldButton;
    public Toggle silenceTutorialsToggle;
    public Toggle silenceStoryToggle;


    //#if UNITY_EDITOR
    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 200, 20), "Delete PlayerPrefs for levels"))
        {
            for (var index = 0; index < 100; index++)
            {
                PlayerPrefs.DeleteKey(index.ToString());
                PlayerPrefs.Save();
            }
        }

        if (GUI.Button(new Rect(0, 20, 200, 20), "Delete PlayerPrefs entirely"))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
//#endif

    private void Awake()
    {
        // Load saved data.
        availableLevels = PlayerPrefs.GetInt("AvailableLevels", 0);
        silenceTutorials = silenceTutorialsToggle.isOn = PlayerPrefs.GetInt("SilenceTutorials", 1) == 1;
        silenceStory = silenceStoryToggle.isOn = PlayerPrefs.GetInt("SilenceTutorials", 1) == 1;
        // Initialize Level Select Panel
        InitializeLevelSelectPanel();
        // Deactivate all but the first world container.
        foreach (var container in _worldContainers)
        {
            container.SetActive(false);
        }

        _worldContainers[0].SetActive(true);
        // Initialize remaining UI components for Level Select Panel.
        prevWorldButton.interactable = false;
        nextWorldButton.interactable = true;
        // Level selection panel is ready, but it's not the initial screen. Deactivate it.
        levelChoicePanel.SetActive(false);
    }

    private void Start()
    {
        // Play music.
        DiskJockey.PlayTrack(mainMenuMusic);
    }

    private void InitializeLevelSelectPanel()
    {
        var idx = 0;
        // Load main level sequence.
        var levelSequence = CustomProjectSettings.i.mainLevelSequence;
        // Initialize container.
        _worldContainers = new List<GameObject>();
        // Create a container for the levels in each world.
        foreach (var world in levelSequence.worlds)
        {
            // Instantiate the world container. Add to _worldContainers.
            var newWorldContainer = Instantiate(worldContainer, levelChoicePanel.transform);
            _worldContainers.Add(newWorldContainer);
            // Create a level choice foreach level in each world.
            foreach (var level in world.levels)
            {
                // Instantiate the level choice.
                var newLevelChoice = Instantiate(levelChoiceButton, newWorldContainer.transform);
                var choice = newLevelChoice.GetComponent<LevelChoice>();
                // Set fields for LevelChoice.
                choice.levelIdx = idx;
                choice.sceneIdx = level.x;
                choice.settings = PlayerPrefs.GetString(level.ToString(), TrophyData.initialTrophyCode);
                choice.ApplySettings();
                choice.GetComponentInChildren<Text>().text = (idx).ToString();
                // Increment current level index.
                idx += 1;
            }
        }

        maxWorld = levelSequence.worlds.Count - 1;
    }

    // Menu actions
    // Main

    public void PlayPressed()
    {
        mainPanel.SetActive(false);
        levelChoicePanel.SetActive(true);
    }

    // Level Select
    public void NextWorldPressed()
    {
        if (displayedWorld < maxWorld)
        {
            _worldContainers[displayedWorld].SetActive(false);
            displayedWorld += 1;
            _worldContainers[displayedWorld].SetActive(true);
        }

        if (displayedWorld == maxWorld)
        {
            nextWorldButton.interactable = false;
        }

        prevWorldButton.interactable = true;
    }

    public void PrevWorldPressed()
    {
        if (displayedWorld > 0)
        {
            _worldContainers[displayedWorld].SetActive(false);
            displayedWorld -= 1;
            _worldContainers[displayedWorld].SetActive(true);
        }

        if (displayedWorld == 0)
        {
            prevWorldButton.interactable = false;
        }

        nextWorldButton.interactable = true;
    }

    public void SilenceTutorialsToggled(bool value)
    {
        PlayerPrefs.SetInt("SilenceTutorials", value ? 1 : 0);
        PlayerPrefs.Save();
        silenceTutorials = value;
    }
    
    public void SilenceStoryToggled(bool value)
    {
        PlayerPrefs.SetInt("SilenceStory", value ? 1 : 0);
        PlayerPrefs.Save();
        silenceStory = value;
    }
}