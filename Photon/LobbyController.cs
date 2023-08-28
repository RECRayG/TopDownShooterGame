using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class LobbyController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private string versionName = "0.1";
    [SerializeField]
    public static int MAX_PLAYERS = 5;
    [SerializeField]
    private string gameLevelName;

    [SerializeField]
    private InputField createGameInput;
    [SerializeField]
    private InputField joinGameInput;

    [SerializeField]
    private GameObject errorMessagePanel;
    [SerializeField]
    private TextMeshProUGUI errorMessageText;

    [SerializeField]
    private GameObject lobbyPanel;

    private class ErrorMessages
    {
        public const string WRONG_ROOM_NAME = "Комнаты с таким именем не существует";
        public const string UNKNOWN_ERROR = "Непредвиденная ошибка. Попробуйте позже";
        public const string EXIST_ROOM_NAME = "Комната с таким именем уже существует";
        public const string SERVER_SHUTDOWN = "Сервер отключён...";
        public const string NULL_TEXT = "Вы не ввели имя комнаты";
        public const string NULL_ROOMS = "Не создано ни одной комнаты";
        public const string GAME_CLOSED = "Игра закрылась при подключении";
    }

    private RoomOptions roomOptions;
    private List<RoomInfo> roomsInfo;

    private void Awake()
    {
        roomsInfo = new List<RoomInfo>();
        roomOptions = new RoomOptions() { MaxPlayers = MAX_PLAYERS, IsVisible = true, IsOpen = true };
    }

    public void CreateGame()
    {
        if(!PhotonNetwork.IsConnected)
        {
            return;
        }

        if (createGameInput.text == null || createGameInput.text.Equals(""))
        {
            lobbyPanel.SetActive(false);
            errorMessageText.SetText(ErrorMessages.NULL_TEXT);
            errorMessagePanel.SetActive(true);

            return;
        }

        PhotonNetwork.CreateRoom(createGameInput.text, roomOptions, TypedLobby.Default, null);

       /* if(roomsInfo.Find(info => info.Name.Equals(createGameInput.text)) != null)
        {
            lobbyPanel.SetActive(false);
            errorMessageText.SetText(ErrorMessages.EXIST_ROOM_NAME);
            errorMessagePanel.SetActive(true);
        }
        else
        {
            if (!PhotonNetwork.CreateRoom(createGameInput.text, roomOptions, TypedLobby.Default, null))
            {
                lobbyPanel.SetActive(false);
                errorMessageText.SetText(ErrorMessages.UNKNOWN_ERROR);
                errorMessagePanel.SetActive(true);
            }
        }*/
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        if(returnCode == 32766)
        {
            lobbyPanel.SetActive(false);
            errorMessageText.SetText(ErrorMessages.EXIST_ROOM_NAME);
            errorMessagePanel.SetActive(true);
        }
        else
        {
            lobbyPanel.SetActive(false);
            errorMessageText.SetText(ErrorMessages.UNKNOWN_ERROR);
            errorMessagePanel.SetActive(true);
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        this.roomsInfo = roomList;
    }

    public void JoinGame()
    {
        if (!PhotonNetwork.IsConnected)
        {
            return;
        }

        if(joinGameInput.text == null || joinGameInput.text.Equals(""))
        {
            if(roomsInfo.Count == 0)
            {
                lobbyPanel.SetActive(false);
                errorMessageText.SetText(ErrorMessages.NULL_ROOMS);
                errorMessagePanel.SetActive(true);
            }
            else
            {
                PhotonNetwork.JoinRandomRoom();
            }

            return;
        }

        PhotonNetwork.JoinRoom(joinGameInput.text);

        // Если нет созданной комнаты с таким именем, то выводим сообщение об этом
        /*if (roomsInfo.Find(info => info.Name.Equals(joinGameInput.text)) == null)
        {
            lobbyPanel.SetActive(false);
            errorMessageText.SetText(ErrorMessages.WRONG_ROOM_NAME);
            errorMessagePanel.SetActive(true);
        }
        else
        {
            PhotonNetwork.JoinRoom(joinGameInput.text);
        }*/

        /*Debug.Log("Rooms: \n");
        Array.ForEach(PhotonNetwork.GetRoomList(), info => Debug.Log(info.Name + "\n"));
        Debug.Log("\n");*/

        // Если нет созданной комнаты с таким именем, то выводим сообщение об этом
        /*if (Array.Find(PhotonNetwork.GetRoomList(),
                        info => info.Equals(joinGameInput.text)) == null)
        {
            lobbyPanel.SetActive(false);
            errorMessageText.SetText(ErrorMessages.WRONG_ROOM_NAME);
            errorMessagePanel.SetActive(true);
        }
        else
        {
            PhotonNetwork.JoinRoom(joinGameInput.text);
        }*/
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel(gameLevelName);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if(returnCode == 32758)
        {
            lobbyPanel.SetActive(false);
            errorMessageText.SetText(ErrorMessages.WRONG_ROOM_NAME);
            errorMessagePanel.SetActive(true);
        }
        else if(returnCode == 32764)
        {
            lobbyPanel.SetActive(false);
            errorMessageText.SetText(ErrorMessages.GAME_CLOSED);
            errorMessagePanel.SetActive(true);
        }
        else
        {
            lobbyPanel.SetActive(false);
            errorMessageText.SetText(ErrorMessages.UNKNOWN_ERROR);
            Debug.Log(returnCode + ": " + message);
            errorMessagePanel.SetActive(true);
        }
    }
}
