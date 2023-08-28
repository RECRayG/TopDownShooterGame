using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private string lobbySceneName = "Lobby";
    [SerializeField]
    private string regionName = "ru";

    private void Start()
    {
        //PhotonNetwork.SerializationRate = 15;
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.ConnectToRegion(regionName);
    }

    public override void OnConnectedToMaster()
    {
        if(!PhotonNetwork.InLobby)
            PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        SceneManager.LoadScene(lobbySceneName);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log(cause);
    }
}
