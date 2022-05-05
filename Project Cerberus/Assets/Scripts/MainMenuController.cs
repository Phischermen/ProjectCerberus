/*
 * MainMenuController contains actions to be executed when buttons from the main menu are pressed. It is also
 * responsible for instantiating the level select screen and populating it with instances of LevelChoice.
 */

using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviourPun
{
    public static LevelSequence chosenLevelSequence = CustomProjectSettings.i.mainLevelSequence;
    public static int availableLevels = 0;
    public static bool silenceTutorials;
    public static bool silenceStory;

    public AudioClip mainMenuMusic;
    public GameObject levelChoiceButton;
    public GameObject worldContainer;
    public GameObject levelChoicePanel;
    public GameObject mainPanel;
    public GameObject multiplayerPanel;

    public GameObject multiplayerConnectionPanel;
    public GameObject multiplayerControlsPanel;

    public int displayedWorld = 0;
    public int maxWorld = 0;

    private List<GameObject> _worldContainers;
    public Button nextWorldButton;
    public Button prevWorldButton;
    public Button backToMenuButton;
    public Toggle silenceTutorialsToggle;
    public Toggle silenceStoryToggle;


    //#if UNITY_EDITOR
    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 200, 20), "Delete PlayerPrefs for levels"))
        {
            for (var index = 0; index < 100; index++)
            {
                PlayerPrefs.DeleteKey(CustomProjectSettings.i.mainLevelSequence.name + index);
                PlayerPrefs.DeleteKey(CustomProjectSettings.i.multiplayerLevelSequence.name + index);
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

    // [RuntimeInitializeOnLoadMethod]
    // static void OnRuntimeMethodLoad()
    // {
    //     chosenLevelSequence = CustomProjectSettings.i.mainLevelSequence;
    // }
    
    private void Awake()
    {
        // Initialize containers.
        _worldContainers = new List<GameObject>();
        // Load saved data.
        silenceTutorials = silenceTutorialsToggle.isOn = PlayerPrefs.GetInt("SilenceTutorials", 1) == 1;
        silenceStory = silenceStoryToggle.isOn = PlayerPrefs.GetInt("SilenceTutorials", 1) == 1;

        // Level selection panel is ready, but it's not the initial screen. Deactivate it.
        levelChoicePanel.SetActive(false);
        multiplayerPanel.SetActive(false);
    }

    private void Start()
    {
        // Play music.
        DiskJockey.PlayTrack(mainMenuMusic);
    }

    public void DestroyLevelSelectPanel()
    {
        foreach (var container in _worldContainers)
        {
            Destroy(container);
        }
    }

    public void BuildLevelSelectPanel(LevelSequence levelSequence)
    {
        chosenLevelSequence = levelSequence;
        // Load Saved Data
        availableLevels = PlayerPrefs.GetInt(levelSequence.name + "AvailableLevels", 0);
        // Initialize Level Select Panel
        InstantiateWorldsAndLevelChoiceButtons(levelSequence);
        // Deactivate all but the first world container.
        foreach (var container in _worldContainers)
        {
            container.SetActive(false);
        }

        _worldContainers[0].SetActive(true);
        // Initialize remaining UI components for Level Select Panel.
        prevWorldButton.interactable = false;
        nextWorldButton.interactable = true;
    }

    private void InstantiateWorldsAndLevelChoiceButtons(LevelSequence levelSequence)
    {
        var idx = 0;
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
                choice.settings = PlayerPrefs.GetString(levelSequence.name + level.x, TrophyData.initialTrophyCode);
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
        chosenLevelSequence = CustomProjectSettings.i.mainLevelSequence;
        BuildLevelSelectPanel(CustomProjectSettings.i.mainLevelSequence);
        levelChoicePanel.SetActive(true);
    }

    public void MultiplayerPressed()
    {
        mainPanel.SetActive(false);
        // Take this opportunity to set the right level sequence. This is hacky, but it's fine as long as there's just
        // one level sequence for multiplayer (no plans to make DLC).
        chosenLevelSequence = CustomProjectSettings.i.multiplayerLevelSequence;
        multiplayerPanel.SetActive(true);
    }

    public void ShowLevelSelectPanelForMultiplayer()
    {
        multiplayerPanel.SetActive(false);
        BuildLevelSelectPanel(CustomProjectSettings.i.multiplayerLevelSequence);
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

    public void BackToMenuPressed()
    {
        PhotonNetwork.Disconnect();
        mainPanel.SetActive(true);
        DestroyLevelSelectPanel();
        levelChoicePanel.SetActive(false);
        multiplayerPanel.SetActive(false);
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

    public void SendRPCSyncLobbySettings()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RPCSyncLobbySettings), RpcTarget.Others, silenceStory, silenceTutorials);
        }
    }

    [PunRPC]
    public void RPCSyncLobbySettings(bool pSilenceStory, bool pSilenceTutorials)
    {
        silenceStory = pSilenceStory;
        silenceTutorials = pSilenceTutorials;
    }
}