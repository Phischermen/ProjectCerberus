using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Multiplayer
{
    public class Launcher : MonoBehaviourPunCallbacks, IPunObservable
    {
        string gameVersion = "1";
        public static byte maxPlayersPerRoom = 3;

        public GameObject multiplayerConnectionPanel;

        public GameObject multiplayerControlsPanel;

        // NOTE: Both MainMenuController and this script have control over this variable's state.
        public GameObject levelSelectPanel;
        public GameObject dogSelectPanel;
        public GameObject dogSelectNavigationControls;
        public Button dogSelectPlayButton;

        public bool isConnecting;

        private Step gameStartSequenceStep;

        enum Step
        {
            enteringRoomName,
            connectingToRoom,
            selectingLevel,
            selectingDog
        }

        [SerializeField] private Text connectionStatusText;
        private MainMenuController _mainMenuController;

        private void Awake()
        {
            _mainMenuController = FindObjectOfType<MainMenuController>();
            PhotonNetwork.AutomaticallySyncScene = true;
            gameStartSequenceStep = Step.enteringRoomName;
            DisplayMenuForStep(Step.enteringRoomName);
        }

        public override void OnConnectedToMaster()
        {
            
            
            Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
            if (isConnecting)
            {
                PhotonNetwork.JoinRandomRoom();
                isConnecting = false;
            }
        }


        public override void OnDisconnected(DisconnectCause cause)
        {
            gameStartSequenceStep = Step.enteringRoomName;
            DisplayMenuForStep(Step.enteringRoomName);
            Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}",
                cause);
            isConnecting = false;
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            PhotonNetwork.CreateRoom(null, new RoomOptions
            {
                MaxPlayers = maxPlayersPerRoom
            });
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
            var currentRoom = PhotonNetwork.CurrentRoom;
            if (PhotonNetwork.IsMasterClient)
            {
                // Initialize custom properties.
                currentRoom.SetCustomProperties(new Hashtable()
                    {{"Jack", false}, {"Kahuna", false}, {"Laguna", false}});
                // Goto next screen.
                gameStartSequenceStep = Step.selectingLevel;
                DisplayMenuForStep(Step.selectingLevel);
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            DisplayMenuForStep(gameStartSequenceStep);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            DisplayMenuForStep(gameStartSequenceStep);
        }

        public void LevelSelectedForMultiplayer(int level)
        {
            MainMenuController.chosenLevelSceneIndex = level;
            gameStartSequenceStep = Step.selectingDog;
            DisplayMenuForStep(Step.selectingDog);
        }

        public void GoBackToLevelSelection()
        {
            gameStartSequenceStep = Step.selectingLevel;
            DisplayMenuForStep(Step.selectingLevel);
        }

        private void Connect()
        {
            gameStartSequenceStep = Step.connectingToRoom;
            DisplayMenuForStep(Step.connectingToRoom);
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.JoinRandomRoom();
            }

            else
            {
                isConnecting = PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = gameVersion;
            }
        }

        private void DisplayMenuForStep(Step step)
        {
            // Hide everything.
            multiplayerControlsPanel.SetActive(false);
            multiplayerConnectionPanel.SetActive(false);
            levelSelectPanel.SetActive(false);
            dogSelectPanel.SetActive(false);
            // Show menu panel.
            switch (step)
            {
                case Step.enteringRoomName:
                    multiplayerControlsPanel.SetActive(true);
                    break;
                case Step.connectingToRoom:
                    multiplayerConnectionPanel.SetActive(true);
                    connectionStatusText.text = "Connecting";
                    break;
                case Step.selectingLevel:
                    if (PhotonNetwork.IsMasterClient)
                    {
                        // Show dem level select controls
                        Debug.Log("Should be showing level select controls now. :/");
                        _mainMenuController.ShowLevelSelectPanelForMultiplayer();
                    }
                    else
                    {
                        // Show em the screen that says "Host selectin da level."
                        multiplayerConnectionPanel.SetActive(true);
                        connectionStatusText.text = "Host is selecting a level.";
                    }

                    break;
                case Step.selectingDog:
                    // Show em the dog screen.
                    dogSelectPanel.SetActive(true);
                    if (!PhotonNetwork.IsMasterClient)
                    {
                        dogSelectNavigationControls.SetActive(false);
                    }
                    else
                    {
                        dogSelectNavigationControls.SetActive(true);
                        // Reset controls on this screen.
                        _mainMenuController.userToDogMap = new[] {-1, -1, -1};
                        foreach (var dogButton in _mainMenuController.dogButtons)
                        {
                            dogButton.interactable = true;
                        }

                        dogSelectPlayButton.interactable = false;
                    }

                    break;
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting && PhotonNetwork.IsMasterClient)
            {
                stream.SendNext(gameStartSequenceStep);
                // Validate userToDogMap
                var hashMap = new HashSet<int>();
                var map = _mainMenuController.userToDogMap;
                for (var index = 0; index < map.Length; index++)
                {
                    var i = map[index];
                    if (!hashMap.Add(i))
                    {
                        // Duplicate found
                        map[index] = -1;
                    }
                }

                stream.SendNext(map);
            }
            else if (stream.IsReading)
            {
                gameStartSequenceStep = (Step) stream.ReceiveNext();
                DisplayMenuForStep(gameStartSequenceStep);
                var map = (int[]) stream.ReceiveNext();
                var inactiveButtons = 0;
                for (var i = 0; i < map.Length; i++)
                {
                    if (map[i] != -1)
                    {
                        _mainMenuController.dogButtons[i].interactable = false;
                        inactiveButtons += 1;
                        if (map[i] == PhotonNetwork.LocalPlayer.ActorNumber)
                        {
                            MainMenuController.defaultDog = i;
                        }
                    }
                }

                dogSelectPlayButton.interactable = inactiveButtons == PhotonNetwork.PlayerList.Length;
                _mainMenuController.userToDogMap = map;
            }
        }
    }
}