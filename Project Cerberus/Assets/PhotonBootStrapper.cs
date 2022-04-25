/*
 * PhotonBootStrapper is instantiated by GameManager of the master client. It's purpose is to sync the viewId of all
 * client's GameManagers, so that RPC works properly. This process is repeated every level change.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhotonBootStrapper : MonoBehaviourPunCallbacks, IPunObservable
{
    public GameManager gameManager;
    public int gameManagerViewId;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (PhotonNetwork.IsMasterClient && stream.IsWriting)
        {
            gameManagerViewId = gameManager.photonView.ViewID;
            stream.SendNext(gameManagerViewId);
        }
        else
        {
            var newGameManagerViewId = (int)stream.ReceiveNext();
            if (gameManagerViewId != newGameManagerViewId)
            {
                gameManagerViewId = newGameManagerViewId;
                gameManager.photonView.ViewID = newGameManagerViewId;
            }
        }
    }
}
