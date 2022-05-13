/*
 * This file contains all the RPC calls game manager can send/receive.
 */
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;

public partial class GameManager
{
    public void SendRPCEnqueueCommand(Cerberus.CerberusCommand command)
        {
            if (PhotonNetwork.InRoom)
            {
                photonView.RPC(nameof(RPCEnqueueCommand), RpcTarget.All, command);
            }
            else
            {
                RPCEnqueueCommand(command);
            }
        }
    
        [PunRPC]
        public void RPCEnqueueCommand(Cerberus.CerberusCommand command)
        {
            _commandQueue.Enqueue(command);
        }
    
        public void SendRPCSyncBoard(StateData[] objectArray)
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(RPCSyncBoard), RpcTarget.Others, objectArray, timer, move, currentLevel);
            }
            // No reason to sync board if there's only one client.
        }
    
        [PunRPC]
        public void RPCSyncBoard(StateData[] stateDatas, float pTimer, int pMove, int pCurrentLevel)
        {
            _puzzleContainer.SyncBoardWithData(stateDatas);
            timer = pTimer;
            move = pMove;
            currentLevel = pCurrentLevel;
        }
    
        public void ValidateAndSendRPCSyncCerberusMap()
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
            {
                // Validate cerberusToUserMap.
                var duplicate = cerberusToUserMap.GroupBy(x => x).Any(g => g.Count() > 1);
                if (duplicate)
                {
                    // Reset cerberus map.
                    cerberusToUserMap = new List<int>() {-1, -1, -1};
                    var playerList = PhotonNetwork.PlayerList;
                    for (int i = 0; i < playerList.Length; i++)
                    {
                        cerberusToUserMap[i] = playerList[i].ActorNumber;
                    }
                }
    
                photonView.RPC(nameof(RPCSyncCerberusMap), RpcTarget.All, cerberusToUserMap.ToArray());
            }
            // No reason to sync board if there's only one client.
        }
    
        [PunRPC]
        public void RPCSyncCerberusMap(int[] map)
        {
            cerberusToUserMap = new List<int>(map);
            // Ensure correct cerberus is possessed.
            currentCerberus = cerberusFormed
                ? _cerberusMajor
                : availableCerberus[cerberusToUserMap.IndexOf(PhotonNetwork.LocalPlayer.ActorNumber)];
            UpdateDogTrackers();
        }
    
        public void SendRPCReplayLevel()
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(RPCReplayLevel), RpcTarget.Others);
            }
        }
    
        [PunRPC]
        public void RPCReplayLevel()
        {
            ReplayLevel();
            DestroyEndcardUI();
        }
    
        public void SendRPCUndoLastMove()
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(RPCUndoLastMove), RpcTarget.Others);
            }
        }
    
        [PunRPC]
        public void RPCUndoLastMove()
        {
            UndoLastMove();
            DestroyEndcardUI();
        }
    
        public void DestroyEndcardUI()
        {
            // Destroy endcard UI that may or may not be present.
            var puzzleUIEndCardSuccess = FindObjectOfType<PuzzleUIEndCardSuccess>();
            if (puzzleUIEndCardSuccess != null)
            {
                Destroy(puzzleUIEndCardSuccess);
            }
    
            var puzzleUIEndCardFailure = FindObjectOfType<PuzzleUIEndCardFailure>();
            if (puzzleUIEndCardFailure != null)
            {
                Destroy(puzzleUIEndCardFailure);
            }
        }
    
        public void SendRPCCycleCharacter(int oldDog, int newDog)
        {
            if (oldDog == newDog) return;
            if (PhotonNetwork.InRoom)
            {
                photonView.RPC(nameof(RPCCycleCharacter), RpcTarget.All, oldDog, newDog,
                    PhotonNetwork.LocalPlayer.ActorNumber);
            }
        }
    
        [PunRPC]
        public void RPCCycleCharacter(int oldDog, int newDog, int actorId)
        {
            // Swap old and new.
            if (oldDog != -1)
            {
                cerberusToUserMap[oldDog] = cerberusToUserMap[newDog];
            }
    
            cerberusToUserMap[newDog] = actorId;
            // Validate my idxOfLastControlledDog.
            idxOfLastControlledDog = cerberusToUserMap.IndexOf(PhotonNetwork.LocalPlayer.ActorNumber);
            UpdateDogTrackers();
        }
}
