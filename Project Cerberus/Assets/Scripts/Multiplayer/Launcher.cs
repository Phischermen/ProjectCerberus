using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Multiplayer
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        string gameVersion = "1";
        public static byte maxPlayersPerRoom = 3;

        public GameObject multiplayerConnectionPanel;
        public GameObject multiplayerControlsPanel;

        public bool isConnecting;

        private void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            multiplayerControlsPanel.SetActive(true);
            multiplayerConnectionPanel.SetActive(false);
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
            multiplayerControlsPanel.SetActive(true);
            multiplayerConnectionPanel.SetActive(false);
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
            // TODO Let host select a level. Don't imediately load a level.
            PhotonNetwork.LoadLevel((int) Scenum.Scene.BasicsTutorial_1_K);
        }

        private void Connect()
        {
            multiplayerControlsPanel.SetActive(false);
            multiplayerConnectionPanel.SetActive(true);
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
    }
}